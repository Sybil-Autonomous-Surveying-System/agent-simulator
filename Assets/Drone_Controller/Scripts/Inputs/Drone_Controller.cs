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
        public static bool destinationReached = true;

        private Vector2 xzcoordStart;
        private Vector2 xzcoordEnd;
        // Start is called before the first frame update
        void Start()
        {
            input = GetComponent<Drone_Inputs>();
            engines = GetComponentsInChildren<Engine>().ToList<Engine>();
            // myCamera = GameObject.FindWithTag("drone_cam").GetComponent<Camera>();
            myCamera = Camera.main;
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
                float angle = Quaternion.Angle(rb.rotation, _lookRotation);
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
                    input.Throttle = 0;
                    if (angle != 0 && softBlock)
                    {
                        rb.angularVelocity = Vector3.zero;
                        rb.velocity = Vector3.zero;
                        rb.rotation = (Quaternion.Slerp(rb.rotation, _lookRotation, Time.deltaTime * 0.8f));
                    }
                    else
                    { 
                        if (softBlock)
                        {
                            rb.angularVelocity = Vector3.zero;
                            rb.velocity = Vector3.zero;
                            softBlock = false;
                        }
                        if ((MathUtility.IsBetweenRange(Mathf.Abs(rb.position.x), Mathf.Abs(destination.x-0.5f), Mathf.Abs(destination.x + 0.5f))) 
                            && MathUtility.IsBetweenRange(Mathf.Abs(rb.position.z), Mathf.Abs(destination.z - 0.5f), Mathf.Abs(destination.z + 0.5f))) 
                        {
                            //rb.freezeRotation = true;
                            rb.angularVelocity = Vector3.zero;
                            rb.velocity = Vector3.zero;
                            input.Cyclic = new Vector2(0, 0);
                            destinationReached = true;
                        }
                        else
                        {
                            input.Cyclic = new Vector2(0, Mathf.Lerp(0.25f, (new Vector2(rb.position.x, rb.position.z) - xzcoordEnd).magnitude / xzcoordEnd.magnitude, Time.deltaTime * 50));
                        }
                    }

                }

            }
            else
            {
                if (UDP_Connect.cqDestinations != null)
                {
                    if (UDP_Connect.cqDestinations.TryDequeue(out destination))
                    {
                        destinationReached = false;
                        softBlock = true;
                        xzcoordStart = new Vector2(rb.position.x, rb.position.z);
                        xzcoordEnd = new Vector2(destination.x, destination.z);
                        destinationBool = true;
                        rb.freezeRotation = false;
                    }
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
            Quaternion rot = Quaternion.Euler(finalPitch, rb.rotation.eulerAngles.y, finalRoll);
            //torque instead but need to clamp it to prevent complete flip
            rb.MoveRotation(rot);
        }


        
    }
}