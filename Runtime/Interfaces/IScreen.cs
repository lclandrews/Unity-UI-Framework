namespace UIFramework
{
    public interface IReadOnlyScreen : IReadOnlyNavigableWindow
    {
        
    }

    public interface IScreen : IReadOnlyScreen, INavigableWindow
    {
        Controller Controller { get; }

        ControllerType GetController<ControllerType>() where ControllerType : Controller;
        void Initialize(Controller controller);
        bool SetBackButtonActive(bool active);
    }
}