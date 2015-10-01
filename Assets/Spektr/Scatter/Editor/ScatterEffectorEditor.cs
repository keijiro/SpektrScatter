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
using UnityEditor;

namespace Spektr
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScatterEffector))]
    public class ScatterEffectorEditor : Editor
    {
        SerializedProperty _size;

        SerializedProperty _positionNoiseFrequency;
        SerializedProperty _positionNoiseSpeed;
        SerializedProperty _positionNoiseAmplitude;

        SerializedProperty _rotationNoiseFrequency;
        SerializedProperty _rotationNoiseSpeed;
        SerializedProperty _rotationNoiseAmplitude;

        SerializedProperty _inflation;

        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textSpeed     = new GUIContent("Speed");
        static GUIContent _textAmplitude = new GUIContent("Amplitude");

        void OnEnable()
        {
            _size = serializedObject.FindProperty("_size");

            _positionNoiseFrequency = serializedObject.FindProperty("_positionNoiseFrequency");
            _positionNoiseSpeed     = serializedObject.FindProperty("_positionNoiseSpeed");
            _positionNoiseAmplitude = serializedObject.FindProperty("_positionNoiseAmplitude");

            _rotationNoiseFrequency = serializedObject.FindProperty("_rotationNoiseFrequency");
            _rotationNoiseSpeed     = serializedObject.FindProperty("_rotationNoiseSpeed");
            _rotationNoiseAmplitude = serializedObject.FindProperty("_rotationNoiseAmplitude");

            _inflation = serializedObject.FindProperty("_inflation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_size);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Noise To Position", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_positionNoiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_positionNoiseSpeed, _textSpeed);
            EditorGUILayout.PropertyField(_positionNoiseAmplitude, _textAmplitude);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Noise To Rotation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_rotationNoiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_rotationNoiseSpeed, _textSpeed);
            EditorGUILayout.PropertyField(_rotationNoiseAmplitude, _textAmplitude);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_inflation);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
