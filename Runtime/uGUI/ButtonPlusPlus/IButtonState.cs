namespace UIFramework.UGUI
{
    public interface IButtonState
    {
        string State { get; }

        public void ResetButtonState();
        public void SetButtonState(string state);
    }
}
