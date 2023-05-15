using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpeedAndGear : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool mph = false;

    [Header("References")]
    [SerializeField] private VolvoCars.Data.GearLeverIndication gear = default;
    [SerializeField] private VolvoCars.Data.Velocity velocity = default;
    [SerializeField] private TMPro.TMP_Text gearText;
    [SerializeField] private TMPro.TMP_Text speedText;
    [SerializeField] private TMPro.TMP_Text unityScreenText;
    //private UnityEngine.UI.Text text;

    private string gearString = "";
    Action<int> gearAction;
    Action<float> velocityAction;

    // Start is called before the first frame update
    void Start()
    {

        gearAction = gearInt =>
        {
            switch (gearInt) {
                case 0:
                    gearString = "P";
                    break;
                case 1:
                    gearString = "R";
                    break;
                case 2:
                    gearString = "N";
                    break;
                case 3:
                    gearString = "D";
                    break;
            }
            if (gearText != null)
                gearText.text = gearString;

        };
        gear.Subscribe(gearAction);

        velocityAction = v =>
        {
            if (speedText == null)
                return;

            if (mph) {
                speedText.text = ((int)(2.23694f * Mathf.Abs(v) + 0.9f)).ToString();
            } else {
                speedText.text = ((int)(3.6f * Mathf.Abs(v) + 0.9f)).ToString();
            }
        };
        velocity.Subscribe(velocityAction);


    }

    void Update()
    {
        
        if(unityScreenText != null) {
            float speed = Mathf.Abs(velocity.Value);
            unityScreenText.text = mph ? "mph" : "km/h";
        }
            
    }

    private void OnDestroy()
    {
        gear.Unsubscribe(gearAction);
        velocity.Unsubscribe(velocityAction);
    }

}
