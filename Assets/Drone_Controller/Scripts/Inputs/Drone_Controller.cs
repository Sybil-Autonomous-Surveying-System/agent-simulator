using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
// UDP addition
using System.Net;
using System.Net.Sockets;

using System.Text;
//Multithreading
using System.Threading;

//Json Library
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
    
namespace Sybil
{
    [RequireComponent(typeof(PlayerInput))]
    public class Drone_Controller : Base_Rigidbody
    {
        [Header("Control Properties")]
        [SerializeField] private float minMaxPitch = 30F;
        [SerializeField] private float minMaxRoll = 30F;
        [SerializeField] private float yawPower = 4F;
        [SerializeField] private float lerpSpeed = 2f;
        [SerializeField] private Camera_SaveShot cameraScreenShot;

        JsonVector desiredLocation;

        private Drone_Inputs input;
        private List<Engine> engines = new List<Engine>();
        private Camera myCamera;

        private float finalPitch;
        private float finalRoll;
        private float finalYaw;
        private float yaw;


        //UDP
        private string ip;
        public IPAddress address;
        private Thread thread;
        private UdpClient socket;
        private IPEndPoint ipAddressEndPoint;
        private byte[] data;
        private string dataString;

        private ConcurrentQueue<Vector3> cq;

        private Vector3 destination;
        private bool destinationBool = false;
        private bool softBlock = true;
        private bool destinationReached = true;
        // Start is called before the first frame update
        void Start()
        {
            input = GetComponent<Drone_Inputs>();
            engines = GetComponentsInChildren<Engine>().ToList<Engine>();
            // myCamera = GameObject.FindWithTag("drone_cam").GetComponent<Camera>();
            myCamera = Camera.main;



            //UDP
            cq = new ConcurrentQueue<Vector3>();

            ipAddressEndPoint = new IPEndPoint(IPAddress.Any, 8080);

            socket = new UdpClient(ipAddressEndPoint);
            thread = new Thread(performMovement);
            thread.Start();
        }

        // Update is called once per frame
        void Update()
        {

        }
        protected override void HandlePhysics()
        {
            directedControlOrchestrator();
            HandleEngines();
            HandleControls();
        }
        protected override void HandleOther()
        {
            
            //if (input.CameraThrottle==1)
            //{
            //    cameraScreenShot.TakeScreenshot(myCamera, 1291, 543);
            //    //reset value
            //    input.CameraThrottle = 0;
                

            //};
            
        }
        protected virtual void HandleEngines()
        {
            foreach(Engine engine in engines)
            {
                engine.UpdateEngine(rb, input);
            }
            
        }

        // based on x,y,z coordinate to move to that specific location
        void directedControlOrchestrator()
        {
            
            if (destinationBool && !destinationReached)
            {
                Vector3 _direction = (new Vector3(destination.x,0, destination.z) - new Vector3 (rb.position.x,0,rb.position.z)).normalized;
                Quaternion _lookRotation = Quaternion.LookRotation(_direction);
                if ((int)rb.position.y < destination.y)
                {
                    input.Throttle = 1;
                }
                else if ((int)rb.position.y > destination.y)
                {
                    input.Throttle = -0.5f;
                }
                else if ((int)rb.position.y == destination.y)
                {
                    if (softBlock|| ((int)rb.position.x == destination.x) || ((int)rb.position.z == destination.z))
                    {
                        rb.angularVelocity = Vector3.zero;
                        rb.velocity = Vector3.zero;
                        softBlock = false;
                        input.Cyclic = new Vector2(0, 0);
                        input.Pedals = 0;
                    }
                    input.Throttle = 0;
                    if (_lookRotation.y > (rb.rotation.y + .005f))
                    {
                        input.Pedals = .3f;
                    }
                    else if (_lookRotation.y < (rb.rotation.y - .005f))
                    {
                        input.Pedals = -.3f;
                    }
                    else
                    {
                        input.Pedals = 0;
                        if (((int)rb.position.x == destination.x) && ((int)rb.position.z == destination.z)) 
                        {
                            rb.angularVelocity = Vector3.zero;
                            rb.velocity = Vector3.zero;
                            input.Cyclic = new Vector2(0, 0);
                            destinationReached = true;
                        }
                        else
                        {
                            input.Cyclic = new Vector2(0, 1);
                        }
                    }
                }

            }
            else
            {
                if (cq.TryDequeue(out destination))
                {
                    destinationReached = false;
                    Debug.Log(destination);
                }
            }
        }

        protected virtual void HandleControls()
        {
            //Y
            float pitch = input.Cyclic.y * minMaxPitch;
            //X made negative
            float roll = -input.Cyclic.x * minMaxRoll;
            yaw += input.Pedals * yawPower;
            finalPitch = Mathf.Lerp(finalPitch, pitch, Time.deltaTime * lerpSpeed);
            finalRoll = Mathf.Lerp(finalRoll, roll, Time.deltaTime * lerpSpeed);
            finalYaw = Mathf.Lerp(finalYaw, yaw, Time.deltaTime * lerpSpeed);
            Quaternion rot = Quaternion.Euler(finalPitch, finalYaw, finalRoll);
            //torque instead but need to clamp it to prevent complete flip
            rb.MoveRotation(rot);
        }


        void performMovement()
        {
            while (true)
            {
                Debug.Log("does continue to ruyn after destination chosen");
                try
                {
                    data = socket.Receive(ref ipAddressEndPoint);
                    dataString = Encoding.UTF8.GetString(data);

                    JsonVector jsonvector = JsonConvert.DeserializeObject<JsonVector>(dataString);
                    //input.Cyclic = new Vector2(input.Cyclic.x,1);
                    //
                    //List<string> jsonToArray = JsonConvert.DeserializeObject<List<string>>(dataString);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(dataString);

                    //List<string> vector = jsonData["vector"][0].Value<List<string>>();
                    Vector3 enqueuedestination = new Vector3(jsonData["vector"][0], jsonData["vector"][1], jsonData["vector"][2]);
                    destinationBool = true;
                    cq.Enqueue(enqueuedestination);
                    Debug.Log(enqueuedestination);
                }
                catch (SocketException ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
    }
}