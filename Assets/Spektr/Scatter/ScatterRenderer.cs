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
    [AddComponentMenu("Spektr/Scatter Renderer")]
    public class ScatterRenderer : MonoBehaviour
    {
        #region Public Properties And Functions

        [SerializeField]
        ScatterEffector _effector;

        public ScatterEffector effector {
            get { return _effector; }
            set { _effector = value; }
        }

        [SerializeField]
        Mesh _mesh;

        public Mesh mesh {
            get { return _ownMesh != null ? _ownMesh : _mesh; }
            set { ClearOwnMesh(); _mesh = value; }
        }

        [SerializeField]
        Material[] _materials = new Material[1];

        public Material[] materials {
            get { return _ownMaterials != null ? _ownMaterials : _materials; }
            set { ClearOwnMaterials(); _materials = value; }
        }

        [SerializeField]
        Color _backFaceColor = Color.gray;

        public Color backFaceColor {
            get { return _backFaceColor; }
            set { _backFaceColor = value; }
        }

        [SerializeField, Range(0, 1)]
        float _backFaceMetallic = 0;

        public float backFaceMetallic {
            get { return _backFaceMetallic; }
            set { _backFaceMetallic = value; }
        }

        [SerializeField, Range(0, 1)]
        float _backFaceSmoothness = 0.2f;

        public float backFaceSmoothness {
            get { return _backFaceSmoothness; }
            set { _backFaceSmoothness = value; }
        }

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

        public void DisposeInternalObjects()
        {
            ClearOwnMesh();
            ClearOwnMaterials();
        }

        #endregion

        #region Private Variables

        MaterialPropertyBlock _materialOptions;
        ScatterEffector _foundEffector;

        ScatterEffector FindEffector()
        {
            if (_effector != null) return _effector;
            if (_foundEffector == null)
                _foundEffector = FindObjectOfType<ScatterEffector>();
            return _foundEffector;
        }

        #endregion

        #region Private Mesh Object

        Mesh _ownMesh;

        void ClearOwnMesh()
        {
            if (_ownMesh != null) {
                DestroyImmediate(_ownMesh);
                _ownMesh = null;
            }
        }

        Mesh GetScatterableMesh()
        {
            if (_mesh == null) return null;

            // Use own mesh if it already exists.
            if (_ownMesh) return _ownMesh;

            if (ScatterTool.CheckScatterable(_mesh))
            {
                // Use the given mesh if it's scatterable.
                return _mesh;
            }
            else
            {
                // Make scatterable clone and return it.
                _ownMesh = ScatterTool.MakeScatterableClone(_mesh);
                _ownMesh.hideFlags = HideFlags.DontSave;
                return _ownMesh;
            }
        }

        #endregion

        #region Material Handling

        Material[] _ownMaterials;

        bool CheckAllMaterialsScatterable()
        {
            foreach (var m in _materials)
                if (m != null && !ScatterTool.CheckScatterable(m)) return false;
            return true;
        }

        void ClearOwnMaterials()
        {
            if (_ownMaterials != null) {
                foreach (var m in _ownMaterials)
                    if (m != null) DestroyImmediate(m);
                _ownMaterials = null;
            }
        }

        Material[] GetScatterableMaterials()
        {
            if (_materials == null || _materials.Length == 0) return null;

            // Use own material list if it already exists.
            if (_ownMaterials != null && _ownMaterials.Length > 0) return _ownMaterials;

            if (CheckAllMaterialsScatterable())
            {
                // Use the given materials if all of them are scatterable.
                return _materials;
            }
            else
            {
                // Make scatterable clone and return it.
                _ownMaterials = new Material[_materials.Length];
                for (var i = 0; i < _materials.Length; i++)
                {
                    _ownMaterials[i] = ScatterTool.MakeScatterableClone(_materials[i]);
                    _ownMaterials[i].hideFlags = HideFlags.DontSave;
                }
                return _ownMaterials;
            }
        }

        #endregion

        #region MonoBehaviour Functions

        void OnDestroy()
        {
            ClearOwnMesh();
            ClearOwnMaterials();
        }

        void Update()
        {
            var mesh = GetScatterableMesh();
            var materials = GetScatterableMaterials();

            if (mesh == null || materials == null) return;

            if (_materialOptions == null)
                _materialOptions = new MaterialPropertyBlock();

            // model local space to world space matrix
            var l2w = transform.localToWorldMatrix;

            var effector = FindEffector();
            if (effector != null)
            {
                // world space to effector local space matrix
                var w2e = effector.transform.worldToLocalMatrix;

                // effector local space to normalized effector space matrix
                var es = effector.size;
                var e2n = Matrix4x4.Scale(new Vector3(1.0f / es.x, 1.0f / es.y, 1.0f / es.z));

                _materialOptions.SetMatrix("_Effector", e2n * w2e * l2w);

                _materialOptions.SetVector("_PNoise", new Vector3(
                    effector.positionNoiseFrequency,
                    effector.positionNoiseSpeed,
                    effector.positionNoiseAmplitude
                ));

                _materialOptions.SetVector("_RNoise", new Vector3(
                    effector.rotationNoiseFrequency,
                    effector.rotationNoiseSpeed,
                    effector.rotationNoiseAmplitude
                ));

                _materialOptions.SetFloat("_Inflation", effector.inflation);
            }

            _materialOptions.SetColor("_BackColor", _backFaceColor);
            _materialOptions.SetFloat("_BackMetallic", _backFaceMetallic);
            _materialOptions.SetFloat("_BackGlossiness", _backFaceSmoothness);

            var maxi = Mathf.Min(mesh.subMeshCount, materials.Length);
            for (var i = 0; i < maxi; i++)
            {
                Graphics.DrawMesh(
                    mesh, l2w,
                    materials[i], 0, null, i,
                    _materialOptions, _castShadows, _receiveShadows);
            }
        }

        #endregion
    }
}
