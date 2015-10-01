//
// Scatter - polygon scattering effect
//
using UnityEngine;
using UnityEditor;

namespace Spektr
{
    // custom material editor
    public class ScatterStandardMaterialEditor : ShaderGUI
    {
        MaterialProperty _color;
        MaterialProperty _mainTex;

        MaterialProperty _glossiness;
        MaterialProperty _metallic;
        MaterialProperty _metallicGlossMap;

        MaterialProperty _bumpScale;
        MaterialProperty _bumpMap;

        MaterialProperty _occlusionStrength;
        MaterialProperty _occlusionMap;

        MaterialProperty _emissionColor;
        MaterialProperty _emissionMap;

        MaterialProperty _transitionBase;
        MaterialProperty _transitionSpeed;
        MaterialProperty _transitionTime;

        MaterialProperty _transitionAxisPitch;
        MaterialProperty _transitionAxisYaw;

        MaterialProperty _backColor;
        MaterialProperty _backGlossiness;
        MaterialProperty _backMetallic;

        static GUIContent _albedoText     = new GUIContent("Albedo");
        static GUIContent _metallicText   = new GUIContent("Metallic");
        static GUIContent _smoothnessText = new GUIContent("Smoothness");
        static GUIContent _normalMapText  = new GUIContent("Normal Map");
        static GUIContent _occlusionText  = new GUIContent("Occlusion");
        static GUIContent _emissionText   = new GUIContent("Emission");

        static ColorPickerHDRConfig _colorPickerHDRConfig = new ColorPickerHDRConfig(0, 99, 1.0f / 99, 3);

        bool _initial = true;

        void FindProperties(MaterialProperty[] props)
        {
            _color   = FindProperty("_Color", props);
            _mainTex = FindProperty("_MainTex", props);

            _glossiness       = FindProperty("_Glossiness", props);
            _metallic         = FindProperty("_Metallic", props);
            _metallicGlossMap = FindProperty("_MetallicGlossMap", props);

            _bumpScale = FindProperty("_BumpScale", props);
            _bumpMap   = FindProperty("_BumpMap", props);

            _occlusionStrength = FindProperty("_OcclusionStrength", props);
            _occlusionMap      = FindProperty("_OcclusionMap", props);

            _emissionColor = FindProperty("_EmissionColor", props);
            _emissionMap   = FindProperty("_EmissionMap", props);

            _backColor      = FindProperty("_BackColor", props);
            _backGlossiness = FindProperty("_BackGlossiness", props);
            _backMetallic   = FindProperty("_BackMetallic", props);

            _transitionAxisYaw   = FindProperty("_TransitionAxisYaw", props);
            _transitionAxisPitch = FindProperty("_TransitionAxisPitch", props);

            _transitionBase  = FindProperty("_TransitionBase", props);
            _transitionSpeed = FindProperty("_TransitionSpeed", props);
            _transitionTime  = FindProperty("_TransitionTime", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            if (ShaderPropertiesGUI(materialEditor) || _initial)
                foreach (Material m in materialEditor.targets)
                    SetMaterialKeywords(m);

            _initial = false;
        }

        bool ShaderPropertiesGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();

            // albedo
            materialEditor.TexturePropertySingleLine(_albedoText, _mainTex, _color);

            // metallic / smoothness
            if (_metallicGlossMap.textureValue == null)
                materialEditor.TexturePropertyTwoLines(_metallicText, _metallicGlossMap, _metallic, _smoothnessText, _glossiness);
            else
                materialEditor.TexturePropertySingleLine(_metallicText, _metallicGlossMap);

            // normal map
            materialEditor.TexturePropertySingleLine(_normalMapText, _bumpMap, _bumpMap.textureValue != null ? _bumpScale : null);

            // occlusion
            materialEditor.TexturePropertySingleLine(_occlusionText, _occlusionMap, _occlusionMap.textureValue != null ? _occlusionStrength : null);

            // emission
            bool hadEmissionTexture = _emissionMap.textureValue != null;
            materialEditor.TexturePropertyWithHDRColor(_emissionText, _emissionMap, _emissionColor, _colorPickerHDRConfig, false);

            // if texture was assigned and color was black set color to white
            if (_emissionMap.textureValue != null && !hadEmissionTexture)
                if (_emissionColor.colorValue.maxColorComponent <= 0)
                    _emissionColor.colorValue = Color.white;

            EditorGUILayout.Space();

            // backface properties
            EditorGUILayout.LabelField("Backface Properties");
            materialEditor.ShaderProperty(_backColor, "Color");
            materialEditor.ShaderProperty(_backMetallic, "Matallic");
            materialEditor.ShaderProperty(_backGlossiness, "Smoothness");

            EditorGUILayout.Space();

            // scatter effect parameters
            EditorGUILayout.LabelField("Transition");
            //materialEditor.ShaderProperty(_transitionAxis, "Axis");
            materialEditor.ShaderProperty(_transitionAxisYaw, "Axis Yaw");
            materialEditor.ShaderProperty(_transitionAxisPitch, "Axis Pitch");
            materialEditor.ShaderProperty(_transitionBase, "Base Position");
            materialEditor.ShaderProperty(_transitionSpeed, "Speed");
            materialEditor.ShaderProperty(_transitionTime, "Time");

            return EditorGUI.EndChangeCheck();
        }

        static void SetMaterialKeywords(Material material)
        {
            SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));

            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

            var emissive = material.GetColor("_EmissionColor").maxColorComponent > 0.1f / 255;
            SetKeyword(material, "_EMISSION", emissive);

            var yaw = material.GetFloat("_TransitionAxisYaw") * Mathf.Deg2Rad;
            var pitch = material.GetFloat("_TransitionAxisPitch") * Mathf.Deg2Rad;

            var ax = Mathf.Cos(pitch) * Mathf.Cos(yaw);
            var ay = Mathf.Sin(pitch);
            var az = Mathf.Cos(pitch) * -Mathf.Sin(yaw);

            material.SetVector("_TransitionAxis", new Vector3(ax, ay, az));
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
}
