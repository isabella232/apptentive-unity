using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApptentiveConnect;

public class UIHandler : MonoBehaviour
{
    public void OnEngageButton()
    {
        Apptentive.sharedConnection.Engage("test_event");
    }

    public void OnMessageCenterButton()
    {
        Apptentive.sharedConnection.PresentMessageCenter();
    }
}
