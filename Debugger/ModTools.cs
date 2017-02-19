﻿using System;
using UnityEngine;

namespace ModTools
{

    public class ModTools : GUIWindow
    {

#if DEBUG
        public static readonly bool DEBUG_MODTOOLS = true;
#else
        public static readonly bool DEBUG_MODTOOLS = false;
#endif
        private static readonly object LoggingLock = new object();
        private static readonly object InstanceLock = new object();

        private Vector2 mainScroll = Vector2.zero;

        public Console console;
        public SceneExplorer sceneExplorer;
        public SceneExplorerColorConfig sceneExplorerColorConfig;

        public ScriptEditor scriptEditor;

        public Watches watches;
        public ColorPicker colorPicker;

        public Configuration config = new Configuration();
        public static readonly string configPath = "ModToolsConfig.xml";

        private static ModTools instance = null;

        public void OnUnityDestroyCallback()
        {
            Destroy(console);
            Destroy(sceneExplorer);
            Destroy(sceneExplorerColorConfig);
            Destroy(scriptEditor);
            Destroy(watches);
            Destroy(colorPicker);

            instance = null;
            ModToolsBootstrap.initialized = false;
        }

        public static ModTools Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    instance = instance ?? FindObjectOfType<ModTools>();
                    return instance;
                }
            }
        }

        public void LoadConfig()
        {
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
                SaveConfig();
            }

            if (console != null)
            {
                console.rect = config.consoleRect;
                console.visible = config.consoleVisible;
            }

            watches.rect = config.watchesRect;
            watches.visible = config.watchesVisible;

            sceneExplorer.rect = config.sceneExplorerRect;
            sceneExplorer.visible = config.sceneExplorerVisible;

            if (sceneExplorer.visible)
            {
                sceneExplorer.Refresh();
            }

            scriptEditor.ReloadProjectWorkspace();
        }

        public void SaveConfig()
        {
            if (config != null)
            {
                if (console != null)
                {
                    config.consoleRect = console.rect;
                    config.consoleVisible = console.visible;
                }

                config.watchesRect = watches.rect;
                config.watchesVisible = watches.visible;

                config.sceneExplorerRect = sceneExplorer.rect;
                config.sceneExplorerVisible = sceneExplorer.visible;

                Configuration.Serialize(configPath, config);
            }
        }

        public ModTools() : base("Mod Tools", new Rect(128, 128, 356, 290), skin)
        {
            onDraw = DoMainWindow;
            onUnityDestroy = OnUnityDestroyCallback;
            resizable = false;
        }

        private static bool loggingInitialized = false;

        public void Initialize()
        {

            if (!loggingInitialized)
            {
                Application.logMessageReceivedThreaded += OnApplicationLogMessageReceivedThreaded;

                loggingInitialized = true;
            }

            sceneExplorer = gameObject.AddComponent<SceneExplorer>();
            watches = gameObject.AddComponent<Watches>();
            colorPicker = gameObject.AddComponent<ColorPicker>();
            scriptEditor = gameObject.AddComponent<ScriptEditor>();
            scriptEditor.visible = false;
            sceneExplorerColorConfig = gameObject.AddComponent<SceneExplorerColorConfig>();

            LoadConfig();

            if (config.useModToolsConsole)
            {
                console = gameObject.AddComponent<Console>();
            }

        }

        private void OnApplicationLogMessageReceivedThreaded(string condition, string trace, LogType type)
        {
            if (!ModToolsBootstrap.initialized)
            {
                return;
            }
            lock (LoggingLock)
            {
                if (!config.hookUnityLogging)
                {
                    return;
                }
                if (type == LogType.Exception)
                {
                    var message = condition;
                    if (config.logExceptionsToConsole)
                    {
                        if (trace != null)
                        {
                            message = $"{message}\n\n{trace}";
                        }
                    }
                    Log.Error(message);
                }
                else if (type == LogType.Error || type == LogType.Assert)
                {
                    Log.Error(condition);
                }
                else if (type == LogType.Warning)
                {
                    Log.Warning(condition);
                }
                else
                {
                    Log.Message(condition);
                }
            }
        }

        void Update()
        {
            UpdateMouseScrolling();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
            {
                visible = !visible;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
            {
                sceneExplorer.visible = !sceneExplorer.visible;
                if (sceneExplorer.visible)
                {
                    sceneExplorer.Refresh();
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
            {
                watches.visible = !watches.visible;
            }

            if (config.useModToolsConsole && Input.GetKeyDown(KeyCode.F7))
            {
                console.visible = !console.visible;
            }

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                scriptEditor.visible = !scriptEditor.visible;
            }
        }

        void DoMainWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Use ModTools console");
            var newUseConsole = GUILayout.Toggle(config.useModToolsConsole, "");
            GUILayout.EndHorizontal();

            if (newUseConsole != config.useModToolsConsole)
            {
                config.useModToolsConsole = newUseConsole;

                if (config.useModToolsConsole)
                {
                    console = gameObject.AddComponent<Console>();
                }
                else
                {
                    Destroy(console);
                    console = null;
                }

                SaveConfig();
            }

            if (!config.useModToolsConsole)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hook Unity's logging");
            var newHookLogging = GUILayout.Toggle(config.hookUnityLogging, "");
            GUILayout.EndHorizontal();

            if (newHookLogging != config.hookUnityLogging)
            {
                config.hookUnityLogging = newHookLogging;
                SaveConfig();
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Log exceptions to console");
            var newLogExceptionsToConsole = GUILayout.Toggle(config.logExceptionsToConsole, "");
            GUILayout.EndHorizontal();
            if (newLogExceptionsToConsole != config.logExceptionsToConsole)
            {
                config.logExceptionsToConsole = newLogExceptionsToConsole;
                SaveConfig();
            }

            if (GUILayout.Button("Debug console (F7)"))
            {
                if (console != null)
                {
                    console.visible = !console.visible;
                }
            }

            if (GUILayout.Button("Watches (Ctrl+W)"))
            {
                watches.visible = !watches.visible;
            }

            if (GUILayout.Button("Scene explorer (Ctrl+E)"))
            {
                sceneExplorer.visible = !sceneExplorer.visible;
                if (sceneExplorer.visible)
                {
                    sceneExplorer.Refresh();
                }
            }

            if (GUILayout.Button("Script editor (Ctrl+`)"))
            {
				// Disabled until I find a workaround
                //scriptEditor.visible = !scriptEditor.visible;
            }
        }
    }

}
