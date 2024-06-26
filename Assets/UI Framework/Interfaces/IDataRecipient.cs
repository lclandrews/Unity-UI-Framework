namespace UIFramework
{
    public interface IDataRecipient
    {
        void SetData(object data);
        bool IsValidData(object data);
    }
}