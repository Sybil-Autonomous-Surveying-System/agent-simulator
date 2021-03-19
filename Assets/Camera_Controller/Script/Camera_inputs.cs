using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Camera_inputs : MonoBehaviour
{
    private float takeScreenshot;
    private float tilt;
    public float Tilt { get => tilt; set => tilt = value; }
    public float TakeScreenshot { get => takeScreenshot; set => takeScreenshot = value; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTakeScreenshot(InputValue value)
    {
        takeScreenshot = value.Get<float>();
    }

    private void OnTilt(InputValue value)
    {
        tilt = value.Get<float>();
    }
}
