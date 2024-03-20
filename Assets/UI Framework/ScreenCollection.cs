using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class ScreenCollection<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public Dictionary<Type, IScreen<ControllerType>> dictionary = null;
        public IScreen<ControllerType>[] array = null;

        private ScreenCollection() { }

        public ScreenCollection(Dictionary<Type, IScreen<ControllerType>> dictionary, IScreen<ControllerType>[] array)
        {
            this.dictionary = dictionary;
            this.array = array;
        }
    }
}