using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class ObjectTypeMap<T>
    {
        public T[] array { get; private set; } = null;
        public Dictionary<Type, T> dictionary { get; private set; } = null;

        private ObjectTypeMap() { }

        public ObjectTypeMap(List<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            this.array = list.ToArray();
            dictionary = new Dictionary<Type, T>(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                Type type = array[i].GetType();
                if (!dictionary.ContainsKey(type))
                {
                    dictionary.Add(type, array[i]);
                }
                else
                {
                    throw new InvalidOperationException("Multiple instances of the same array element have been found, " +
                        "please ensure all instances are of a unique type.");
                }
            }
        }

        public ObjectTypeMap(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            this.array = array;
            dictionary = new Dictionary<Type, T>(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                Type type = array[i].GetType();
                if (!dictionary.ContainsKey(type))
                {
                    dictionary.Add(type, array[i]);
                }
                else
                {
                    throw new InvalidOperationException("Multiple instances of the same array element have been found, " +
                        "please ensure all instances are of a unique type.");
                }
            }
        }
    }
}