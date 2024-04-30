namespace UIFramework
{
    public interface IDataRecipient
    {
        bool requiresData { get; }
        object data { get; }

        void SetData(object data);
        bool IsValidData(object data);
    }
}