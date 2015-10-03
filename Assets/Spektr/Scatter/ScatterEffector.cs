//
// SpektrScatter - Polygon scatter effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using System.Collections.Generic;

namespace Spektr
{
    [ExecuteInEditMode]
    [AddComponentMenu("Spektr/Scatter Effector")]
    public class ScatterEffector : MonoBehaviour
    {
        #region Basic Properties

        public enum EffectType {
            Destruction,
            Disintegration
        }

        [SerializeField]
        EffectType _effectType;

        public EffectType effectType {
            get { return _effectType; }
            set { _effectType = value; }
        }

        [SerializeField]
        Vector3 _effectorSize = Vector3.one;

        public Vector3 effectorSize {
            get { return _effectorSize; }
            set { _effectorSize = value; }
        }

        [SerializeField, Range(0.1f, 8)]
        float _transitionSteepness = 1;

        public float transitionSteepness {
            get { return _transitionSteepness; }
            set { _transitionSteepness = value; }
        }

        #endregion

        #region Motion Properties

        [SerializeField]
        Vector3 _motionVector = Vector3.forward;

        public Vector3 motionVector {
            get { return _motionVector; }
            set { _motionVector = value; }
        }

        [SerializeField, Range(0, 1)]
        float _directionSpread = 0.2f;

        public float directionSpread {
            get { return _directionSpread; }
            set { _directionSpread = value; }
        }

        [SerializeField, Range(0, 1)]
        float _speedRandomness = 0.5f;

        public float speedRandomness {
            get { return _speedRandomness; }
            set { _speedRandomness = value; }
        }

        [SerializeField]
        float _turbulenceFrequency = 1;

        public float turbulenceFrequency {
            get { return _turbulenceFrequency; }
            set { _turbulenceFrequency = value; }
        }

        [SerializeField]
        float _turbulenceAmplitude = 0;

        public float turbulenceAmplitude {
            get { return _turbulenceAmplitude; }
            set { _turbulenceAmplitude = value; }
        }

        #endregion

        #region Noise To Position Properties

        [SerializeField]
        float _positionNoiseAmplitude = 0.05f;

        public float positionNoiseAmplitude {
            get { return _positionNoiseAmplitude; }
            set { _positionNoiseAmplitude = value; }
        }

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

        #endregion

        #region Rotation Properties 

        [SerializeField]
        float _rotationAngle = 600;

        public float rotationAngle {
            get { return _rotationAngle; }
            set { _rotationAngle = value; }
        }

        [SerializeField, Range(0, 1)]
        float _rotationRandomness = 0.2f;

        public float rotationRandomness {
            get { return _rotationRandomness; }
            set { _rotationRandomness = value; }
        }

        #endregion

        #region Noise To Rotation Properties

        [SerializeField]
        float _rotationNoiseAmplitude = 4.0f;

        public float rotationNoiseAmplitude {
            get { return _rotationNoiseAmplitude; }
            set { _rotationNoiseAmplitude = value; }
        }

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

        #endregion

        #region Other Parameters

        [SerializeField, ColorUsage(true, true, 0, 8, 0.125f, 3)]
        Color _initialEmission = Color.white;

        public Color initialEmission {
            get { return _initialEmission; }
            set { _initialEmission = value; }
        }

        [SerializeField, Range(0.1f, 20)]
        float _emissionTransitionSteepness = 8;

        public float emissionTransitionSteepness {
            get { return _emissionTransitionSteepness; }
            set { _emissionTransitionSteepness = value; }
        }

        [SerializeField]
        float _initialScale = 3.5f;

        public float initialScale {
            get { return _initialScale; }
            set { _initialScale = value; }
        }

        [SerializeField, Range(0.1f, 8)]
        float _scaleTransitionSteepness = 1;

        public float scaleTransitionSteepness {
            get { return _scaleTransitionSteepness; }
            set { _scaleTransitionSteepness = value; }
        }

        #endregion

        #region Private Properties And Functions

        [SerializeField]
        Shader _destructionShader;

        [SerializeField]
        Shader _disintegrationShader;

        public Shader shader {
            get {
                if (_effectType == EffectType.Destruction)
                    return _destructionShader;
                return _disintegrationShader;
            }
        }

        void SetDestructionProps(MaterialPropertyBlock block,
                                 Transform modelTransform)
        {
            var mv = transform.TransformDirection(_motionVector);
            mv = modelTransform.InverseTransformDirection(mv);

            block.SetVector("_MotionVector", new Vector4(
                mv.x, mv.y, mv.z, mv.magnitude
            ));

            block.SetVector("_MotionParams", new Vector4(
                _directionSpread,
                _speedRandomness,
                _turbulenceFrequency,
                _turbulenceAmplitude
            ));

            block.SetVector("_RotationParams", new Vector2(
                _rotationAngle * Mathf.Deg2Rad,
                _rotationRandomness
            ));
        }

        void SetDisintegrationProps(MaterialPropertyBlock block)
        {
            block.SetVector("_PNoise", new Vector3(
                _positionNoiseAmplitude,
                _positionNoiseFrequency,
                _positionNoiseSpeed
            ));

            block.SetVector("_RNoise", new Vector3(
                _rotationNoiseAmplitude,
                _rotationNoiseFrequency,
                _rotationNoiseSpeed
            ));
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
            var es = _effectorSize;
            var invs = new Vector3(1.0f / es.x, 1.0f / es.y, 1.0f / es.z);
            var e2n = Matrix4x4.Scale(invs);

            block.SetMatrix("_Effector", e2n * w2e * l2w);

            block.SetVector("_Steepness", new Vector3(
                _transitionSteepness,
                _emissionTransitionSteepness,
                _scaleTransitionSteepness
            ));

            block.SetColor("_InitialEmission", _initialEmission);
            block.SetFloat("_InitialScale", _initialScale);

            if (_effectType == EffectType.Destruction)
                SetDestructionProps(block, modelTransform);
            else
                SetDisintegrationProps(block);
        }

        #endregion

        #region MonoBehaviour Functions

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, _effectorSize);

            Gizmos.color = new Color(1, 1, 1, 0.4f);
            Gizmos.DrawCube(Vector3.zero, _effectorSize);
        }

        #endregion
    }
}
