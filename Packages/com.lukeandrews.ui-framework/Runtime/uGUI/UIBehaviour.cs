using System.Collections.Generic;

using UnityEngine;

namespace UIFramework.UGUI
{
    public abstract class UIBehaviour : MonoBehaviour, IUIBehaviour
    {
        public BehaviourState State { get; private set; } = BehaviourState.Uninitialized;

        public UIBehaviour Parent { get; private set; } = null;
        private HashSet<UIBehaviour> _children { get; set; } = new HashSet<UIBehaviour>();        


        // IUIBehaviour
        public bool IsValid()
        {
            return true;
        }

        public virtual void Initialize()
        {
            State = BehaviourState.Initialized;
        }

        public virtual void UpdateUI(float deltaTime)
        {
            foreach (UIBehaviour child in _children)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    child.UpdateUI(deltaTime);
                }
            }
        }

        public virtual void Terminate()
        {
            State = BehaviourState.Terminated;
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
                Parent = transform.parent.GetComponentInParent<UIBehaviour>(true);
                if (Parent != null)
                {
                    Parent.AddChild(this);
                }
            }
        }

        protected virtual void Start()
        {

        }                

        protected virtual void OnDestroy()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this);
            }
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // UGUIBehaviour
        protected virtual void AddChild(UIBehaviour child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }

        protected virtual void RemoveChild(UIBehaviour child)
        {
            if (_children.Contains(child))
            {
                _children.Remove(child);
            }
        }                
    }
}