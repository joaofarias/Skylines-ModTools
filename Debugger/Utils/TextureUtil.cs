using System;
using System.IO;
using UnityEngine;

namespace ModTools.Utils
{
    public static class TextureUtil
    {

        public static void DumpTexture2D(Texture2D texture, string filename)
        {
            byte[] bytes;
            try
            {
                bytes = texture.EncodeToPNG();
            }
            catch
            {
                try
                {
                    bytes = texture.MakeReadable().EncodeToPNG();
                }
                catch (Exception ex)
                {
                    Log.Error("There was an error while dumping the texture - " + ex.Message);
                    return;
                }
            }
            File.WriteAllBytes(filename, bytes);
            Log.Warning($"Texture dumped to \"{filename}\"");
        }

        private static Texture2D MakeReadable(this Texture texture)
        {
            Log.Warning($"Texture \"{texture.name}\" is marked as read-only, running workaround..");
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0);
            Graphics.Blit(texture, rt);
            var tex = ToTexture2D(rt);
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        private static Texture2D ToTexture2D(this RenderTexture rt)
        {
            var oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = oldRt;
            return tex;
        }


        public static Texture2D ToTexture2D(this Texture3D t3d)
        {
            var pixels = t3d.GetPixels();
            var width = t3d.width;
            var depth = t3d.depth;
            var height = t3d.height;
            var tex = new Texture2D(width * depth, height);
            for (var k = 0; k < depth; k++)
            {
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        tex.SetPixel(j * width + i, (height - k - 1), pixels[width * depth * j + k * depth + i]);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        public static void DumpTextureToPNG(Texture previewTexture, string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                var filenamePrefix = $"rt_dump_{previewTexture.name.LegalizeFileName()}";
                if (!File.Exists($"{filenamePrefix}.png"))
                {
                    filename = $"{filenamePrefix}.png";
                }
                else
                {
                    int i = 1;
                    while (File.Exists($"{filenamePrefix}_{i}.png"))
                    {
                        i++;
                    }

                    filename = $"{filenamePrefix}_{i}.png";
                }
            }
            else
            {
                if (!filename.EndsWith(".png"))
                {
                    filename = $"{filename}.png";
                }
            }
            filename = Path.Combine(Path.Combine(Application.dataPath, "Import"), filename);

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            if (previewTexture is Texture2D)
            {
                DumpTexture2D((Texture2D)previewTexture, filename);
            }
            else if (previewTexture is RenderTexture)
            {
                DumpTexture2D(((RenderTexture)previewTexture).ToTexture2D(), filename);
                Log.Warning($"Texture dumped to \"{filename}\"");
            }
            else if (previewTexture is Texture3D)
            {
                DumpTexture2D(((Texture3D)previewTexture).ToTexture2D(), filename);
            }
            else
            {
                Log.Error($"Don't know how to dump type \"{previewTexture.GetType()}\"");
            }
        }

        public static Color32[] TextureToColors(this Texture2D texture2D)
        {
            Color32[] input;
            try
            {
                input = texture2D.GetPixels32();
            }
            catch
            {
                input = texture2D.MakeReadable().GetPixels32();
            }
            return input;
        }

        public static Texture2D ColorsToTexture(this Color32[] colors, int width, int height)
        {
            var texture2D = new Texture2D(width, height);
            texture2D.SetPixels32(colors);
            texture2D.Apply();
            return texture2D;
        }

        public static Color32[] Invert(this Color32[] colors)
        {
            var result = new Color32[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                result[i].r = (byte) (byte.MaxValue - colors[i].r);
                result[i].g = (byte)(byte.MaxValue - colors[i].g);
                result[i].b = (byte)(byte.MaxValue - colors[i].b);
                result[i].a = (byte)(byte.MaxValue - colors[i].a);
            }
            return result;
        }

        public static void ExtractChannels(this Texture2D texture, Color32[] r, Color32[] g, Color32[] b, Color32[] a, bool ralpha, bool galpha, bool balpha, bool rinvert, bool ginvert, bool binvert, bool ainvert)
        {
            var input = texture.TextureToColors();
            for (var index = 0; index < input.Length; ++index)
            {
                var rr = input[index].r;
                var gg = input[index].g;
                var bb = input[index].b;
                var aa = input[index].a;
                if (rinvert)
                    rr = (byte)(byte.MaxValue - rr);
                if (ginvert)
                    gg = (byte)(byte.MaxValue - gg);
                if (binvert)
                    bb = (byte)(byte.MaxValue - bb);
                if (ainvert)
                    aa = (byte)(byte.MaxValue - aa);
                if (r != null) if (ralpha) { r[index].r = rr; r[index].g = rr; r[index].b = rr; } else { r[index].r = rr; }
                if (g != null) if (galpha) { g[index].r = gg; g[index].g = gg; g[index].b = gg; } else { g[index].g = gg; }
                if (b != null) if (balpha) { b[index].r = bb; b[index].g = bb; b[index].b = bb; } else { b[index].b = bb; }
                if (a != null) { a[index].r = aa; a[index].g = aa; a[index].b = aa;}
            }
        }
    }
}