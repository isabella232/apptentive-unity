using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEngine;

using ApptentiveConnectInternal;

namespace ApptentiveConnect
{
    delegate void ApptentiveNativeMessageCallback(string message);
    delegate void ApptentiveNativeMessageHandler(IDictionary<string, string> data);

    public sealed class Apptentive : MonoBehaviour
    {
        static Apptentive s_instance;

        [Tooltip("The API key for Apptentive.\n\n This key is found on the Apptentive website under Settings, API & Development.")]
        [SerializeField]
        string m_APIKey;

        IPlatform m_platform;

        IDictionary<string, ApptentiveNativeMessageHandler> m_nativeHandlerLookup;

        #region Life cycle

        void Awake()
        {
            InitInstance();
        }

        void OnEnable()
        {
            InitInstance();
        }

        void InitInstance()
        {
            if (s_instance == null)
            {
                if (InitPlatform(m_APIKey))
                {
                    s_instance = this;
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else if (s_instance != this)
            {
                Destroy(gameObject);
            }
        }

        void OnValidate()
        {
            if (string.IsNullOrEmpty(m_APIKey))
            {
                Debug.LogWarning("Missing Apptentive API key");
            }
        }

        #endregion

        #region Platforms

        bool InitPlatform(string APIKey)
        {
            try
            {
                if (m_platform == null)
                {
                    m_platform = CreatePlatform(APIKey);
                    return m_platform != null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Can't init " + Constants.PluginName + ": " + e.Message);
            }

            return false;
        }

        IPlatform CreatePlatform(string APIKey)
        {
            #if UNITY_IOS || UNITY_IPHONE
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ApptentiveNativeMessageCallback callback = NativeMessageCallback;
                return new PlatformIOS(gameObject.name, callback.Method.Name, Constants.Version, APIKey);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                ApptentiveNativeMessageCallback callback = NativeMessageCallback;
                return new PlatformAndroid(gameObject.name, callback.Method.Name, Constants.Version, APIKey);
            }
            #endif

            return null;
        }

        interface IPlatform
        {
            bool Engage(string evt, IDictionary<string, object> customData);
            bool PresentMessageCenter(IDictionary<string, object> customData);
            bool CanShowInteraction(string eventName);
            bool CanShowMessageCenter { get; }
        }

        #if UNITY_IOS || UNITY_IPHONE

        class PlatformIOS : IPlatform
        {
            [DllImport("__Internal")]
            private static extern void __apptentive_initialize(string targetName, string methodName, string version, string APIKey);

            [DllImport("__Internal")]
            private static extern bool __apptentive_engage(string eventName, string customData);

            [DllImport("__Internal")]
            private static extern bool __apptentive_present_message_center(string customData);

            [DllImport("__Internal")]
            private static extern bool __apptentive_can_show_interaction(string customData);

            [DllImport("__Internal")]
            private static extern bool __apptentive_can_show_message_center();
            
            /// <summary>
            /// Initializes a new instance of the iOS platform class.
            /// </summary>
            /// <param name="targetName">The name of the game object which will receive native callbacks</param>
            /// <param name="methodName">The method of the game object which will be called from the native code</param>
            /// <param name="version">Plugin version</param>
            /// <param name="APIKey">Apptentive API key</param>
            public PlatformIOS(string targetName, string methodName, string version, string APIKey)
            {
                __apptentive_initialize(targetName, methodName, version, APIKey);
            }

            public bool Engage(string evt, IDictionary<string, object> customData)
            {
                return __apptentive_engage(evt, StringUtils.SerializeString(customData));
            }

            public bool PresentMessageCenter(IDictionary<string, object> customData)
            {
                return __apptentive_present_message_center(StringUtils.SerializeString(customData));
            }

            public bool CanShowInteractionForEvent(string eventName)
            {
                return __apptentive_can_show_interaction(eventName);
            }

            public bool CanShowMessageCenter
            {
                get { return __apptentive_can_show_message_center(); }
            }

            public bool CanShowInteraction(string eventName)
            {
                return __apptentive_can_show_interaction(eventName);
            }
        }

        #elif UNITY_ANDROID

        class PlatformAndroid : IPlatform
        {
            /// <summary>
            /// Initializes a new instance of the Android platform class.
            /// </summary>
            /// <param name="targetName">The name of the game object which will receive native callbacks</param>
            /// <param name="methodName">The method of the game object which will be called from the native code</param>
            /// <param name="version">Plugin version</param>
            /// <param name="APIKey">Apptentive API key</param>
            public PlatformAndroid(string targetName, string methodName, string version, string APIKey)
            {
            }

            public bool Engage(string evt, IDictionary<string, object> customData)
            {
                throw new NotImplementedException();
            }
            public bool PresentMessageCenter(IDictionary<string, object> customData)
            {
                throw new NotImplementedException();
            }
            public bool CanShowInteraction(string eventName)
            {
                throw new NotImplementedException();
            }
            public bool CanShowMessageCenter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        #endif // UNITY_ANDROID

        #endregion

        #region Native callback

        void NativeMessageCallback(string param)
        {
            IDictionary<string, string> data = StringUtils.DeserializeString(param);
            string name = data["name"];
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Can't handle native callback: 'name' is undefined");
                return;
            }

            ApptentiveNativeMessageHandler handler;
            if (!nativeHandlerLookup.TryGetValue(name, out handler))
            {
                Debug.LogError("Can't handle native callback: handler not found '" + name + "'");
                return;
            }

            try
            {
                handler(data);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception while handling native callback (" + name + "): " + e.Message);
            }
        }

        IDictionary<string, ApptentiveNativeMessageHandler> nativeHandlerLookup
        {
            get
            {
                if (m_nativeHandlerLookup == null)
                {
                    m_nativeHandlerLookup = new Dictionary<string, ApptentiveNativeMessageHandler>();
                }

                return m_nativeHandlerLookup;
            }
        }

        #endregion

        #region Public interface

        public bool Engage(string evt, IDictionary<string, object> customData = null)
        {
            return m_platform != null && m_platform.Engage(evt, customData);
        }

        public bool PresentMessageCenter(IDictionary<string, object> customData = null)
        {
            return m_platform != null && m_platform.PresentMessageCenter(customData);
        }

        public bool CanShowInteraction(string eventName)
        {
            return m_platform != null && m_platform.CanShowInteraction(eventName);
        }

        public bool CanShowMessageCenter
        {
            get { return m_platform != null && m_platform.CanShowMessageCenter; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The shared singleton of `Apptentive`
        /// </summary>
        public static Apptentive sharedConnection
        {
            get { return s_instance; } // FIXME: null safety
        }

        /// <summary>
        /// The API key for Apptentive.
        /// This key is found on the Apptentive website under Settings, API & Development.
        /// </summary>
        public String APIKey
        {
            get { return m_APIKey; }
        }

        /// <summary>
        /// Determines if Message Center will be displayed when `presentMessageCenterFromViewController:` is called.
        ///
        /// If app has not yet synced with Apptentive, you will be unable to display Message Center. Use `canShowMessageCenter`
        /// to determine if Message Center is ready to be displayed. If Message Center is not ready you could, for example,
        /// hide the "Message Center" button in your interface.
        /// </summary>
        public bool canShowMessageCenter
        {
            get { return m_platform != null && m_platform.CanShowMessageCenter; }
        }

        #endregion
    }
}
