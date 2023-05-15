using System.Collections;
using System.Collections.Generic;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomTutorialWelcomePage", menuName = "Tutorials/Custom Tutorial Welcome Page", order = 1)]
public class CustomTutorialWelcomePage : TutorialWelcomePage
{
    public Tutorial m_startTutorial;
    public void StartTutorial()
    {
        if (m_startTutorial)
            TutorialManager.Instance.StartTutorial(m_startTutorial);
    }
}
