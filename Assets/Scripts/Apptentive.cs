using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEngine;

using ApptentiveConnectInternal;

namespace ApptentiveConnect
{
    delegate void LunarConsoleNativeMessageCallback(string message);
    delegate void LunarConsoleNativeMessageHandler(IDictionary<string, string> data);

    public sealed class Apptentive : MonoBehaviour
    {
        static Apptentive s_instance;

        [Tooltip("The API key for Apptentive.\n\n This key is found on the Apptentive website under Settings, API & Development.")]
        [SerializeField]
        string m_APIKey;

        IPlatform m_platform;

        IDictionary<string, LunarConsoleNativeMessageHandler> m_nativeHandlerLookup;

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
                LunarConsoleNativeMessageCallback callback = NativeMessageCallback;
                return new PlatformIOS(gameObject.name, callback.Method.Name, Constants.Version, APIKey);
            }
            #elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                LunarConsoleNativeMessageCallback callback = NativeMessageCallback;
                return new PlatformAndroid(gameObject.name, callback.Method.Name, Constants.Version, capacity, trim, GetGestureName(m_gesture));
            }
            #endif

            return null;
        }

        interface IPlatform
        {
            void OnLogMessageReceived(string message, string stackTrace, LogType type);
            bool ShowConsole();
            bool HideConsole();
            void ClearConsole();
        }

        #if UNITY_IOS || UNITY_IPHONE

        class PlatformIOS : IPlatform
        {
            [DllImport("__Internal")]
            private static extern void __apptentive_initialize(string targetName, string methodName, string version, string APIKey);
            
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
        }

        #elif UNITY_ANDROID

        class PlatformAndroid : IPlatform
        {
            private readonly object logLock = new object();

            private readonly jvalue[] args0 = new jvalue[0];
            private readonly jvalue[] args3 = new jvalue[3];

            private static readonly string PluginClassName = "spacemadness.com.lunarconsole.console.ConsolePlugin";

            private readonly AndroidJavaClass pluginClass;

            private readonly IntPtr pluginClassRaw;
            private readonly IntPtr methodLogMessage;
            private readonly IntPtr methodShowConsole;
            private readonly IntPtr methodHideConsole;
            private readonly IntPtr methodClearConsole;

            /// <summary>
            /// Initializes a new instance of the Android platform class.
            /// </summary>
            /// <param name="targetName">The name of the game object which will receive native callbacks</param>
            /// <param name="methodName">The method of the game object which will be called from the native code</param>
            /// <param name="version">Plugin version</param>
            /// <param name="capacity">Console capacity (elements over this amount will be trimmed)</param>
            /// <param name="trim">Console trim amount (how many elements will be trimmed on the overflow)</param>
            /// <param name="gesture">Gesture name to activate the console</param>
            public PlatformAndroid(string targetName, string methodName, string version, int capacity, int trim, string gesture)
            {
                pluginClass = new AndroidJavaClass(PluginClassName);
                pluginClassRaw = pluginClass.GetRawClass();

                IntPtr methodInit = GetStaticMethod(pluginClassRaw, "init", "(Ljava.lang.String;Ljava.lang.String;Ljava.lang.String;IILjava.lang.String;)V");
                CallStaticVoidMethod(methodInit, new jvalue[] {
                    jval(targetName),
                    jval(methodName),
                    jval(version),
                    jval(capacity),
                    jval(trim),
                    jval(gesture)
                });

                methodLogMessage = GetStaticMethod(pluginClassRaw, "logMessage", "(Ljava.lang.String;Ljava.lang.String;I)V");
                methodShowConsole = GetStaticMethod(pluginClassRaw, "show", "()V");
                methodHideConsole = GetStaticMethod(pluginClassRaw, "hide", "()V");
                methodClearConsole = GetStaticMethod(pluginClassRaw, "clear", "()V");
            }

            ~PlatformAndroid()
            {
                pluginClass.Dispose();
            }

            #region IPlatform implementation
            
            public void OnLogMessageReceived(string message, string stackTrace, LogType type)
            {
                lock (logLock)
                {
                    args3[0] = jval(message);
                    args3[1] = jval(stackTrace);
                    args3[2] = jval((int)type);

                    CallStaticVoidMethod(methodLogMessage, args3);
                }
            }

            public bool ShowConsole()
            {
                try
                {
                    CallStaticVoidMethod(methodShowConsole, args0);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool HideConsole()
            {
                try
                {
                    CallStaticVoidMethod(methodHideConsole, args0);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public void ClearConsole()
            {
                try
                {
                    CallStaticVoidMethod(methodClearConsole, args0);
                }
                catch (Exception)
                {
                }
            }

            #endregion

            #region Helpers

            private static IntPtr GetStaticMethod(IntPtr classRaw, string name, string signature)
            {
                return AndroidJNIHelper.GetMethodID(classRaw, name, signature, true);
            }

            private void CallStaticVoidMethod(IntPtr method, jvalue[] args)
            {
                AndroidJNI.CallStaticVoidMethod(pluginClassRaw, method, args);
            }

            private bool CallStaticBoolMethod(IntPtr method, jvalue[] args)
            {
                return AndroidJNI.CallStaticBooleanMethod(pluginClassRaw, method, args);
            }

            private jvalue jval(string value)
            {
                jvalue val = new jvalue();
                val.l = AndroidJNI.NewStringUTF(value);
                return val;
            }

            private jvalue jval(int value)
            {
                jvalue val = new jvalue();
                val.i = value;
                return val;
            }

            #endregion
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

            LunarConsoleNativeMessageHandler handler;
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

        IDictionary<string, LunarConsoleNativeMessageHandler> nativeHandlerLookup
        {
            get
            {
                if (m_nativeHandlerLookup == null)
                {
                    m_nativeHandlerLookup = new Dictionary<string, LunarConsoleNativeMessageHandler>();
                    m_nativeHandlerLookup["console_open"] = ConsoleOpenHandler;
                    m_nativeHandlerLookup["console_close"] = ConsoleCloseHandler;
                }

                return m_nativeHandlerLookup;
            }
        }

        void ConsoleOpenHandler(IDictionary<string, string> data)
        {
            if (onConsoleOpened != null)
            {
                onConsoleOpened();
            }
        }

        void ConsoleCloseHandler(IDictionary<string, string> data)
        {
            if (onConsoleClosed != null)
            {
                onConsoleClosed();
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Shows Lunar console on top of everything. Does nothing if platform is not supported or if plugin is not initizlied.
        /// </summary>
        public static void Show()
        {
#if LUNAR_CONSOLE_PLATFORM_SUPPORTED
        #if LUNAR_CONSOLE_ENABLED
            if (s_instance != null)
            {
                s_instance.ShowConsole();
            }
            else
            {
                Debug.LogError("Can't show " + Constants.PluginName + ": instance is not initialized. Make sure you've installed it correctly");
            }
        #else
            Debug.LogWarning("Can't show " + Constants.PluginName + ": plugin is disabled");
        #endif
#else
            Debug.LogWarning("Can't show " + Constants.PluginName + ": current platform is not supported");
#endif
        }

        /// <summary>
        /// Hides Lunar console. Does nothing if platform is not supported or if plugin is not initizlied.
        /// </summary>
        public static void Hide()
        {
#if LUNAR_CONSOLE_PLATFORM_SUPPORTED
        #if LUNAR_CONSOLE_ENABLED
            if (s_instance != null)
            {
                s_instance.HideConsole();
            }
            else
            {
                Debug.LogError("Can't hide " + Constants.PluginName + ": instance is not initialized. Make sure you've installed it correctly");
            }
        #else
            Debug.LogWarning("Can't hide " + Constants.PluginName + ": plugin is disabled");
        #endif
#else
            Debug.LogWarning("Can't hide " + Constants.PluginName + ": current platform is not supported");
#endif
        }

        /// <summary>
        /// Clears Lunar console. Does nothing if platform is not supported or if plugin is not initizlied.
        /// </summary>
        public static void Clear()
        {
#if LUNAR_CONSOLE_PLATFORM_SUPPORTED
        #if LUNAR_CONSOLE_ENABLED
            if (s_instance != null)
            {
                s_instance.ClearConsole();
            }
            else
            {
                Debug.LogError("Can't clear " + Constants.PluginName + ": instance is not initialized. Make sure you've installed it correctly");
            }
        #else
            Debug.LogWarning("Can't clear " + Constants.PluginName + ": plugin is disabled");
        #endif
#else
            Debug.LogWarning("Can't clear " + Constants.PluginName + ": current platform is not supported");
#endif
        }

        public static Action onConsoleOpened { get; set; }
        public static Action onConsoleClosed { get; set; }

        #if LUNAR_CONSOLE_ENABLED

        void ShowConsole()
        {
            if (m_platform != null)
            {
                m_platform.ShowConsole();
            }
        }

        void HideConsole()
        {
            if (m_platform != null)
            {
                m_platform.HideConsole();
            }
        }

        void ClearConsole()
        {
            if (m_platform != null)
            {
                m_platform.ClearConsole();
            }
        }

        #endif // LUNAR_CONSOLE_ENABLED

        #endregion

        #region Properties

        /// <summary>
        /// The shared singleton of `Apptentive`
        /// </summary>
        public static Apptentive sharedConnection
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The API key for Apptentive.
        /// This key is found on the Apptentive website under Settings, API & Development.
        /// </summary>
        public String APIKey
        {
            get { return m_APIKey; }
            set { m_APIKey = value; }
        }

        #endregion
    }
}
