namespace UIFramework
{
    public interface IWindowData
    {
        bool requiresData { get; }
        object data { get; }

        bool SetData(object data);
        bool IsValidData(object data);
    }
}