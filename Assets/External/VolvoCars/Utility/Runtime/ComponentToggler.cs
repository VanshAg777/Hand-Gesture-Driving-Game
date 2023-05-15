using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentToggler : MonoBehaviour
{

    public MonoBehaviour[] components;
    public MonoBehaviour initiallyActiveComponent;
    public KeyCode toggleKey;

    private int m_currentIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i<components.Length; i++) {
            components[i].enabled = false;
            if(components[i] == initiallyActiveComponent) {
                components[i].enabled = true;
                m_currentIdx = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            components[m_currentIdx].enabled = false;
            m_currentIdx++;
            if (m_currentIdx >= components.Length)
            {
                m_currentIdx = 0;
            }

            components[m_currentIdx].enabled = true;
        }
    }
}
