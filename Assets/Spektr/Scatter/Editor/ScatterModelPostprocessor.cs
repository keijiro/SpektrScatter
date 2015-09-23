//
// Scatter - polygon scattering effect
//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Spektr
{
    //
    // Custom model postprocessor
    //
    // It splits up all the consolidated vertices so that each triangle
    // consists of individual vertices. It also embeds the centroid of each
    // triangle into UV1.
    //
    // This postprocessor is applied only to models with the suffix
    // "scatterable" (case-insensitive).
    //
    public class ScatterModelPostprocessor : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject go)
        {
            var filename = Path.GetFileNameWithoutExtension(assetPath);
            if (filename.ToLower().EndsWith("scatterable"))
            {
                Debug.Log("Making scatterable: " + go.name);
                foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
                    MakeScatterable(meshFilter.sharedMesh);
            }
        }

        void MakeScatterable(Mesh mesh)
        {
            var ia_i = mesh.triangles;
            var vcount = ia_i.Length;

            {
                var va_o = new Vector3[vcount];
                var na_o = new Vector3[vcount];
                var ta_o = new Vector4[vcount];
                var uv_o = new Vector2[vcount];
                var ca_o = new List<Vector3>(vcount);

                {
                    var va_i = mesh.vertices;
                    var na_i = mesh.normals;
                    var ta_i = mesh.tangents;
                    var uv_i = mesh.uv;

                    for (var i = 0; i < vcount; i++)
                    {
                        var vi = ia_i[i];
                        va_o[i] = va_i[vi];
                        na_o[i] = na_i[vi];
                        ta_o[i] = ta_i[vi];
                        uv_o[i] = uv_i[vi];
                    }
                }

                for (var i = 0; i < vcount; i += 3)
                {
                    var c = (va_o[i] + va_o[i + 1] + va_o[i + 2]) / 3;
                    ca_o.Add(c);
                    ca_o.Add(c);
                    ca_o.Add(c);
                }

                mesh.vertices = va_o;
                mesh.normals = na_o;
                mesh.tangents = ta_o;
                mesh.uv = uv_o;
                mesh.SetUVs(1, ca_o);
            }

            var vi2 = 0;
            for (var smi = 0; smi < mesh.subMeshCount; smi++)
            {
                var sia_i = mesh.GetTriangles(smi);
                var sia_o = new int[sia_i.Length];
                for (var i = 0; i < sia_o.Length; i++) sia_o[i] = vi2++;
                mesh.SetTriangles(sia_o, smi);
            }
        }
    }
}
