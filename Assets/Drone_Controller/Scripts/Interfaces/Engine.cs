using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sybil
{
    public interface Engine
    {
        void InitEngine();
        void UpdateEngine(Rigidbody rb, Drone_Inputs input);

    }
}
