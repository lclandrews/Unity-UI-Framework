using UIFramework.UGUI;

using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.Editor.UGUI
{
    [CustomEditor(typeof(ButtonGraphicComponent), true)]
    public class ButtonGraphicComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty _targetGraphicProperty = null;

        private SerializedProperty _transitionProperty = null;

        private SerializedProperty _colorBlockProperty = null;
        private SerializedProperty _normalSpriteProperty = null;
        private SerializedProperty _spriteStateProperty = null;
        private SerializedProperty _animTriggerProperty = null;

        private SerializedProperty _statesProperty = null;
        private ButtonStatesReorderableList _statesList = null;

        private bool _showColorTint = false;
        private bool _showSpriteTrasition = false;
        private bool _showAnimTransition = false;

        protected virtual void OnEnable()
        {
            _targetGraphicProperty = serializedObject.FindProperty("_targetGraphic");

            _transitionProperty = serializedObject.FindProperty("_transition");

            _colorBlockProperty = serializedObject.FindProperty("_colors");
            _normalSpriteProperty = serializedObject.FindProperty("_normalSprite");
            _spriteStateProperty = serializedObject.FindProperty("_spriteState");
            _animTriggerProperty = serializedObject.FindProperty("_animationTriggers");

            _statesProperty = serializedObject.FindProperty("_states");
            _statesList = new ButtonStatesReorderableList(serializedObject, _statesProperty);

            Selectable.Transition transition = GetTransition(_transitionProperty);
            _showColorTint = (transition == Selectable.Transition.ColorTint);
            _showSpriteTrasition = (transition == Selectable.Transition.SpriteSwap);
            _showAnimTransition = (transition == Selectable.Transition.Animation);
        }

        static Selectable.Transition GetTransition(SerializedProperty transition)
        {
            return (Selectable.Transition)transition.enumValueIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Selectable.Transition transition = GetTransition(_transitionProperty);

            Graphic graphic = _targetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as ButtonGraphicComponent).GetComponent<Graphic>();

            Animator animator = (target as ButtonGraphicComponent).GetComponent<Animator>();
            _showColorTint= (!_transitionProperty.hasMultipleDifferentValues && transition == Button.Transition.ColorTint);
            _showSpriteTrasition = (!_transitionProperty.hasMultipleDifferentValues && transition == Button.Transition.SpriteSwap);
            _showAnimTransition = (!_transitionProperty.hasMultipleDifferentValues && transition == Button.Transition.Animation);            

            EditorGUILayout.PropertyField(_transitionProperty);

            ++EditorGUI.indentLevel;
            {
                if (transition == Selectable.Transition.ColorTint || transition == Selectable.Transition.SpriteSwap)
                {
                    EditorGUILayout.PropertyField(_targetGraphicProperty);
                }

                switch (transition)
                {
                    case Selectable.Transition.ColorTint:
                        if (graphic == null)
                            EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Warning);
                        break;

                    case Selectable.Transition.SpriteSwap:
                        if (graphic as Image == null)
                            EditorGUILayout.HelpBox("You must have a Image target in order to use a sprite swap transition.", MessageType.Warning);
                        break;
                }

                if(_showColorTint)
                {
                    EditorGUILayout.PropertyField(_colorBlockProperty);
                }

                if (_showSpriteTrasition)
                {
                    EditorGUILayout.PropertyField(_normalSpriteProperty);
                    EditorGUILayout.PropertyField(_spriteStateProperty);
                }

                if (_showAnimTransition)
                {
                    EditorGUILayout.PropertyField(_animTriggerProperty);

                    if (animator == null || animator.runtimeAnimatorController == null)
                    {
                        Rect buttonRect = EditorGUILayout.GetControlRect();
                        buttonRect.xMin += EditorGUIUtility.labelWidth;
                        if (GUI.Button(buttonRect, "Auto Generate Animation", EditorStyles.miniButton))
                        {
                            var controller = GenerateSelectableAnimatorContoller((target as ButtonGraphicComponent).AnimationTriggers, target as ButtonGraphicComponent);
                            if (controller != null)
                            {
                                if (animator == null)
                                    animator = (target as ButtonGraphicComponent).gameObject.AddComponent<Animator>();

                                AnimatorController.SetAnimatorController(animator, controller);
                            }
                        }
                    }
                }
            }
            --EditorGUI.indentLevel;

            _statesList.ShowColorTint = _showColorTint;
            _statesList.ShowSpriteTransition = _showSpriteTrasition;
            _statesList.ShowAnimTransition = _showAnimTransition;
            _statesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private static AnimatorController GenerateSelectableAnimatorContoller(AnimationTriggers animationTriggers, ButtonGraphicComponent target)
        {
            if (target == null)
                return null;

            // Where should we create the controller?
            var path = GetSaveControllerPath(target);
            if (string.IsNullOrEmpty(path))
                return null;

            // figure out clip names
            var normalName = string.IsNullOrEmpty(animationTriggers.normalTrigger) ? "Normal" : animationTriggers.normalTrigger;
            var highlightedName = string.IsNullOrEmpty(animationTriggers.highlightedTrigger) ? "Highlighted" : animationTriggers.highlightedTrigger;
            var pressedName = string.IsNullOrEmpty(animationTriggers.pressedTrigger) ? "Pressed" : animationTriggers.pressedTrigger;
            var selectedName = string.IsNullOrEmpty(animationTriggers.selectedTrigger) ? "Selected" : animationTriggers.selectedTrigger;
            var disabledName = string.IsNullOrEmpty(animationTriggers.disabledTrigger) ? "Disabled" : animationTriggers.disabledTrigger;

            // Create controller and hook up transitions.
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            GenerateTriggerableTransition(normalName, controller);
            GenerateTriggerableTransition(highlightedName, controller);
            GenerateTriggerableTransition(pressedName, controller);
            GenerateTriggerableTransition(selectedName, controller);
            GenerateTriggerableTransition(disabledName, controller);

            AssetDatabase.ImportAsset(path);

            return controller;
        }

        private static string GetSaveControllerPath(ButtonGraphicComponent target)
        {
            var defaultName = target.gameObject.name;
            var message = string.Format("Create a new animator for the game object '{0}':", defaultName);
            return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "controller", message);
        }

        private static AnimationClip GenerateTriggerableTransition(string name, AnimatorController controller)
        {
            // Create the clip
            var clip = AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(clip, controller);

            // Create a state in the animatior controller for this clip
            var state = controller.AddMotion(clip);

            // Add a transition property
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

            // Add an any state transition
            var stateMachine = controller.layers[0].stateMachine;
            var transition = stateMachine.AddAnyStateTransition(state);
            transition.AddCondition(AnimatorConditionMode.If, 0, name);
            return clip;
        }
    }
}
