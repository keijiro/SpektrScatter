//
// SpektrScatter - Polygon scatter effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using System.Collections.Generic;

namespace Spektr
{
    public static class ScatterTool
    {
        const string _suffix = " (scatterable)";
        const string _shaderName = "Spektr/Scatter/Standard";

        static public bool CheckScatterable(Mesh mesh)
        {
            return mesh.name.EndsWith(_suffix);
        }

        static public bool CheckScatterable(Material material)
        {
            return material.shader.name == _shaderName;
        }

        static public Mesh MakeScatterableClone(Mesh mesh)
        {
            var newMesh = new Mesh();
            newMesh.name = mesh.name + _suffix;
            newMesh.bounds = mesh.bounds;
            newMesh.subMeshCount = mesh.subMeshCount;
            MakeScatterable(mesh, newMesh);
            return newMesh;
        }

        static public Material MakeScatterableClone(Material material)
        {
            var newMaterial = new Material(material);
            newMaterial.shader = Shader.Find(_shaderName);
            return newMaterial;
        }

        static public void MakeScatterableInplace(Mesh mesh)
        {
            MakeScatterable(mesh, mesh);
            mesh.name += _suffix;
        }

        static public void MakeScatterableInplace(Material material)
        {
            material.shader = Shader.Find(_shaderName);
        }

        //
        // Mesh converter
        //
        // It splits up all the consolidated vertices so that each triangle
        // consists of individual vertices. It also embeds the centroid of each
        // triangle into UV1.
        //
        // This postprocessor is applied only to models with the suffix
        // "scatterable" (case-insensitive).
        //
        static void MakeScatterable(Mesh srcMesh, Mesh dstMesh)
        {
            var ia_i = srcMesh.triangles;
            var vcount = ia_i.Length;

            {
                var va_o = new Vector3[vcount];
                var na_o = new Vector3[vcount];
                var ta_o = new Vector4[vcount];
                var uv_o = new Vector2[vcount];
                var ca_o = new List<Vector3>(vcount);

                {
                    var va_i = srcMesh.vertices;
                    var na_i = srcMesh.normals;
                    var ta_i = srcMesh.tangents;
                    var uv_i = srcMesh.uv;

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

                dstMesh.vertices = va_o;
                dstMesh.normals = na_o;
                dstMesh.tangents = ta_o;
                dstMesh.uv = uv_o;
                dstMesh.SetUVs(1, ca_o);
            }

            var vi2 = 0;
            for (var smi = 0; smi < srcMesh.subMeshCount; smi++)
            {
                var sia_i = srcMesh.GetTriangles(smi);
                var sia_o = new int[sia_i.Length];
                for (var i = 0; i < sia_o.Length; i++) sia_o[i] = vi2++;
                dstMesh.SetTriangles(sia_o, smi);
            }
        }
    }
}

