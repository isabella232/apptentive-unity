using System;
using UnityEngine;
using UnityEngine.UI;

using ApptentiveSDK;

public class UIHandler : MonoBehaviour
{
    [SerializeField]
    Text m_unreadMessagesText;

    private void Start()
    {
        Apptentive.RegisterUnreadMessageDelegate(delegate(int unreadMesssages) {
            m_unreadMessagesText.text = "Unread message count: " + unreadMesssages;
        });
        m_unreadMessagesText.text = "Unread message count: " + Apptentive.UnreadMessageCount;
    }

    public void OnLoveDialogButton()
    {
        Apptentive.Engage("love_dialog", CreateBooleanCallback("Love Dialog"));
    }

    public void OnSurveyButton()
    {
        Apptentive.Engage("survey", CreateBooleanCallback("Survey"));
    }

    public void OnMessageCenterButton()
    {
        Apptentive.PresentMessageCenter(CreateBooleanCallback("Message Center"));
    }

    static Action<bool> CreateBooleanCallback(string title, string message = null) {
        return delegate (bool succesful)
        {
            if (!succesful)
            {
                MNPopup popup = new MNPopup(title, message != null ? message : (title + " was not engaged"));
                popup.AddAction("Close", () => { });
                popup.Show();
            }
        };
    }
}
