using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;
using System;
// UDP 
using System.Net;
using System.Net.Sockets;

//Multithreading
using System.Threading;

//Json Library
using Newtonsoft.Json;
public class UDP_Connect : MonoBehaviour
{

    public static ConcurrentQueue<Vector3> cqDestinations;
    public static ConcurrentQueue<Vector3> cqLookAt;
    public static bool takePhoto;
    private string ip;
    public IPAddress address;
    private Thread thread;
    private UdpClient socket;
    private IPEndPoint ipAddressEndPoint;
    private byte[] data;
    private string dataString;

    void getCommands()
    {
        while (true)
        {
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
                if (jsonData.ContainsKey("vector")) { 
                Vector3 enqueuedestination = new Vector3(jsonData["vector"][0], jsonData["vector"][1], jsonData["vector"][2]);
                    cqDestinations.Enqueue(enqueuedestination);
                }
                else if (jsonData.ContainsKey("lookat"))
                {
                    Vector3 enqueulookat = new Vector3(jsonData["lookat"][0], jsonData["lookat"][1], jsonData["lookat"][2]);
                    cqLookAt.Enqueue(enqueulookat);
                }
                else if (jsonData.ContainsKey("photo"))
                {
                    takePhoto = Convert.ToBoolean(jsonData["photo"][0]);
                }
                //cqDestinations.Enqueue(enqueuedestination);
                //Debug.Log(enqueuedestination);
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }
    void Start()
    {
        //UDP
        cqDestinations = new ConcurrentQueue<Vector3>();
        cqLookAt = new ConcurrentQueue<Vector3>();
        ipAddressEndPoint = new IPEndPoint(IPAddress.Any, 8080);

        socket = new UdpClient(ipAddressEndPoint);
        thread = new Thread(getCommands);
        thread.Start();


    }
    

    // Update is called once per frame
    void Update()
    {
       // performMovement();
    }

}
