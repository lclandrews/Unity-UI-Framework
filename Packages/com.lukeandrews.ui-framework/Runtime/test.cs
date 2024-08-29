using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Editor
{
    [System.Serializable]
    public struct teststruct
    {
        public string name;
        public bool meh;
    }

    public class test : MonoBehaviour
    {
        public teststruct[] teststructarray;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
