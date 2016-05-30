﻿using System;
using System.Reflection;
using ICities;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{
    public class MeshViewer : GUIWindow
    {

        public Mesh previewMesh = null;
        public bool materialProvided = false;
        public String assetName = null;

        private RenderTexture targetRT;

        private float zoom = 100.0f;

        private Quaternion rotation = Quaternion.identity;
        private Vector2 lastMousePos = Vector2.zero;

        private Camera meshViewerCamera;

        private Material material;
        private Light light;

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
            if (material != null)
            {
                meshViewer.material = material;
                meshViewer.materialProvided = true;
            }
            else
            {
                meshViewer.material = new Material(Shader.Find("Diffuse")); ;
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

            if (light != null)
            {
                intensity = light.intensity;
                color = light.color;
                light.intensity = 2.0f;
                light.color = Color.white;
            }

            var pos = meshViewerCamera.transform.position + new Vector3(0.0f, -zoom, -zoom);
            var trs = Matrix4x4.TRS(pos, rotation, new Vector3(1.0f, 1.0f, 1.0f));
            meshViewerCamera.transform.LookAt(pos, Vector3.up);

            Graphics.DrawMesh(previewMesh, trs, material, 0, meshViewerCamera, 0, null, false, false);
            meshViewerCamera.RenderWithShader(material.shader, "");

            if (light != null)
            {
                light.intensity = intensity;
                light.color = color;
            }
        }

        void DrawWindow()
        {
            if (previewMesh != null)
            {
                title = $"Previewing \"{assetName ?? previewMesh.name}\"";

                GUILayout.BeginHorizontal();

                if (!materialProvided)
                {
                    if (GUILayout.Button("Dump mesh", GUILayout.Width(128)))
                    {
                        MeshUtil.DumpMeshToOBJ(previewMesh, $"{previewMesh.name}.obj");
                    }
                }
                else
                {
                    if (GUILayout.Button("Dump mesh+textures", GUILayout.Width(160)))
                    {
                        DumpUtil.DumpAsset(assetName, previewMesh, material);
                    }
                }

                if (previewMesh.isReadable)
                {
                    GUILayout.Label(String.Format("Triangles: {0}", previewMesh.triangles.Length / 3));
                }
                if (materialProvided && material.mainTexture != null)
                {
                    GUILayout.Label(String.Format("Texture size: {0}x{1}", material.mainTexture.width, material.mainTexture.height));
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
