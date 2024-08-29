using UnityEngine.UI;

namespace UIFramework.UGUI
{
    public enum SelectionState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    public interface IButtonComponent : IButtonState
    {
        public void DoStateTransition(SelectionState selectionState, bool instant);
    }
}
