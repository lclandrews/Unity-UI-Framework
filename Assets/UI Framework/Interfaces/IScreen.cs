namespace UIFramework
{
    public interface IScreen<ControllerType> : IWindow, INavigable where ControllerType : Controller<ControllerType>
    {
        ControllerType controller { get; }

        void Init(Controller<ControllerType> controller);

        bool SetBackButtonActive(bool active);
    }
}