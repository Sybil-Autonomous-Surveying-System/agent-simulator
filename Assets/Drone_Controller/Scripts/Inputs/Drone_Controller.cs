using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
// UDP addition
using System.Net;
using System.Net.Sockets;
using System.Text;
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


        private Vector3 destination;
        private bool destinationBool = false;
        // Start is called before the first frame update
        void Start()
        {
            input = GetComponent<Drone_Inputs>();
            engines = GetComponentsInChildren<Engine>().ToList<Engine>();
            // myCamera = GameObject.FindWithTag("drone_cam").GetComponent<Camera>();
            myCamera = Camera.main;


            //UDP

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
            if (destinationBool)
            {
                if (rb.position.y < destination.y)
                {
                    input.Throttle = 1;
                }
                else if (rb.position.y >= destination.y)
                {
                    input.Throttle = 0;
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
                try
                {
                    data = socket.Receive(ref ipAddressEndPoint);
                    dataString = Encoding.UTF8.GetString(data);

                    Debug.Log(dataString);
                    JsonVector jsonvector = JsonConvert.DeserializeObject<JsonVector>(dataString);
                    //input.Cyclic = new Vector2(input.Cyclic.x,1);
                    //
                    //List<string> jsonToArray = JsonConvert.DeserializeObject<List<string>>(dataString);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(dataString);
                    Debug.Log(jsonData["vector"]);
                    //List<string> vector = jsonData["vector"][0].Value<List<string>>();
                    destination = new Vector3(jsonData["vector"][0], jsonData["vector"][1], jsonData["vector"][2]);
                    destinationBool = true;
                }
                catch (SocketException ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
    }
}