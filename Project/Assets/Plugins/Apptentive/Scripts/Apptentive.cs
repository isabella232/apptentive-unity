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
    public class ApptentivePlatformConfiguration
    {
        [SerializeField]
        string m_appKey;

        [SerializeField]
        string m_appSignature;

        public string appKey
        {
            get { return m_appKey; }
        }

        public string appSignature
        {
            get { return m_appSignature; }
        }
    }

    [Serializable]
    public enum ApptentiveLogLevel
    {
        Verbose,
        Debug,
        Info,
        Warn,
        Error
    }

    [Serializable]
    public class ApptentiveConfiguration
    {
        [SerializeField]
        ApptentivePlatformConfiguration m_ios;

        [SerializeField]
        ApptentivePlatformConfiguration m_android;

        [SerializeField]
        ApptentiveLogLevel m_logLevel = ApptentiveLogLevel.Info;

        [SerializeField]
        bool m_sanitizeLogMessages = true;

        public string appKey
        {
            get { return currentPlatformConfiguration != null ? currentPlatformConfiguration.appKey : "undefined"; }
        }

        public string appSignature
        {
            get { return currentPlatformConfiguration != null ? currentPlatformConfiguration.appSignature : "undefined"; }
        }

        public ApptentiveLogLevel logLevel
        {
            get { return m_logLevel; }
        }

        public bool sanitizeLogMessages
        {
            get { return m_sanitizeLogMessages; }
        }

        ApptentivePlatformConfiguration currentPlatformConfiguration
        {
            #if UNITY_IOS || UNITY_IPHONE
            get { return m_ios; }
            #elif UNITY_ANDROID
            get { return m_android; }
            #else
            get { return null; }
            #endif
        }
    }

    public sealed class Apptentive : MonoBehaviour
    {
        public static readonly string kVersion = "5.0.0";

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
                ApptentiveNativeCallback callback = NativeMessageCallback;
                return new PlatformAndroid(gameObject.name, callback.Method.Name, kVersion, configuration);
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
            private static readonly string kPluginClassName = "com.apptentive.android.sdk.unity.ApptentiveUnity";
            private readonly object mutex = new object();

            private readonly jvalue[] m_args0 = new jvalue[0];
            private readonly jvalue[] m_args1 = new jvalue[1];
            private readonly jvalue[] m_args2 = new jvalue[2];
            private readonly jvalue[] m_args3 = new jvalue[3];
            private readonly jvalue[] m_args9 = new jvalue[9];

            private readonly AndroidJavaClass m_pluginClass;

            private readonly IntPtr m_pluginClassRaw;
            private readonly IntPtr m_methodShowMessageCenter;
            private readonly IntPtr m_methodCanShowMessageCenter;
            private readonly IntPtr m_methodGetUnreadMessageCount;
            private readonly IntPtr m_methodEngage;
            private readonly IntPtr m_methodQueryCanShowInteraction;
            private readonly IntPtr m_methodSetPersonName;
            private readonly IntPtr m_methodSetPersonEmail;

            /// <summary>
            /// Initializes a new instance of the Android platform class.
            /// </summary>
            /// <param name="targetName">The name of the game object which will receive native callbacks</param>
            /// <param name="methodName">The method of the game object which will be called from the native code</param>
            public PlatformAndroid(string targetName, string methodName, String version, ApptentiveConfiguration configuration)
            {
                m_pluginClass = new AndroidJavaClass(kPluginClassName);
                m_pluginClassRaw = m_pluginClass.GetRawClass();

                // register the plugin
                IntPtr methodRegister = GetStaticMethod(m_pluginClassRaw, "register", "(Ljava.lang.String;Ljava.lang.String;Ljava.lang.String;Ljava.lang.String;)Z");
                var methodRegisterParams = new jvalue[] {
                    jval(targetName),
                    jval(methodName),
                    jval(version),
                    jval(ToJson(configuration))
                };

                var registered = CallStaticBoolMethod(methodRegister, methodRegisterParams);
                if (!registered)
                {
                    throw new Exception("Platform not registered");
                }

                foreach (var param in methodRegisterParams)
                {
                    AndroidJNI.DeleteLocalRef(param.l);
                }

                // register methods
                m_methodShowMessageCenter = GetStaticMethod(m_pluginClassRaw, "showMessageCenter", "(Ljava.lang.String;)I");;
                m_methodCanShowMessageCenter = GetStaticMethod(m_pluginClassRaw, "canShowMessageCenter", "()I"); ;
                m_methodGetUnreadMessageCount = GetStaticMethod(m_pluginClassRaw, "getUnreadMessageCount", "()I"); ;
                m_methodEngage = GetStaticMethod(m_pluginClassRaw, "engage", "(Ljava.lang.String;Ljava.lang.String;)I"); ;
                m_methodQueryCanShowInteraction = GetStaticMethod(m_pluginClassRaw, "queryCanShowInteraction", "(Ljava.lang.String;)I"); ;
                m_methodSetPersonName = GetStaticMethod(m_pluginClassRaw, "setPersonName", "(Ljava.lang.String;)V"); ;
                m_methodSetPersonEmail = GetStaticMethod(m_pluginClassRaw, "setPersonEmail", "(Ljava.lang.String;)V"); ;
            }

            ~PlatformAndroid()
            {
                m_pluginClass.Dispose();
            }

            public void Engage(string evt, IDictionary<string, object> customData, Action<bool> callback)
            {
                lock (mutex)
                {
                    try
                    {
                        m_args2[0] = jval(evt);
                        m_args2[1] = jval(customData != null && customData.Count > 0 ? JsonUtils.ToJson(customData) : "{}");

                        int callbackId = CallStaticIntMethod(m_methodEngage, m_args2);

                        AndroidJNI.DeleteLocalRef(m_args2[0].l);
                        AndroidJNI.DeleteLocalRef(m_args2[1].l);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception while calling 'Apptentive.Engage': " + e.Message);
                        if (callback != null)
                        {
                            callback(false);
                        }
                    }
                }
            }

            public void PresentMessageCenter(IDictionary<string, object> customData, Action<bool> callback)
            {
                throw new NotImplementedException();
            }

            public void CanShowInteraction(string eventName, Action<bool> callback)
            {
                throw new NotImplementedException();
            }

            void IPlatform.CanShowMessageCenter(Action<bool> callback)
            {
                throw new NotImplementedException();
            }

            #region Helpers

            private static IntPtr GetStaticMethod(IntPtr classRaw, string name, string signature)
            {
                return AndroidJNIHelper.GetMethodID(classRaw, name, signature, true);
            }

            private void CallStaticVoidMethod(IntPtr method, jvalue[] args)
            {
                AndroidJNI.CallStaticVoidMethod(m_pluginClassRaw, method, args);
            }

            private bool CallStaticBoolMethod(IntPtr method, jvalue[] args)
            {
                return AndroidJNI.CallStaticBooleanMethod(m_pluginClassRaw, method, args);
            }

            private int CallStaticIntMethod(IntPtr method, jvalue[] args)
            {
                return AndroidJNI.CallStaticIntMethod(m_pluginClassRaw, method, args);
            }

            private jvalue jval(string value)
            {
                jvalue val = new jvalue();
                val.l = AndroidJNI.NewStringUTF(value);
                return val;
            }

            private jvalue jval(bool value)
            {
                jvalue val = new jvalue();
                val.z = value;
                return val;
            }

            private jvalue jval(int value)
            {
                jvalue val = new jvalue();
                val.i = value;
                return val;
            }

            private jvalue jval(float value)
            {
                jvalue val = new jvalue();
                val.f = value;
                return val;
            }

            #endregion
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
                throw new NotImplementedException();
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

        public static void Engage(string evt, Action<Boolean> callback = null, IDictionary<string, object> customData = null)
        {
            try
            {
                if (s_instance == null)
                {
                    Debug.LogWarningFormat("[Apptentive] Unable to engage '{0}' event: SDK not properly initialized.", evt);
                    if (callback != null)
                    {
                        callback(false);
                    }
                    return;
                }

                s_instance.platform.Engage(evt, customData, callback);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[Apptentive] Unable to engage '{0}' event: exception is thrown: {1}", evt, e.Message);
            }
        }

        public static void PresentMessageCenter(Action<Boolean> callback = null, IDictionary<string, object> customData = null)
        {
            try
            {
                if (s_instance == null)
                {
                    Debug.LogWarningFormat("[Apptentive] Unable to present message center: SDK not properly initialized.");
                    if (callback != null)
                    {
                        callback(false);
                    }
                    return;
                }

                s_instance.platform.PresentMessageCenter(customData, callback);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[Apptentive] Unable to present message center: exception is thrown: {1}", e.Message);
            }
        }

        public void CanShowInteraction(string eventName, Action<Boolean> callback)
        {
            try
            {
                if (s_instance == null)
                {
                    Debug.LogWarningFormat("[Apptentive] Unable to check if interaction can be shown: SDK not properly initialized.");
                    if (callback != null)
                    {
                        callback(false);
                    }
                    return;
                }

                s_instance.platform.CanShowInteraction(eventName, callback);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[Apptentive] Unable to check if interaction can be shown: exception is thrown: {1}", e.Message);
            }
        }

        public void CanShowMessageCenter(Action<bool> callback)
        {
            try
            {
                if (s_instance == null)
                {
                    Debug.LogWarningFormat("[Apptentive] Unable to check if message center can be shown: SDK not properly initialized.");
                    if (callback != null)
                    {
                        callback(false);
                    }
                    return;
                }

                s_instance.platform.CanShowMessageCenter(callback);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[Apptentive] Unable to check if message center can be shown: exception is thrown: {1}", e.Message);
            }
        }

        #endregion

        #region Helpers

        private static string ToJson(ApptentiveConfiguration configuration)
        {
            var payload = new Dictionary<string, object>();
            payload["apptentiveKey"] = configuration.appKey;
            payload["apptentiveSignature"] = configuration.appSignature;
            payload["apptlogLevelentiveKey"] = configuration.logLevel;
            payload["shouldSanitizeLogMessages"] = configuration.sanitizeLogMessages;
            return JsonUtils.ToJson(payload);
        }

        #endregion

        #region Properties

        IPlatform platform
        {
            get { return m_platform != null ? m_platform : Platform.Null; }
        }

        #endregion
    }
}
