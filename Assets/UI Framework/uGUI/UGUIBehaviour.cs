using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public abstract class UGUIBehaviour : MonoBehaviour, IUIBehaviour
    {
        public UGUIBehaviour parent { get; private set; } = null;
        private HashSet<UGUIBehaviour> _children { get; set; } = new HashSet<UGUIBehaviour>();


        // IUIBehaviour
        public virtual void UpdateUI(float deltaTime)
        {
            foreach (UGUIBehaviour child in _children)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    child.UpdateUI(deltaTime);
                }
            }
        }

        // Unity Messages
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            
        }
#endif

        protected virtual void Awake()
        {
            if (transform.parent != null)
            {
                parent = transform.parent.GetComponentInParent<UGUIBehaviour>();
                if (parent != null)
                {
                    parent.AddChild(this);
                }
            }
        }

        protected virtual void Start()
        {

        }                

        protected virtual void OnDestroy()
        {
            if (parent != null)
            {
                parent.RemoveChild(this);
            }
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // UGUIBehaviour
        protected virtual void AddChild(UGUIBehaviour child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }

        protected virtual void RemoveChild(UGUIBehaviour child)
        {
            if (_children.Contains(child))
            {
                _children.Remove(child);
            }
        }
    }
}