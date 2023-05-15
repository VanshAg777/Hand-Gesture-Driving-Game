using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.Data.Value.Public
{

    /// <summary></summary>
    [System.Serializable]
    public struct WheelTorque : System.IEquatable<WheelTorque>
    {
        /// <summary>Front left wheel</summary>
[Tooltip("Front left wheel")]
        public float fL;
        /// <summary>Front right wheel</summary>
[Tooltip("Front right wheel")]
        public float fR;
        /// <summary>Rear left wheel</summary>
[Tooltip("Rear left wheel")]
        public float rL;
        /// <summary>Front left wheel</summary>
[Tooltip("Front left wheel")]
        public float rR;

        public bool Equals(WheelTorque other)
        {
            return
                fL == other.fL &&
                fR == other.fR &&
                rL == other.rL &&
                rR == other.rR;
        }
    }

}
