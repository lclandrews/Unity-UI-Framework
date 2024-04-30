using System.Collections.Generic;

namespace UIFramework
{
    public interface IScreenCollector<ControllerType> where ControllerType : Controller<ControllerType>
    {
        void Collect(List<IScreen<ControllerType>> screens);

        IScreen<ControllerType>[] Collect();
    }
}
