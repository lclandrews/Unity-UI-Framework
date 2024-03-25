namespace UIFramework
{
    public interface IScreen<ControllerType> : IWindow where ControllerType : Controller<ControllerType>
    {
        ControllerType controller { get; }
        bool supportsHistory { get; }
        ScreenTransition defaultTransition { get; }
        int sortOrder { get; set; }

        void Init(Controller<ControllerType> controller);

        bool SetBackButtonActive(bool active);
    }
}