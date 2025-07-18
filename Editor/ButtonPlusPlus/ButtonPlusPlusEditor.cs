using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIFramework.UGUI;

using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.Editor.UGUI
{
    [CustomEditor(typeof(ButtonPlusPlus), true)]
    [CanEditMultipleObjects]
    public class ButtonPlusPlusEditor : UnityEditor.Editor
    {
        private SerializedProperty _script = null;

        private SerializedProperty _interactableProperty = null;

        private SerializedProperty _targetGraphicProperty = null;

        private SerializedProperty _transitionProperty = null;

        private SerializedProperty _colorBlockProperty = null;
        private SerializedProperty _normalSpriteProperty = null;
        private SerializedProperty _spriteStateProperty = null;
        private SerializedProperty _animTriggerProperty = null;

        private SerializedProperty _statesProperty = null;
        private ButtonStatesReorderableList _statesList = null;

        private SerializedProperty _navigationProperty = null;

        private SerializedProperty _onClickProperty = null;

        private GUIContent _visualizeNavigation = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");

        bool _showColorTint = false;
        bool _showSpriteTrasition = false;
        bool _showAnimTransition = false;

        private static List<ButtonPlusPlusEditor> _editors = new List<ButtonPlusPlusEditor>();
        private static bool _showNavigation = false;
        private static string _showNavigationKey = "SelectableEditor.ShowNavigation";

        // Whenever adding new SerializedProperties to the Selectable and SelectableEditor
        // Also update this guy in OnEnable. This makes the inherited classes from Selectable not require a CustomEditor.
        private string[] _propertyPathToExcludeForChildClasses;

        protected virtual void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _interactableProperty = serializedObject.FindProperty("m_Interactable");
            _targetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            _transitionProperty = serializedObject.FindProperty("m_Transition");
            _colorBlockProperty = serializedObject.FindProperty("m_Colors");
            _normalSpriteProperty = serializedObject.FindProperty("_normalSprite");
            _spriteStateProperty = serializedObject.FindProperty("m_SpriteState");
            _animTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");

            _statesProperty = serializedObject.FindProperty("_states");
            _statesList = new ButtonStatesReorderableList(serializedObject, _statesProperty);

            _navigationProperty = serializedObject.FindProperty("m_Navigation");

            _onClickProperty = serializedObject.FindProperty("m_OnClick");

            _propertyPathToExcludeForChildClasses = new[]
            {
                _script.propertyPath,
                _navigationProperty.propertyPath,
                _transitionProperty.propertyPath,
                _colorBlockProperty.propertyPath,
                _normalSpriteProperty.propertyPath,
                _spriteStateProperty.propertyPath,
                _animTriggerProperty.propertyPath,
                _interactableProperty.propertyPath,
                _targetGraphicProperty.propertyPath,
                _onClickProperty.propertyPath,
                _statesProperty.propertyPath
            };

            var trans = GetTransition(_transitionProperty);
            _showColorTint = (trans == Selectable.Transition.ColorTint);
            _showSpriteTrasition = (trans == Selectable.Transition.SpriteSwap);
            _showAnimTransition = (trans == Selectable.Transition.Animation);

            _editors.Add(this);
            RegisterStaticOnSceneGUI();

            _showNavigation = EditorPrefs.GetBool(_showNavigationKey);            
        }

        protected virtual void OnDisable()
        {
            _editors.Remove(this);
            RegisterStaticOnSceneGUI();
        }

        private void RegisterStaticOnSceneGUI()
        {
            SceneView.duringSceneGui -= StaticOnSceneGUI;
            if (_editors.Count > 0)
                SceneView.duringSceneGui += StaticOnSceneGUI;
        }

        static Selectable.Transition GetTransition(SerializedProperty transition)
        {
            return (Selectable.Transition)transition.enumValueIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_interactableProperty);

            var trans = GetTransition(_transitionProperty);

            var graphic = _targetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as Selectable).GetComponent<Graphic>();

            var animator = (target as Selectable).GetComponent<Animator>();
            _showColorTint = (!_transitionProperty.hasMultipleDifferentValues && trans == Button.Transition.ColorTint);
            _showSpriteTrasition = (!_transitionProperty.hasMultipleDifferentValues && trans == Button.Transition.SpriteSwap);
            _showAnimTransition = (!_transitionProperty.hasMultipleDifferentValues && trans == Button.Transition.Animation);

            EditorGUILayout.PropertyField(_transitionProperty);

            ++EditorGUI.indentLevel;
            {
                if (trans == Selectable.Transition.ColorTint || trans == Selectable.Transition.SpriteSwap)
                {
                    EditorGUILayout.PropertyField(_targetGraphicProperty);
                }

                switch (trans)
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

                if (_showColorTint)
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
                            var controller = GenerateSelectableAnimatorContoller((target as Selectable).animationTriggers, target as Selectable);
                            if (controller != null)
                            {
                                if (animator == null)
                                    animator = (target as Selectable).gameObject.AddComponent<Animator>();

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

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_navigationProperty);

            EditorGUI.BeginChangeCheck();
            Rect toggleRect = EditorGUILayout.GetControlRect();
            toggleRect.xMin += EditorGUIUtility.labelWidth;
            _showNavigation = GUI.Toggle(toggleRect, _showNavigation, _visualizeNavigation, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(_showNavigationKey, _showNavigation);
                SceneView.RepaintAll();
            }

            // We do this here to avoid requiring the user to also write a Editor for their Selectable-derived classes.
            // This way if we are on a derived class we dont draw anything else, otherwise draw the remaining properties.
            ChildClassPropertiesGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onClickProperty);
            serializedObject.ApplyModifiedProperties();
        }

        // Draw the extra SerializedProperties of the child class.
        // We need to make sure that m_PropertyPathToExcludeForChildClasses has all the Selectable properties and in the correct order.
        // TODO: find a nicer way of doing this. (creating a InheritedEditor class that automagically does this)
        private void ChildClassPropertiesGUI()
        {
            if (IsDerivedSelectableEditor())
                return;

            DrawPropertiesExcluding(serializedObject, _propertyPathToExcludeForChildClasses);
        }

        private bool IsDerivedSelectableEditor()
        {
            return GetType() != typeof(ButtonPlusPlusEditor);
        }

        private static AnimatorController GenerateSelectableAnimatorContoller(AnimationTriggers animationTriggers, Selectable target)
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

        private static string GetSaveControllerPath(Selectable target)
        {
            var defaultName = target.gameObject.name;
            var message = string.Format("Create a new animator for the game object '{0}':", defaultName);
            return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "controller", message);
        }

        private static void SetUpCurves(AnimationClip highlightedClip, AnimationClip pressedClip, string animationPath)
        {
            string[] channels = { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" };

            var highlightedKeys = new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.1f), new Keyframe(1f, 1f) };
            var highlightedCurve = new AnimationCurve(highlightedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(highlightedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), highlightedCurve);

            var pressedKeys = new[] { new Keyframe(0f, 1.15f) };
            var pressedCurve = new AnimationCurve(pressedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(pressedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), pressedCurve);
        }

        private static string BuildAnimationPath(Selectable target)
        {
            // if no target don't hook up any curves.
            var highlight = target.targetGraphic;
            if (highlight == null)
                return string.Empty;

            var startGo = highlight.gameObject;
            var toFindGo = target.gameObject;

            var pathComponents = new Stack<string>();
            while (toFindGo != startGo)
            {
                pathComponents.Push(startGo.name);

                // didn't exist in hierarchy!
                if (startGo.transform.parent == null)
                    return string.Empty;

                startGo = startGo.transform.parent.gameObject;
            }

            // calculate path
            var animPath = new StringBuilder();
            if (pathComponents.Count > 0)
                animPath.Append(pathComponents.Pop());

            while (pathComponents.Count > 0)
                animPath.Append("/").Append(pathComponents.Pop());

            return animPath.ToString();
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

        private static void StaticOnSceneGUI(SceneView view)
        {
            if (!_showNavigation)
                return;

            Selectable[] selectables = Selectable.allSelectablesArray;

            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable s = selectables[i];
                if (UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(s.gameObject, Camera.current))
                    DrawNavigationForSelectable(s);
            }
        }

        private static void DrawNavigationForSelectable(Selectable sel)
        {
            if (sel == null)
                return;

            Transform transform = sel.transform;
            bool active = Selection.transforms.Any(e => e == transform);

            Handles.color = new Color(1.0f, 0.6f, 0.2f, active ? 1.0f : 0.4f);
            DrawNavigationArrow(-Vector2.right, sel, sel.FindSelectableOnLeft());
            DrawNavigationArrow(Vector2.up, sel, sel.FindSelectableOnUp());

            Handles.color = new Color(1.0f, 0.9f, 0.1f, active ? 1.0f : 0.4f);
            DrawNavigationArrow(Vector2.right, sel, sel.FindSelectableOnRight());
            DrawNavigationArrow(-Vector2.up, sel, sel.FindSelectableOnDown());
        }

        const float kArrowThickness = 2.5f;
        const float kArrowHeadSize = 1.2f;

        private static void DrawNavigationArrow(Vector2 direction, Selectable fromObj, Selectable toObj)
        {
            if (fromObj == null || toObj == null)
                return;
            Transform fromTransform = fromObj.transform;
            Transform toTransform = toObj.transform;

            Vector2 sideDir = new Vector2(direction.y, -direction.x);
            Vector3 fromPoint = fromTransform.TransformPoint(GetPointOnRectEdge(fromTransform as RectTransform, direction));
            Vector3 toPoint = toTransform.TransformPoint(GetPointOnRectEdge(toTransform as RectTransform, -direction));
            float fromSize = HandleUtility.GetHandleSize(fromPoint) * 0.05f;
            float toSize = HandleUtility.GetHandleSize(toPoint) * 0.05f;
            fromPoint += fromTransform.TransformDirection(sideDir) * fromSize;
            toPoint += toTransform.TransformDirection(sideDir) * toSize;
            float length = Vector3.Distance(fromPoint, toPoint);
            Vector3 fromTangent = fromTransform.rotation * direction * length * 0.3f;
            Vector3 toTangent = toTransform.rotation * -direction * length * 0.3f;

            Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, Handles.color, null, kArrowThickness);
            Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction - sideDir) * toSize * kArrowHeadSize);
            Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction + sideDir) * toSize * kArrowHeadSize);
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }
    }
}
