using System;
using System.IO;
using ObjUnity3D;
using UnityEngine;

namespace ModTools.Utils
{
    public static class MeshUtil
    {
        public static void DumpMeshToOBJ(Mesh mesh, string fileName)
        {
            fileName = Path.Combine(Path.Combine(Application.dataPath, "Import"), fileName);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            Mesh meshToDump = mesh;

            if (!mesh.isReadable)
            {
                Log.Warning($"Mesh \"{mesh.name}\" is marked as non-readable, running workaround..");

                try
                {
                    // copy the relevant data to the temporary mesh 
                    meshToDump = new Mesh
                    {
                        vertices = mesh.vertices,
                        colors = mesh.colors,
                        triangles = mesh.triangles,
                        normals = mesh.normals,
                        tangents = mesh.tangents
                    };
                    meshToDump.RecalculateBounds();
                    meshToDump.Optimize();
                }
                catch (Exception ex)
                {
                    Log.Error($"Workaround failed with error - {ex.Message}");
                    return;
                }
            }

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    OBJLoader.ExportOBJ(meshToDump.EncodeOBJ(), stream);
                    stream.Close();
                    Log.Warning($"Dumped mesh \"{((Mesh) mesh).name}\" to \"{fileName}\"");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error while trying to dump mesh \"{mesh.name}\" - {ex.Message}");
            }
        }
    }
}