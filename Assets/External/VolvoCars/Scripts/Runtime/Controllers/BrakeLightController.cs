using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.Behaviour {
    public class BrakeLightController : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> meshRenderers;
        [SerializeField] private Color emissiveColor;
        [SerializeField] private float litMultiplier = 2;
        [SerializeField] private Data.LampBrake lampData;
        

        private void Start()
        {
            foreach (MeshRenderer renderer in meshRenderers) {
                renderer.material.EnableKeyword("_EMISSION");
            }
            lampData.Subscribe(UpdateLight);
        }

        private void UpdateLight(Data.Value.Public.LampGeneral lightValues)
        {
            foreach(MeshRenderer renderer in meshRenderers) {
                renderer.material.SetColor("_EmissiveColor", emissiveColor * lightValues.intensity * litMultiplier);
            }
        }

        private void OnDestroy()
        {
            lampData.Unsubscribe(UpdateLight);
        }


    }

}