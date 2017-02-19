using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModTools
{

    public class ScriptEditorFile
    {
        public string source = "";
        public string path = "";
        public FileSystemWatcher filesystemWatcher = null;
    }

    public class ScriptEditor : GUIWindow
    {

        private readonly string textAreaControlName = "ModToolsScriptEditorTextArea";

        private const string ExampleScriptFileName = "ExampleScript.cs";

        private IModEntryPoint currentMod;
        private string lastError = "";

        private float compactHeaderHeight = 50.0f;
        private float expandedHeaderHeight = 120.0f;
        private float footerHeight = 60.0f;

        private bool headerExpanded = true;

        private GUIArea headerArea;
        private GUIArea editorArea;
        private GUIArea footerArea;

        private Vector2 editorScrollPosition = Vector2.zero;
        private Vector2 projectFilesScrollPosition = Vector2.zero;

        private string projectWorkspacePath = "";
        private List<ScriptEditorFile> projectFiles = new List<ScriptEditorFile>();
        private ScriptEditorFile currentFile = null;

        public ScriptEditor() : base("Script Editor", new Rect(16.0f, 16.0f, 640.0f, 480.0f), skin)
        {
            onDraw = DrawWindow;
            onException = HandleException;
            visible = true;

            headerArea = new GUIArea(this);
            editorArea = new GUIArea(this);
            footerArea = new GUIArea(this);
            RecalculateAreas();
        }

        void RecalculateAreas()
        {
            float headerHeight = headerExpanded ? expandedHeaderHeight : compactHeaderHeight;

            headerArea.absolutePosition.y = 32.0f;
            headerArea.relativeSize.x = 1.0f;
            headerArea.absoluteSize.y = headerHeight;

            editorArea.absolutePosition.y = 32.0f + headerHeight;
            editorArea.relativeSize.x = 1.0f;
            editorArea.relativeSize.y = 1.0f;
            editorArea.absoluteSize.y = -(32.0f + headerHeight + footerHeight);

            footerArea.relativePosition.y = 1.0f;
            footerArea.absolutePosition.y = -footerHeight;
            footerArea.absoluteSize.y = footerHeight;
            footerArea.relativeSize.x = 1.0f;
        }

        public void ReloadProjectWorkspace()
        {
            projectWorkspacePath = ModTools.Instance.config.scriptEditorWorkspacePath;
            if (projectWorkspacePath.Length == 0)
            {
                lastError = "Invalid project workspace path";
                return;
            }

            bool exampleFileExists = false;

            projectFiles.Clear();

            try
            {
                var files = FileUtil.ListFilesInDirectory(projectWorkspacePath);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".cs")
                    {
                        if (Path.GetFileName(file) == ExampleScriptFileName)
                        {
                            exampleFileExists = true;
                        }

                        projectFiles.Add(new ScriptEditorFile() { path = file, source = File.ReadAllText(file) });
                    }
                }
                if (!exampleFileExists)
                {
                    var exampleFile = new ScriptEditorFile()
                    {
                        path = Path.Combine(projectWorkspacePath, ExampleScriptFileName),
                        source = defaultSource
                    };

                    projectFiles.Add(exampleFile);
                    SaveProjectFile(exampleFile);
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return;
            }
            lastError = "";
        }

        void SaveAllProjectFiles()
        {
            try
            {
                foreach (var file in projectFiles)
                {
                    SaveProjectFile(file);
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return;
            }
            lastError = "";
        }

        void SaveProjectFile(ScriptEditorFile file)
        {
            File.WriteAllText(file.path, file.source);
        }

        void Update()
        {
            if (GUI.GetNameOfFocusedControl() == textAreaControlName)
            {
            }
        }

        void DrawHeader()
        {
            headerArea.Begin();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scripts are stored in project workspace. To add a script create a new .cs file in workspace and click 'Reload'", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Project workspace:", GUILayout.ExpandWidth(false));
            var newProjectWorkspacePath = GUILayout.TextField(projectWorkspacePath, GUILayout.ExpandWidth(true));
            if (!newProjectWorkspacePath.Equals(projectWorkspacePath))
            {
                projectWorkspacePath = newProjectWorkspacePath.Trim();
                ModTools.Instance.config.scriptEditorWorkspacePath = projectWorkspacePath;
                ModTools.Instance.SaveConfig();
            }

            if (GUILayout.Button("Reload", GUILayout.Width(100)))
            {
                ReloadProjectWorkspace();
            }

            GUILayout.EndHorizontal();

            projectFilesScrollPosition = GUILayout.BeginScrollView(projectFilesScrollPosition);
            GUILayout.BeginHorizontal();

            foreach (var file in projectFiles)
            {
                if (GUILayout.Button(Path.GetFileName(file.path), GUILayout.ExpandWidth(false)))
                {
                    currentFile = file;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            headerArea.End();
        }

        void DrawEditor()
        {
            editorArea.Begin();

            editorScrollPosition = GUILayout.BeginScrollView(editorScrollPosition);

            GUI.SetNextControlName(textAreaControlName);

            string text = GUILayout.TextArea(currentFile != null ? currentFile.source : "No file loaded..", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (GUIUtility.keyboardControl == editor.controlID && Event.current.Equals(Event.KeyboardEvent("tab")))
            {
#if NEWVERSION
                if (text.Length > editor.cursorIndex)
                {
                    text = text.Insert(editor.cursorIndex, "\t");
                    editor.cursorIndex++;
                    editor.selectIndex = editor.cursorIndex;
                }
#else
                if (text.Length > editor.pos)
                {
                    text = text.Insert(editor.pos, "\t");
                    editor.pos++;
                    editor.selectPos = editor.pos;
                }
#endif
                Event.current.Use();
            }

            if (currentFile != null)
            {
                currentFile.source = text;
            }

            GUILayout.EndScrollView();

            editorArea.End();
        }

        void DrawFooter()
        {
            footerArea.Begin();

            GUILayout.BeginHorizontal();

            if (currentMod != null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Compile"))
            {
                string dllPath;
                if (ScriptCompiler.CompileSource(projectFiles, out dllPath))
                {
                    Log.Message("Source compiled to \"" + dllPath + "\"");
                }
                else
                {
                    Log.Error("Failed to compile script!");
                }
            }

            if (GUILayout.Button("Run"))
            {
                string errorMessage;
                if (ScriptCompiler.RunSource(projectFiles, out errorMessage, out currentMod))
                {
                    Log.Message("Running IModEntryPoint.OnModLoaded()");

                    try
                    {
                        currentMod.OnModLoaded();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Exception while calling IModEntryPoint.OnModLoaded() - {ex.Message}");
                    }
                }
                else
                {
                    lastError = errorMessage;
                    Log.Error("Failed to compile or run source, reason: " + errorMessage);
                }
            }

            GUI.enabled = false;
            if (currentMod != null)
            {
                GUI.enabled = true;
            }

            if (GUILayout.Button("Stop"))
            {
                Log.Message("Running IModEntryPoint.OnModUnloaded()");
                try
                {
                    currentMod.OnModUnloaded();
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception while calling IModEntryPoint.OnModUnloaded() - {ex.Message}");
                }

                currentMod = null;
            }

            GUI.enabled = true;

            GUILayout.Label("Last error: " + lastError);

            GUILayout.FlexibleSpace();

            if (currentFile == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Save"))
            {
                try
                {
                    SaveProjectFile(currentFile);
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    return;
                }
                lastError = "";
            }

            GUI.enabled = true;

            if (GUILayout.Button("Save all"))
            {
                SaveAllProjectFiles();
            }

            GUILayout.EndHorizontal();

            footerArea.End();
        }

        void DrawWindow()
        {
            DrawHeader();

            if (projectFiles.Any())
            {
                DrawEditor();
                DrawFooter();
            }
            else
            {
                editorArea.Begin();
                GUILayout.Label("Select a valid project workspace path");
                editorArea.End();
            }
        }

        void HandleException(Exception ex)
        {
            Log.Error("Exception in ScriptEditor - " + ex.Message);
            visible = false;
        }

        private readonly string defaultSource = @"//You can copy this script's file and use it for your own scripts
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;
        

namespace ModTools
{
    class ExampleScript : IModEntryPoint
    {
        public void OnModLoaded()
        {
            throw new Exception(""Hello World!""); //replace this line with your script
        }
        public void OnModUnloaded()
        {
            throw new Exception(""Goodbye Cruel World!""); //replace this line with your clean up script
        }
    }
}";

    }

}
