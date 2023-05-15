using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.Data.Value.Public
{

    /// <summary></summary>
    [System.Serializable]
    public struct LampGeneral : System.IEquatable<LampGeneral>
    {
        /// <summary>Target light intensity. Min: 0, max: 1.</summary>
[Tooltip("Target light intensity. Min: 0, max: 1.")]
        public float intensity;
        /// <summary>The duration of the animation</summary>
[Tooltip("The duration of the animation.")]
        public float duration;
        /// <summary>Target red color component, between 0-1.</summary>
[Tooltip("Target red color component, between 0-1.")]
        public float r;
        /// <summary>Target green color component, between 0-1.</summary>
[Tooltip("Target green color component, between 0-1.")]
        public float g;
        /// <summary>Target blue color component, between 0-1.</summary>
[Tooltip("Target blue color component, between 0-1.")]
        public float b;
        /// <summary>How the light should go from its current state to the target values.</summary>
[Tooltip("How the light should go from its current state to the target values.")]
        public int profile;

        public bool Equals(LampGeneral other)
        {
            return
                intensity == other.intensity &&
                duration == other.duration &&
                r == other.r &&
                g == other.g &&
                b == other.b &&
                profile == other.profile;
        }
    }

}
