using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.Data
{
    public class DataSet : ScriptableObject
    {

        [SerializeField]
        private InfoText clarification = new InfoText("Contains data items defining the API.");
        [Tooltip("Information about this data set.")]
        public string info;
        [ReadOnly]
        public GenericData[] items = new GenericData[0];

    }

}
