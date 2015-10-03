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
using UnityEditor;

namespace Spektr
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScatterEffector))]
    public class ScatterEffectorEditor : Editor
    {
        SerializedProperty _effectType;
        SerializedProperty _effectorSize;
        SerializedProperty _transitionSteepness;

        SerializedProperty _motionVector;
        SerializedProperty _directionSpread;
        SerializedProperty _speedRandomness;

        SerializedProperty _turbulenceFrequency;
        SerializedProperty _turbulenceAmplitude;

        SerializedProperty _positionNoiseAmplitude;
        SerializedProperty _positionNoiseFrequency;
        SerializedProperty _positionNoiseSpeed;

        SerializedProperty _rotationAngle;
        SerializedProperty _rotationRandomness;

        SerializedProperty _rotationNoiseAmplitude;
        SerializedProperty _rotationNoiseFrequency;
        SerializedProperty _rotationNoiseSpeed;

        SerializedProperty _initialEmission;
        SerializedProperty _emissionTransitionSteepness;
        SerializedProperty _initialScale;
        SerializedProperty _scaleTransitionSteepness;

        static GUIContent _textAngle = new GUIContent("Angle");
        static GUIContent _textAmplitude = new GUIContent("Amplitude");
        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textRandomness = new GUIContent("Randomness");
        static GUIContent _textSpeed = new GUIContent("Speed");
        static GUIContent _textTransitionSteepness = new GUIContent("Transition Steepness");

        void OnEnable()
        {
            _effectType = serializedObject.FindProperty("_effectType");
            _effectorSize = serializedObject.FindProperty("_effectorSize");
            _transitionSteepness = serializedObject.FindProperty("_transitionSteepness");

            _motionVector = serializedObject.FindProperty("_motionVector");
            _directionSpread = serializedObject.FindProperty("_directionSpread");
            _speedRandomness = serializedObject.FindProperty("_speedRandomness");

            _turbulenceFrequency = serializedObject.FindProperty("_turbulenceFrequency");
            _turbulenceAmplitude = serializedObject.FindProperty("_turbulenceAmplitude");

            _positionNoiseAmplitude = serializedObject.FindProperty("_positionNoiseAmplitude");
            _positionNoiseFrequency = serializedObject.FindProperty("_positionNoiseFrequency");
            _positionNoiseSpeed = serializedObject.FindProperty("_positionNoiseSpeed");

            _rotationAngle = serializedObject.FindProperty("_rotationAngle");
            _rotationRandomness = serializedObject.FindProperty("_rotationRandomness");

            _rotationNoiseAmplitude = serializedObject.FindProperty("_rotationNoiseAmplitude");
            _rotationNoiseFrequency = serializedObject.FindProperty("_rotationNoiseFrequency");
            _rotationNoiseSpeed = serializedObject.FindProperty("_rotationNoiseSpeed");

            _initialEmission = serializedObject.FindProperty("_initialEmission");
            _emissionTransitionSteepness = serializedObject.FindProperty("_emissionTransitionSteepness");
            _initialScale = serializedObject.FindProperty("_initialScale");
            _scaleTransitionSteepness = serializedObject.FindProperty("_scaleTransitionSteepness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var showDestructionProps =
                _effectType.hasMultipleDifferentValues ||
                _effectType.enumValueIndex == 0;

            var showDisintegrationProps =
                _effectType.hasMultipleDifferentValues ||
                _effectType.enumValueIndex == 1;

            EditorGUILayout.PropertyField(_effectType);
            EditorGUILayout.PropertyField(_effectorSize);
            EditorGUILayout.PropertyField(_transitionSteepness);

            EditorGUILayout.Space();

            if (showDestructionProps)
            {
                EditorGUILayout.LabelField("Directional Motion", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_motionVector);
                EditorGUILayout.PropertyField(_directionSpread);
                EditorGUILayout.PropertyField(_speedRandomness);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Turbulent Motion", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_turbulenceFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_turbulenceAmplitude, _textAmplitude);
                EditorGUILayout.Space();
            }

            if (showDisintegrationProps)
            {
                EditorGUILayout.LabelField("Noise To Position", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_positionNoiseAmplitude, _textAmplitude);
                EditorGUILayout.PropertyField(_positionNoiseFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_positionNoiseSpeed, _textSpeed);
                EditorGUILayout.Space();
            }

            if (showDestructionProps)
            {
                EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_rotationAngle, _textAngle);
                EditorGUILayout.PropertyField(_rotationRandomness, _textRandomness);
                EditorGUILayout.Space();
            }

            if (showDisintegrationProps)
            {
                EditorGUILayout.LabelField("Noise To Rotation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_rotationNoiseAmplitude, _textAmplitude);
                EditorGUILayout.PropertyField(_rotationNoiseFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_rotationNoiseSpeed, _textSpeed);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Optional Animation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_initialEmission);
            EditorGUILayout.PropertyField(_emissionTransitionSteepness, _textTransitionSteepness);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_initialScale);
            EditorGUILayout.PropertyField(_scaleTransitionSteepness, _textTransitionSteepness);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
