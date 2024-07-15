namespace UIFramework
{
    public interface IScreen : INavigableWindow
    {
        Controller Controller { get; }

        ControllerType GetController<ControllerType>() where ControllerType : Controller;

        void Init(Controller controller);

        bool SetBackButtonActive(bool active);
    }
}