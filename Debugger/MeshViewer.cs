using System;
using System.Reflection;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{
    public class MeshViewer : GUIWindow
    {

        private Mesh previewMesh = null;
        private Material previewMaterial;
        private String assetName = null;

        private RenderTexture targetRT;

        private float zoom = 100.0f;

        private Quaternion rotation = Quaternion.identity;
        private Vector2 lastMousePos = Vector2.zero;

        private Camera meshViewerCamera;

        private Material material;

        private Light light;

        private bool useOriginalShader = true;

        private MeshViewer()
            : base("Mesh Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;

            try
            {
                light = GameObject.Find("Directional Light").GetComponent<Light>();
            }
            catch (Exception)
            {
                light = null;
            }

            meshViewerCamera = gameObject.AddComponent<Camera>();
            meshViewerCamera.transform.position = new Vector3(-10000.0f, -10000.0f, -10000.0f);
            meshViewerCamera.fieldOfView = 20.0f;
            meshViewerCamera.backgroundColor = Color.grey;
            meshViewerCamera.nearClipPlane = 1.0f;
            meshViewerCamera.farClipPlane = 1000.0f;
            meshViewerCamera.enabled = false;
            meshViewerCamera.hdr = true;

            targetRT = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            meshViewerCamera.targetTexture = targetRT;
        }

        public static MeshViewer CreateMeshViewer(String assetName, Mesh mesh, Material material)
        {
            var go = new GameObject("MeshViewer");
            go.transform.parent = ModTools.Instance.transform;
            var meshViewer = go.AddComponent<MeshViewer>();
            meshViewer.assetName = assetName;
            meshViewer.previewMesh = mesh;
            meshViewer.material = material;

            meshViewer.previewMaterial = new Material(Shader.Find("Diffuse"));
            if (material != null)
            {
                meshViewer.previewMaterial.mainTexture = material.mainTexture;
            }
            meshViewer.visible = true;
            return meshViewer;
        }

        void Update()
        {
            if (previewMesh == null)
            {
                return;
            }

            float intensity = 0.0f;
            Color color = Color.black;
            bool enabled = false;

            if (light != null)
            {
                intensity = light.intensity;
                color = light.color;
                enabled = light.enabled;
                light.intensity = 2.0f;
                light.color = Color.white;
                light.enabled = true;
            }

            var pos = meshViewerCamera.transform.position + new Vector3(0.0f, -zoom, -zoom);
            var trs = Matrix4x4.TRS(pos, rotation, new Vector3(1.0f, 1.0f, 1.0f));
            meshViewerCamera.transform.LookAt(pos, Vector3.up);

            var material1 = (useOriginalShader && material != null) ? material : previewMaterial;
            Graphics.DrawMesh(previewMesh, trs, material1, 0, meshViewerCamera, 0, null, false, false);
            meshViewerCamera.RenderWithShader(material1.shader, "");

            if (light != null)
            {
                light.intensity = intensity;
                light.color = color;
                light.enabled = enabled;
            }
        }

        void DrawWindow()
        {
            if (previewMesh != null)
            {
                title = $"Previewing \"{assetName ?? previewMesh.name}\"";

                GUILayout.BeginHorizontal();

                if (material != null)
                {
                    useOriginalShader = GUILayout.Toggle(useOriginalShader, "Original Shader");
                    if (previewMesh.isReadable)
                    {
                        if (GUILayout.Button("Dump mesh+textures", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpMeshAndTextures(assetName, previewMesh, material);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Dump textures", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpTextures(assetName, material);
                        }
                    }
                }
                else
                {
                    useOriginalShader = false;
                    if (previewMesh.isReadable)
                    {
                        if (GUILayout.Button("Dump mesh", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpMeshAndTextures($"{previewMesh.name}", previewMesh);
                        }
                    }
                }
                if (previewMesh.isReadable)
                {
                    GUILayout.Label($"Triangles: {previewMesh.triangles.Length / 3}");
                }
                else
                {
                    var oldColor = GUI.color;
                    GUI.color = Color.yellow;
                    GUILayout.Label("Mesh insn't readable!");
                    GUI.color = oldColor;
                }
                if (material?.mainTexture != null)
                {
                    GUILayout.Label($"Texture size: {material.mainTexture.width}x{material.mainTexture.height}");
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown)
                {
                    lastMousePos = Event.current.mousePosition;
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    var pos = Event.current.mousePosition;
                    if (lastMousePos != Vector2.zero)
                    {
                        var delta = pos - lastMousePos;
                        zoom += delta.y;
                        rotation *= Quaternion.Euler(0.0f, -delta.x, 0.0f);
                    }
                    lastMousePos = pos;
                }

                GUI.DrawTexture(new Rect(0.0f, 64.0f, rect.width, rect.height - 64.0f), targetRT, ScaleMode.StretchToFill, false);
            }
            else
            {
                title = "Mesh Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Mesh for preview");
            }
        }
    }
}
