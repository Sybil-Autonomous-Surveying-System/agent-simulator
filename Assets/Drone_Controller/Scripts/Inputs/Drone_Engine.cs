using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sybil
{ 
    [RequireComponent(typeof(BoxCollider))]
    public class Drone_Engine : MonoBehaviour, Engine
    {
        [Header("Engine Properties")]
        [SerializeField] private float maxPower = 4f;

        [Header("Propeller Propterties")]
        [SerializeField] private Transform propeller;
        [SerializeField] private float propRotSpeed = 300f;

        // make drone still after amount of frames of inactivity
        private int ticks = 0;
        public void InitEngine()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateEngine(Rigidbody rb, Drone_Inputs input)
        {
            // to help it float 
            Vector3 upVector = transform.up;
            upVector.x = 0f;
            upVector.z = 0f;
            float diff = 1 - upVector.magnitude;
            float finalDiff = Physics.gravity.magnitude * diff;

            Vector3 engineForce = Vector3.zero;
            engineForce = transform.up * ((rb.mass * Physics.gravity.magnitude + finalDiff)  + (input.Throttle * maxPower))/4f;
            rb.AddForce(engineForce, ForceMode.Force);

            // induce psuedo drag 
            if (input.Throttle == 0)
            {
                ticks++;
                if (ticks == 600)
                {
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                ticks = 0;
            }
            HandlePropellers();
        }
        void HandlePropellers()
        {
            if (!propeller)
            {
                return;
            }
            propeller.Rotate(Vector3.up, propRotSpeed);
        }
    }
}