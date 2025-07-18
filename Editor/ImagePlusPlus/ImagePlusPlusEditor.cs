#if UNITY_EDITOR
using UIFramework.UGUI;

using UnityEditor;
using UnityEditor.UI;

using UnityEngine;

namespace UIFramework.Editor.UGUI
{
    [CustomEditor(typeof(ImagePlusPlus), true)]
    [CanEditMultipleObjects]
    public class ImagePlusPlusEditor : ImageEditor
    {
        private static GUIContent _autoCreateMaterialLabel = new GUIContent(
            "Auto Create Material",
            "Whether the componet should be responsible for automatically creating the appropriate material."
        );
        private static GUIContent _cornerModeLabel = new GUIContent(
            "Corner Mode",
            "Sets the style of the image corners: None, Rounded Locked, Rounded Independant."
        );
        private static GUIContent _roundingModeLabel = new GUIContent(
            "Rounding Mode",
            "Absolute: Absolute radius for corners in UI units, Relative: Percentage value of the images size."
        );
        private static GUIContent _lockedCornerRadiusLabel = new GUIContent(
            "Corner Radius",
            "The radius for all corners."
        );
        private static GUIContent _independateCornerRadiiLabel = new GUIContent(
            "Corner Radii",
            "The radii for each independant corner."
        );

        private static GUIContent _borderLabel = new GUIContent(
            "Border Width",
            "The width to draw the border outline."
        );

        private static GUIContent _borderColorLabel = new GUIContent(
            "Border Color",
            "The color of the border outline."
        );
        private static GUIContent _gradientFillLabel = new GUIContent(
            "Gradient Fill",
            "Enable gradient fill effect."
        );

        private static GUIContent _gradientColorLabel = new GUIContent(
            "Gradient Color",
            "The secondary color to blend to create the gradient fill."
        );

        private static GUIContent _gradientAngleLabel = new GUIContent(
            "Gradient Angle",
            "The angle to draw the gradient at."
        );

        private static GUIContent _cornerXLabel = new GUIContent("Top Left");
        private static GUIContent _cornerYLabel = new GUIContent("Top Right");
        private static GUIContent _cornerZLabel = new GUIContent("Bottom Left");
        private static GUIContent _cornerWLabel = new GUIContent("Bottom Right");

        private SerializedProperty _autoCreateMaterialProp = null;
        private SerializedProperty _cornerModeProp = null;
        private SerializedProperty _roundingModeProp = null;
        private SerializedProperty _lockedCornerRadiusProp = null;
        private SerializedProperty _independantCornerRadiiProp = null;

        private SerializedProperty _cornerXProp = null;
        private SerializedProperty _cornerYProp = null;
        private SerializedProperty _cornerZProp = null;
        private SerializedProperty _cornerWProp = null;

        private SerializedProperty _borderProp = null;
        private SerializedProperty _borderColorProp = null;

        private SerializedProperty _gradientFillProp = null;
        private SerializedProperty _gradientColorProp = null;
        private SerializedProperty _gradientAngleProp = null;

        private ImagePlusPlus _imagePlusPlus = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            _imagePlusPlus = serializedObject.targetObject as ImagePlusPlus;

            _autoCreateMaterialProp = serializedObject.FindProperty("_autoCreateMaterial");
            _cornerModeProp = serializedObject.FindProperty("_cornerMode");
            _roundingModeProp = serializedObject.FindProperty("_roundingMode");
            _lockedCornerRadiusProp = serializedObject.FindProperty("_lockedCornerRadius");
            _independantCornerRadiiProp = serializedObject.FindProperty("_independantCornerRadii");

            _cornerXProp = _independantCornerRadiiProp.FindPropertyRelative("x");
            _cornerYProp = _independantCornerRadiiProp.FindPropertyRelative("y");
            _cornerZProp = _independantCornerRadiiProp.FindPropertyRelative("z");
            _cornerWProp = _independantCornerRadiiProp.FindPropertyRelative("w");

            _borderProp = serializedObject.FindProperty("_border");
            _borderColorProp = serializedObject.FindProperty("_borderColor");

            _gradientFillProp = serializedObject.FindProperty("_gradientFill");
            _gradientColorProp = serializedObject.FindProperty("_gradientColor");
            _gradientAngleProp = serializedObject.FindProperty("_gradientAngle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_autoCreateMaterialProp, _autoCreateMaterialLabel);

            bool validMaterial = true;
            if (_imagePlusPlus.material == null)
            {
                EditorGUILayout.HelpBox("Material is null! Please set a material with the UI/ImagePlusPlus Shader.", MessageType.Warning);
                validMaterial = false;
            }
            else if (_imagePlusPlus.material.shader != ImagePlusPlus.Shader)
            {
                EditorGUILayout.HelpBox("Material does not use the correct shader! Please set the shader to UI/ImagePlusPlus Shader.", MessageType.Warning);
                validMaterial = false;
            }

            if (validMaterial)
            {
                EditorGUILayout.PropertyField(_cornerModeProp, _cornerModeLabel);

                ImagePlusPlus.CornerMode cornerMode = (ImagePlusPlus.CornerMode)_cornerModeProp.enumValueIndex;
                if (cornerMode != ImagePlusPlus.CornerMode.None)
                {
                    switch (cornerMode)
                    {
                        case ImagePlusPlus.CornerMode.RoundedIndependant:
                            EditorGUILayout.LabelField(_independateCornerRadiiLabel);
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(_cornerXProp, _cornerXLabel);
                            EditorGUILayout.PropertyField(_cornerYProp, _cornerYLabel);
                            EditorGUILayout.PropertyField(_cornerWProp, _cornerZLabel);
                            EditorGUILayout.PropertyField(_cornerZProp, _cornerWLabel);
                            EditorGUI.indentLevel--;
                            break;
                        case ImagePlusPlus.CornerMode.RoundedLocked:
                            EditorGUILayout.PropertyField(_lockedCornerRadiusProp, _lockedCornerRadiusLabel);
                            break;
                    }
                    EditorGUILayout.PropertyField(_roundingModeProp, _roundingModeLabel);
                    EditorGUILayout.PropertyField(_borderProp, _borderLabel);
                    EditorGUILayout.PropertyField(_borderColorProp, _borderColorLabel);
                }

                EditorGUILayout.PropertyField(_gradientFillProp, _gradientFillLabel);
                if (_gradientFillProp.boolValue)
                {
                    EditorGUILayout.PropertyField(_gradientColorProp, _gradientColorLabel);
                    EditorGUILayout.PropertyField(_gradientAngleProp, _gradientAngleLabel);
                }
            }

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif