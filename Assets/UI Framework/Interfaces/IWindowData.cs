namespace UIFramework
{
    public interface IWindowData
    {
        bool requiresData { get; }
        object data { get; }

        void SetData(object data);
        bool IsValidData(object data);
    }
}