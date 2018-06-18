using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApptentiveSDK;

public class UIHandler : MonoBehaviour
{
    public void OnLoveDialogButton()
    {
        Apptentive.Engage("love_dialog");
    }

    public void OnSurveyButton()
    {
        Apptentive.Engage("survey");
    }

    public void OnMessageCenterButton()
    {
        Apptentive.PresentMessageCenter();
    }
}
