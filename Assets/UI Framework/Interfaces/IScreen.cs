using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public interface IScreen<ControllerType> : IWindow where ControllerType : Controller<ControllerType>
    {
        ControllerType controller { get; }
        bool supportsHistory { get; }

        void Init(Controller<ControllerType> controller);

        bool SetBackButtonActive(bool active);
    }
}