using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.Editor.UGUI
{
    public class ButtonStatesReorderableList
    {
        public bool ShowColorTint = false;
        public bool ShowSpriteTransition = false;
        public bool ShowAnimTransition = false;

        private SerializedProperty _serializedProperty = null;
        private UnityEditorInternal.ReorderableList _reorderableList = null;

        private const string _defaultNormalAnimName = "Normal";
        private const string _defaultHighlightedAnimName = "Highlighted";
        private const string _defaultPressedAnimName = "Pressed";
        private const string _defaultSelectedAnimName = "Selected";
        private const string _defaultDisabledAnimName = "Disabled";

        public ButtonStatesReorderableList(SerializedObject serializedObject, SerializedProperty serializedProperty)
        {
            if(serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }
            _serializedProperty = serializedProperty;
            _reorderableList = new UnityEditorInternal.ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            _reorderableList.drawElementCallback += DrawElement;
            _reorderableList.drawHeaderCallback += DrawHeader;
            _reorderableList.elementHeightCallback += ElementHeight;
            _reorderableList.onAddCallback += OnAdd;
        }

        public void DoLayoutList()
        {
            _reorderableList.DoLayoutList();
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Additional States");
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = _serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty nameProperty = property.FindPropertyRelative("_name");

            property.isExpanded = EditorGUI.Foldout(
                new Rect(rect.x + EditorStyles.foldout.padding.left, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                string.IsNullOrEmpty(nameProperty.stringValue) ? $"Element {index}" : nameProperty.stringValue
            );

            rect.Indent(2);
            rect.NewLine();
            rect.Space();

            if (property.isExpanded)
            {
                nameProperty.stringValue = EditorGUI.TextField(
                    rect.Property(true),
                    new GUIContent("Name", ""),
                    nameProperty.stringValue
                );

                if (ShowColorTint)
                {
                    SerializedProperty colorBlockProperty = property.FindPropertyRelative("_colors");
                    EditorGUI.PropertyField(
                        rect.PropertyBlock(colorBlockProperty, true),
                        colorBlockProperty
                    );
                }

                if (ShowSpriteTransition)
                {
                    SerializedProperty normalSpriteProperty = property.FindPropertyRelative("_normalSprite");
                    EditorGUI.PropertyField(
                        rect.Property(true),
                        normalSpriteProperty
                    );
                    SerializedProperty spriteStateProperty = property.FindPropertyRelative("_spriteState");
                    EditorGUI.PropertyField(
                        rect.PropertyBlock(spriteStateProperty, true),
                        spriteStateProperty
                    );
                }

                if (ShowAnimTransition)
                {
                    SerializedProperty animTriggerProperty = property.FindPropertyRelative("_animationTriggers");
                    EditorGUI.PropertyField(
                        rect.PropertyBlock(animTriggerProperty, true),
                        animTriggerProperty
                    );
                }
            }
        }

        private float ElementHeight(int index)
        {
            SerializedProperty property = _serializedProperty.GetArrayElementAtIndex(index);

            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                // Add for name property
                height += Layout.StandardVerticalHeight;
                if (ShowColorTint)
                {
                    SerializedProperty colorBlockProperty = property.FindPropertyRelative("_colors");
                    height += colorBlockProperty.DefaultHeight(true);
                }

                if (ShowSpriteTransition)
                {
                    height += Layout.StandardVerticalHeight;
                    SerializedProperty spriteStateProperty = property.FindPropertyRelative("_spriteState");
                    height += spriteStateProperty.DefaultHeight(true);
                }

                if (ShowAnimTransition)
                {
                    SerializedProperty animTriggerProperty = property.FindPropertyRelative("_animationTriggers");
                    height += animTriggerProperty.DefaultHeight(true);
                }
            }
            return height;
        }

        private void OnAdd(UnityEditorInternal.ReorderableList list)
        {
            Debug.Log(_serializedProperty.arraySize);
            _serializedProperty.InsertArrayElementAtIndex(_serializedProperty.arraySize);
            Debug.Log(_serializedProperty.arraySize);
            SerializedProperty property = _serializedProperty.GetArrayElementAtIndex(_serializedProperty.arraySize - 1);
            SerializedProperty nameProperty = property.FindPropertyRelative("_name");
            nameProperty.stringValue = "";
            SerializedProperty colorBlockProperty = property.FindPropertyRelative("_colors");
            colorBlockProperty.boxedValue = ColorBlock.defaultColorBlock;
            SerializedProperty animTriggerProperty = property.FindPropertyRelative("_animationTriggers");
            AnimationTriggers animTriggers = animTriggerProperty.boxedValue as AnimationTriggers;
            animTriggers.normalTrigger = _defaultNormalAnimName;
            animTriggers.highlightedTrigger = _defaultHighlightedAnimName;
            animTriggers.pressedTrigger = _defaultPressedAnimName;
            animTriggers.selectedTrigger = _defaultSelectedAnimName;
            animTriggers.disabledTrigger = _defaultDisabledAnimName;
        }
    }
}
