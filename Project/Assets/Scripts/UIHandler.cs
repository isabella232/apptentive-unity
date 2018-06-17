using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApptentiveSDK;

public class UIHandler : MonoBehaviour
{
    public void OnEngageButton()
    {
        Apptentive.Engage("test_event");
    }

    public void OnMessageCenterButton()
    {
        Apptentive.PresentMessageCenter();
    }
}
