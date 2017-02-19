using System.IO;
using System.Linq;
using UnityEngine;

namespace ModTools.Utils
{
    public static class DumpUtil
    {
        public static void DumpAsset(string assetName, Mesh mesh, Material material,
            Mesh lodMesh = null, Material lodMaterial = null)
        {
            assetName = assetName.Replace("_Data", "");
            Log.Warning($"Dumping asset \"{assetName}\"...");
            DumpMeshAndTextures(assetName, mesh, material);
            DumpMeshAndTextures($"{assetName}_lod", lodMesh, lodMaterial);
            Log.Warning($"Successfully dumped asset \"{assetName}\"");
        }

        public static void DumpMeshAndTextures(string assetName, Mesh mesh, Material material = null)
        {
            assetName = assetName.Replace("_Data", "").LegalizeFileName();

            if (mesh != null && mesh.isReadable)
            {
                MeshUtil.DumpMeshToOBJ(mesh, $"{assetName}.obj");
            }
            if (material != null)
            {
                DumpTextures(assetName, material);
            }
        }

        public static void DumpTextures(string assetName, Material material)
        {
            assetName = assetName.Replace("_Data", "").LegalizeFileName();
            DumpMainTex(assetName, (Texture2D) material.GetTexture("_MainTex"));
            DumpACI(assetName, (Texture2D) material.GetTexture("_ACIMap"));
            DumpXYS(assetName, (Texture2D) material.GetTexture("_XYSMap"));
            DumpXYCA(assetName, (Texture2D) material.GetTexture("_XYCAMap"));
        }

        private static void DumpMainTex(string assetName, Texture2D mainTex, bool extract = true)
        {
            if (mainTex == null)
            {
                return;
            }
            if (extract)
            {
                var length = mainTex.width * mainTex.height;
                var r = new Color32[length].Invert();
                mainTex.ExtractChannels(r, r, r, null, false, false, false, false, false, false, false);
                TextureUtil.DumpTextureToPNG(r.ColorsToTexture(mainTex.width, mainTex.height), $"{assetName}_d");
            }
            else
            {
                TextureUtil.DumpTextureToPNG(mainTex, $"{assetName}_MainTex");
            }

        }

        private static void DumpACI(string assetName, Texture2D aciMap, bool extract = true)
        {
            if (aciMap == null)
            {
                return;
            }
            if (extract)
            {
                var length = aciMap.width * aciMap.height;
                var r = new Color32[length].Invert();
                var g = new Color32[length].Invert();
                var b = new Color32[length].Invert();
                aciMap.ExtractChannels(r, g, b, null, true, true, true, true, true, false, false);
                TextureUtil.DumpTextureToPNG(r.ColorsToTexture(aciMap.width, aciMap.height), $"{assetName}_a");
                TextureUtil.DumpTextureToPNG(g.ColorsToTexture(aciMap.width, aciMap.height), $"{assetName}_c");
                TextureUtil.DumpTextureToPNG(b.ColorsToTexture(aciMap.width, aciMap.height), $"{assetName}_i");
            }
            else
            {
                TextureUtil.DumpTextureToPNG(aciMap, $"{assetName}_ACI");
            }
        }

        private static void DumpXYS(string assetName, Texture2D xysMap, bool extract = true)
        {
            if (xysMap == null)
            {
                return;
            }
            if (extract)
            {
                var length = xysMap.width * xysMap.height;
                var r1 = new Color32[length].Invert();
                var b1 = new Color32[length].Invert();
                xysMap.ExtractChannels(r1, r1, b1, null, false, false, true, false, false, true, false);
                TextureUtil.DumpTextureToPNG(r1.ColorsToTexture(xysMap.width, xysMap.height), $"{assetName}_n");
                TextureUtil.DumpTextureToPNG(b1.ColorsToTexture(xysMap.width, xysMap.height), $"{assetName}_s");
            }
            else
            {
                TextureUtil.DumpTextureToPNG(xysMap, $"{assetName}_XYS");
            }
        }

        private static void DumpXYCA(string assetName, Texture2D xycaMap, bool extract = true)
        {
            if (xycaMap == null)
            {
                return;
            }
            if (extract)
            {
                var length = xycaMap.width * xycaMap.height;
                var r1 = new Color32[length].Invert();
                var b1 = new Color32[length].Invert();
                var a1 = new Color32[length].Invert();
                xycaMap.ExtractChannels(r1, r1, b1, a1, false, false, true, false, false, true, true);
                TextureUtil.DumpTextureToPNG(r1.ColorsToTexture(xycaMap.width, xycaMap.height), $"{assetName}_n");
                TextureUtil.DumpTextureToPNG(b1.ColorsToTexture(xycaMap.width, xycaMap.height), $"{assetName}_c");
                TextureUtil.DumpTextureToPNG(a1.ColorsToTexture(xycaMap.width, xycaMap.height), $"{assetName}_a");
            }
            else
            {
                TextureUtil.DumpTextureToPNG(xycaMap, $"{assetName}_XYCA");
            }
        }
    }
}