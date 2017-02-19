using System;
using ModTools.Utils;
using UnityEngine;

namespace ModTools.Explorer
{
    public class GUIButtons
    {
        private static System.Object _buffer = null;

        public static void SetupButtons(Type type, object value, ReferenceChain refChain)
        {
            if (TypeUtil.IsTextureType(type) && value != null)
            {
                var texture = (Texture)value;
                if (GUILayout.Button("Preview"))
                {
                    TextureViewer.CreateTextureViewer(refChain, texture);
                }
                if (GUILayout.Button("Dump .png"))
                {
                    TextureUtil.DumpTextureToPNG(texture);
                }
            }
            else if (TypeUtil.IsMeshType(type) && value != null)
            {
                if (GUILayout.Button("Preview"))
                {
                    MeshViewer.CreateMeshViewer(null, (Mesh)value, null);
                }
                if (((Mesh)value).isReadable)
                {
                    if (GUILayout.Button("Dump .obj"))
                    {
                        var outPath = refChain.ToString().Replace(' ', '_');
                        DumpUtil.DumpMeshAndTextures(outPath, value as Mesh);
                    }
                }
            }
            if (GUILayout.Button("Copy"))
            {
                _buffer = value;
            }
        }

        public static bool SetupPasteButon(Type type, out object paste)
        {
            paste = null;
            if (_buffer != null && type.IsInstanceOfType(_buffer))
            {
                if (GUILayout.Button("Paste"))
                {
                    paste = _buffer;
                    return true;
                }
            }
            return GUILayout.Button("Unset");
        }
    }
}