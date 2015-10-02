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
    [CustomEditor(typeof(ScatterRenderer))]
    public class ScatterRendererEditor : Editor
    {
        SerializedProperty _effector;

        SerializedProperty _mesh;
        SerializedProperty _materials;

        SerializedProperty _backFaceColor;
        SerializedProperty _backFaceMetallic;
        SerializedProperty _backFaceSmoothness;

        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        static GUIContent _textColor = new GUIContent("Color");
        static GUIContent _textMetallic = new GUIContent("Metallic");
        static GUIContent _textSmoothness = new GUIContent("Smoothness");

        void OnEnable()
        {
            _effector = serializedObject.FindProperty("_effector");

            _mesh = serializedObject.FindProperty("_mesh");
            _materials = serializedObject.FindProperty("_materials");

            _backFaceColor = serializedObject.FindProperty("_backFaceColor");
            _backFaceMetallic = serializedObject.FindProperty("_backFaceMetallic");
            _backFaceSmoothness = serializedObject.FindProperty("_backFaceSmoothness");

            _castShadows = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");
        }

        public override void OnInspectorGUI()
        {
            var instance = (ScatterRenderer)target;

            serializedObject.Update();

            EditorGUILayout.PropertyField(_effector);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_mesh);
            EditorGUILayout.PropertyField(_materials, true);
            if (EditorGUI.EndChangeCheck())
                instance.DisposeInternalObjects();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Back-face Material", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_backFaceColor, _textColor);
            EditorGUILayout.PropertyField(_backFaceMetallic, _textMetallic);
            EditorGUILayout.PropertyField(_backFaceSmoothness, _textSmoothness);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
