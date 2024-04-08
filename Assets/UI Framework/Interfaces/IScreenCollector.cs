using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public interface IScreenCollector<ControllerType> where ControllerType : Controller<ControllerType>
    {
        void Collect(List<IScreen<ControllerType>> screens);

        IScreen<ControllerType>[] Collect();
    }
}
