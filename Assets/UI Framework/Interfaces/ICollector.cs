namespace UIFramework
{
    public interface ICollector<out T> 
    {
        T[] Collect();
    }
}
