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
    [ExecuteInEditMode]
    [AddComponentMenu("Spektr/Scatter Effector")]
    public class ScatterEffector : MonoBehaviour
    {
        #region Effector Properties

        // range parameters

        [SerializeField]
        Vector3 _size = Vector3.one;

        public Vector3 size {
            get { return _size; }
            set { _size = value; }
        }

        // noise-to-position parameters

        [SerializeField]
        float _positionNoiseFrequency = 5.0f;

        public float positionNoiseFrequency {
            get { return _positionNoiseFrequency; }
            set { _positionNoiseFrequency = value; }
        }

        [SerializeField]
        float _positionNoiseSpeed = 2.0f;

        public float positionNoiseSpeed {
            get { return _positionNoiseSpeed; }
            set { _positionNoiseSpeed = value; }
        }

        [SerializeField]
        float _positionNoiseAmplitude = 0.05f;

        public float positionNoiseAmplitude {
            get { return _positionNoiseAmplitude; }
            set { _positionNoiseAmplitude = value; }
        }

        // noise-to-rotation parameters

        [SerializeField]
        float _rotationNoiseFrequency = 2.0f;

        public float rotationNoiseFrequency {
            get { return _rotationNoiseFrequency; }
            set { _rotationNoiseFrequency = value; }
        }

        [SerializeField]
        float _rotationNoiseSpeed = 1.2f;

        public float rotationNoiseSpeed {
            get { return _rotationNoiseSpeed; }
            set { _rotationNoiseSpeed = value; }
        }

        [SerializeField]
        float _rotationNoiseAmplitude = 4.0f;

        public float rotationNoiseAmplitude {
            get { return _rotationNoiseAmplitude; }
            set { _rotationNoiseAmplitude = value; }
        }

        // scaling parameters

        [SerializeField]
        float _inflation = 3.5f;

        public float inflation {
            get { return _inflation; }
            set { _inflation = value; }
        }

        #endregion

        #region Private Resources

        [SerializeField]
        Shader _shader;

        public Shader shader {
            get { return _shader; }
        }

        #endregion

        #region Public Functions

        public void SetPropertyBlock(MaterialPropertyBlock block,
                                     Transform modelTransform)
        {
            // model local space to world space matrix
            var l2w = modelTransform.localToWorldMatrix;

            // world space to effector local space matrix
            var w2e = transform.worldToLocalMatrix;

            // effector local space to normalized effector space matrix
            var invs = new Vector3(1.0f / _size.x, 1.0f / _size.y, 1.0f / _size.z);
            var e2n = Matrix4x4.Scale(invs);

            block.SetMatrix("_Effector", e2n * w2e * l2w);

            block.SetVector("_PNoise", new Vector3(
                _positionNoiseFrequency,
                _positionNoiseSpeed,
                _positionNoiseAmplitude
            ));

            block.SetVector("_RNoise", new Vector3(
                _rotationNoiseFrequency,
                _rotationNoiseSpeed,
                _rotationNoiseAmplitude
            ));

            block.SetFloat("_Inflation", _inflation);
        }

        #endregion

        #region MonoBehaviour Functions

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, _size);

            Gizmos.color = new Color(1, 1, 1, 0.4f);
            Gizmos.DrawCube(Vector3.zero, _size);
        }

        #endregion
    }
}
