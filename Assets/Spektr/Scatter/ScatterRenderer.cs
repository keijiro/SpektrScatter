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
using UnityEngine.Rendering;

namespace Spektr
{
    [ExecuteInEditMode]
    [AddComponentMenu("Spektr/Scatter/Renderer")]
    public class ScatterRenderer : MonoBehaviour
    {
        #region Public Properties

        [SerializeField]
        ScatterEffector _effector;

        [SerializeField]
        Mesh _mesh;

        [SerializeField]
        Material[] _materials = new Material[1];

        [SerializeField]
        ShadowCastingMode _castShadows;

        public ShadowCastingMode castShadows {
            get { return _castShadows; }
            set { _castShadows = value; }
        }

        [SerializeField]
        bool _receiveShadows = false;

        public bool receiveShadows {
            get { return _receiveShadows; }
            set { _receiveShadows = value; }
        }

        #endregion

        #region Private Properties

        MaterialPropertyBlock _materialOptions;

        #endregion

        #region MonoBehaviour Functions

        void Update()
        {
            if (_mesh == null) return;

            if (_materialOptions == null)
                _materialOptions = new MaterialPropertyBlock();

            var l2w = transform.localToWorldMatrix;
            var w2l = transform.worldToLocalMatrix;

            if (_effector != null)
            {
                var axis = w2l * _effector.transform.up;
                var origin = Vector3.Dot(w2l * _effector.transform.position, axis);

                var axis_origin = new Vector4(axis.x, axis.y, axis.z, origin);
                _materialOptions.SetVector("_EffectorAxis", axis_origin);
                _materialOptions.SetVector("_EffectorSize", _effector.size);

                _materialOptions.SetVector("_PNoise", new Vector3(
                    _effector.positionNoiseFrequency,
                    _effector.positionNoiseSpeed,
                    _effector.positionNoiseAmplitude
                ));

                _materialOptions.SetVector("_RNoise", new Vector3(
                    _effector.rotationNoiseFrequency,
                    _effector.rotationNoiseSpeed,
                    _effector.rotationNoiseAmplitude
                ));

                _materialOptions.SetFloat("_Inflation", _effector.inflation);
            }

            var maxi = Mathf.Min(_mesh.subMeshCount, _materials.Length);
            for (var i = 0; i < maxi; i++)
            {
                Graphics.DrawMesh(
                    _mesh, l2w,
                    _materials[i], 0, null, i,
                    _materialOptions, _castShadows, _receiveShadows);
            }
        }

        #endregion
    }
}
