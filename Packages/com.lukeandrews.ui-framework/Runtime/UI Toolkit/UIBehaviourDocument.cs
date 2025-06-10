using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class UIBehaviourDocument : MonoBehaviour
    {
        public UIDocument Document { get; private set; } = null;

        public event Action Enabled = default;
        public event Action Disabled = default;
        public event Action Destroyed = default;

        private void Awake()
        {
            Document = GetComponent<UIDocument>();
        }

        private void OnEnabled()
        {
            Enabled?.Invoke();
        }

        private void OnDisabled()
        {
            Disabled?.Invoke();
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}
