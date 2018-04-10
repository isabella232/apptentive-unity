using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEngine;

using ApptentiveSDKInternal;

namespace ApptentiveSDK
{
    delegate void ApptentiveNativeCallback(string name, string payload);
    delegate void ApptentiveNativeCallbackHandler(IDictionary<string, string> data);

    [Serializable]
    public class ApptentiveConfiguration
    {
        public string appKey;
        public string appSignature;
    }

    public sealed class Apptentive : MonoBehaviour
    {
        static Apptentive s_instance;

        [SerializeField]
        ApptentiveConfiguration m_configuration;

        IPlatform m_platform;

        IDictionary<string, ApptentiveNativeCallbackHandler> m_nativeHandlerLookup;

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
                if (InitPlatform(m_configuration))
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
            if (string.IsNullOrEmpty(m_configuration.appKey))
            {
                Debug.LogWarning("Missing Apptentive App key");
            }
            if (string.IsNullOrEmpty(m_configuration.appSignature))
            {
                Debug.LogWarning("Missing Apptentive App Signature");
            }
        }

        #endregion

        #region Platforms

        bool InitPlatform(ApptentiveConfiguration configuration)
        {
            try
            {
                if (m_platform == null)
                {
                    m_platform = CreatePlatform(configuration);
                    return m_platform != null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Can't init " + Constants.PluginName + ": " + e.Message);
            }

            return false;
        }

        IPlatform CreatePlatform(ApptentiveConfiguration configuration)
        {
            #if UNITY_IOS || UNITY_IPHONE
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ApptentiveNativeCallback callback = NativeMessageCallback;
                return new PlatformIOS(gameObject.name, callback.Method.Name, Constants.Version, configuration);
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
            void Engage(string evt, IDictionary<string, object> customData, Action<Boolean> callback);
            void PresentMessageCenter(IDictionary<string, object> customData, Action<Boolean> callback);
            void CanShowInteraction(string eventName, Action<Boolean> callback);
            void CanShowMessageCenter(Action<Boolean> callback);
        }

        abstract class Platform : IPlatform
        {
            public static readonly IPlatform Null = new NullPlatform();

            readonly IDictionary<int, Delegate> m_callbackLookup;

            public Platform()
            {
                m_callbackLookup = new Dictionary<int, Delegate>();
            }

            public void Engage(string evt, IDictionary<string, object> customData, Action<Boolean> callback)
            {
                int callbackId = Engage(evt, customData);
                RegisterCallback(callbackId, callback);
            }

            public void PresentMessageCenter(IDictionary<string, object> customData, Action<Boolean> callback)
            {
                int callbackId = PresentMessageCenter(customData);
                RegisterCallback(callbackId, callback);
            }

            public void CanShowInteraction(string eventName, Action<Boolean> callback)
            {
                int callbackId = CanShowInteraction(eventName);
                RegisterCallback(callbackId, callback);
            }

            public void CanShowMessageCenter(Action<Boolean> callback)
            {
                int callbackId = CanShowMessageCenter();
                RegisterCallback(callbackId, callback);
            }

            protected abstract int Engage(string evt, IDictionary<string, object> customData);

            protected abstract int PresentMessageCenter(IDictionary<string, object> customData);

            protected abstract int CanShowInteraction(string eventName);

            protected abstract int CanShowMessageCenter();

            void RegisterCallback(int callbackId, Delegate callback)
            {
                if (callback != null)
                {
                    m_callbackLookup[callbackId] = callback; // TODO: check for duplicates
                }
            }
        }

        class NullPlatform : IPlatform
        {
            public void CanShowInteraction(string eventName, Action<bool> callback)
            {
                if (callback != null)
                {
                    callback(false);
                }
            }

            public void CanShowMessageCenter(Action<bool> callback)
            {
                if (callback != null)
                {
                    callback(false);
                }
            }

            public void Engage(string evt, IDictionary<string, object> customData, Action<bool> callback)
            {
                if (callback != null)
                {
                    callback(false);
                }
            }

            public void PresentMessageCenter(IDictionary<string, object> customData, Action<bool> callback)
            {
                if (callback != null)
                {
                    callback(false);
                }
            }
        }

        #if UNITY_IOS || UNITY_IPHONE

        class PlatformIOS : Platform
        {
            [DllImport("__Internal")]
            private static extern void __apptentive_initialize(string targetName, string methodName, string version, string configuration);

            [DllImport("__Internal")]
            private static extern int __apptentive_engage(string eventName, string customData);

            [DllImport("__Internal")]
            private static extern int __apptentive_present_message_center(string customData);

            [DllImport("__Internal")]
            private static extern int __apptentive_can_show_interaction(string customData);

            [DllImport("__Internal")]
            private static extern int __apptentive_can_show_message_center();

            public PlatformIOS(string targetName, string methodName, string version, ApptentiveConfiguration configuration)
            {
                var configurationDict = JsonUtils.ToJson(configuration);
                __apptentive_initialize(targetName, methodName, version, configurationDict);
            }

            protected override int Engage(string evt, IDictionary<string, object> customData)
            {
                return __apptentive_engage(evt, JsonUtils.ToJson(customData));
            }

            protected override int PresentMessageCenter(IDictionary<string, object> customData)
            {
                return __apptentive_present_message_center(JsonUtils.ToJson(customData));
            }

            protected override int CanShowInteraction(string eventName)
            {
                return __apptentive_can_show_interaction(eventName);
            }

            protected override int CanShowMessageCenter()
            {
                return __apptentive_can_show_message_center();
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

        #endregion // Platform

        #region Native callback

        void NativeMessageCallback(string name, string payload)
        {
            ApptentiveNativeCallbackHandler handler;
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

        IDictionary<string, ApptentiveNativeCallbackHandler> nativeHandlerLookup
        {
            get
            {
                if (m_nativeHandlerLookup == null)
                {
                    m_nativeHandlerLookup = new Dictionary<string, ApptentiveNativeCallbackHandler>();
                }

                return m_nativeHandlerLookup;
            }
        }

        #endregion

        #region Public interface

        public void Engage(string evt, Action<Boolean> callback = null, IDictionary<string, object> customData = null)
        {
            m_platform.Engage(evt, customData, callback);
        }

        public void PresentMessageCenter(Action<Boolean> callback = null, IDictionary<string, object> customData = null)
        {
            m_platform.PresentMessageCenter(customData, callback);
        }

        public void CanShowInteraction(string eventName, Action<Boolean> callback)
        {
            if (m_platform != null)
            {
                m_platform.CanShowInteraction(eventName, callback);
            }
        }

        public void CanShowMessageCenter(Action<bool> callback)
        {
            m_platform.CanShowMessageCenter(callback);
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

        IPlatform platform
        {
            get { return m_platform != null ? m_platform : Platform.Null; }
        }

        #endregion
    }
}
