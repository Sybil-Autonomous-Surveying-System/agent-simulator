using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sybil
{
    [RequireComponent(typeof(Rigidbody))]
    public class Base_Rigidbody : MonoBehaviour
    {
        [Header("Rigidbody Properties")]
        [SerializeField] private float weightkgs = 0.45f;

        protected Rigidbody rb;
        protected float startDrag;
        protected float startAngularDrag;

        // Awake is called before the first frame update
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.mass = weightkgs;
                startDrag = rb.drag;
                startAngularDrag = rb.angularDrag;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (!rb)
            {
                return;
            }
            HandlePhysics();
            HandleOther();
        }
        protected virtual void HandlePhysics() { }
        protected virtual void HandleOther() { }
    }
}
