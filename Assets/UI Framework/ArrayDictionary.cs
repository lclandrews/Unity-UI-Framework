using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class ArrayDictionary<T>
    {
        public T[] array { get; private set; } = null;
        public Dictionary<Type, T> dictionary { get; private set; } = null;        

        private ArrayDictionary() { }

        public ArrayDictionary(T[] array)
        {
            if(array == null)
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