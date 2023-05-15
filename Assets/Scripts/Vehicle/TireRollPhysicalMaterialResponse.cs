using com.unity.testtrack.Data;
using com.unity.testtrack.physics;
using com.unity.testtrack.terrainsystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.unity.testtrack.vehicle
{
    public class TireRollPhysicalMaterialResponse : MonoBehaviour
    {
        [SerializeField] AudioSource                    m_audioSourceTemplate;
        [SerializeField] CollidingPhysicalMaterial      m_collidingMaterial = default;
        [SerializeField] VolvoCars.Data.WheelVelocity   m_wheelVelocity = default;
        [Range(0, 1000)]
        [SerializeField] float                          m_volumeAt0WheelVelocity = 0;
        [Range(0, 1000)]
        [SerializeField] float                          m_volumeAt100WheelVelocity = 920;

        private AudioClip                   m_currentAudioClip;
        private Stack<PhysicalProperty>     m_propertyChangeRequests = new Stack<PhysicalProperty>();
        static AudioClip                    s_nextAudioClip = null;

        private void OnEnable()
        {
            m_currentAudioClip = null;
            m_collidingMaterial?.Subscribe(OnPhysicalMaterialChange);
        }

        private void OnDisable()
        {
            m_propertyChangeRequests.Clear();
            m_collidingMaterial?.Unsubscribe(OnPhysicalMaterialChange);
            m_currentAudioClip = null;
        }

        void Remap_float(float In, Vector2 InMinMax, Vector2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        /// <summary>
        /// Fade out audio sounds to avoid crackles on sound change
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clip"></param>
        /// <param name="fadeTime"></param>
        /// <returns></returns>
        static IEnumerator SwitchAudioClip(AudioSource source, AudioClip clip, float fadeTime = 0.2f)
        {
            float startVolume = source.volume;

            while (source.volume > 0)
            {
                source.volume -= (Time.deltaTime / fadeTime);
                yield return null;
            }

            source.volume = 0;
            yield return new WaitForSeconds(0.05f); //Wait a bit before stopping the sound so the volume has actually registered by the audio system
            source.Stop();
            source.clip = clip;
            source.volume = startVolume;
            source.Play();
            s_nextAudioClip = null;
        }

        private void FixedUpdate()
        {
            if (s_nextAudioClip == null && m_currentAudioClip != null) //Not in a transition
            {
                float vl = Mathf.Clamp(GetAverageWheelVelocityValue(), 0, 1000);
                Remap_float(vl, new Vector2(m_volumeAt0WheelVelocity, m_volumeAt100WheelVelocity), new Vector2(0, 100), out var relativeSpeed);
                var vol = relativeSpeed <= 0 ? 0 : relativeSpeed / 100;
                m_audioSourceTemplate.volume = Mathf.Clamp(vol, 0, 1.0f);
            }

            if (s_nextAudioClip == null && m_propertyChangeRequests.TryPop(out var newProp))
            {
                var vfxInfo = GetVfxInfo(PhysicalVFXInfo.Type.TireRoll, newProp);
                if (m_currentAudioClip != vfxInfo?.m_sound)
                {
                    s_nextAudioClip = vfxInfo?.m_sound;
                    StartCoroutine(SwitchAudioClip(m_audioSourceTemplate, s_nextAudioClip, 0.1f));
                    m_currentAudioClip = s_nextAudioClip;
                }

                m_propertyChangeRequests.Clear();
            }
        }

        /// <summary>
        /// Listen to any physical material changes and push them on a stack that will be processed in the FixedUpdate
        /// </summary>
        /// <param name="physicProperty"></param>
        void OnPhysicalMaterialChange(WheelPhysicalProperty physicProperty)
        {
            m_propertyChangeRequests.Push(GetWheelAveragePhysicalPropertyValue(physicProperty));
        }

        PhysicalVFXInfo GetVfxInfo(PhysicalVFXInfo.Type type, PhysicalProperty physicalProperty)
        {
            if (physicalProperty == null || physicalProperty.m_VFXInfos == null || physicalProperty.m_VFXInfos.Count == 0)
                return null;

            return physicalProperty.m_VFXInfos.FirstOrDefault(prop => prop.m_type == type);
        }

        float GetAverageWheelVelocityValue()
        {
            float cumulativeVelocity = m_wheelVelocity.Value.fL;
            cumulativeVelocity += m_wheelVelocity.Value.fR;
            cumulativeVelocity += m_wheelVelocity.Value.rL;
            cumulativeVelocity += m_wheelVelocity.Value.rR;

            return cumulativeVelocity / 4;
        }

        PhysicalProperty m_lastPhysicalProp = null;
        PhysicalProperty GetWheelAveragePhysicalPropertyValue(WheelPhysicalProperty physicProperty)
        {
            Dictionary<PhysicalProperty, int> propertyCounts = new Dictionary<PhysicalProperty, int>();

            System.Action<PhysicalProperty> AddPropertyToCount = (physProp) =>
            {
                if (physProp != null)
                {
                    if (!propertyCounts.ContainsKey(physProp))
                        propertyCounts.Add(physProp, 0);

                    ++propertyCounts[physProp];
                }
            };
            AddPropertyToCount(physicProperty.fL);
            AddPropertyToCount(physicProperty.fR);
            AddPropertyToCount(physicProperty.rL);
            AddPropertyToCount(physicProperty.rR);

            if (propertyCounts.Count == 0)
                return null;

            if (m_lastPhysicalProp != null && propertyCounts.ContainsKey(m_lastPhysicalProp) && propertyCounts[m_lastPhysicalProp] > 1)
                return m_lastPhysicalProp;

            var ordered = propertyCounts.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            m_lastPhysicalProp = ordered.LastOrDefault().Key;
            return m_lastPhysicalProp;
        }
    }
}

