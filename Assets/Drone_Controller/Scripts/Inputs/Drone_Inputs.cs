using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Drone_Inputs : MonoBehaviour
{
    private Vector2 cyclic;
    private float pedals;
    private float throttle;
    private float cameraThrottle;

    public Vector2 Cyclic { get => cyclic; set => cyclic = value; }
    public float Pedals { get => pedals; set => pedals = value; }
    public float Throttle { get => throttle; set => throttle = value; }
    public float CameraThrottle { get => cameraThrottle; set => cameraThrottle=value; }

    private void OnCyclic(InputValue value)
    {
        cyclic = value.Get<Vector2>();
    }
    private void OnThrottle(InputValue value)
    {
        throttle = value.Get<float>();
    }
    private void OnPedals(InputValue value)
    {
        pedals = value.Get<float>();
    }
    private void OnCameraThrottle(InputValue value)
    {
       cameraThrottle = value.Get<float>();
    }
}
