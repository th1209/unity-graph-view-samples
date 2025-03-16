using System;
using UnityEngine;

namespace DAGSample.Runtime
{
    [Serializable]
    public class DAGSampleNodeData
    {
        [SerializeField] private string name;
        [SerializeField] private string guid;
        [SerializeField] private Rect rect;

        public string Name => name;
        public string Guid => guid;
        public Rect Rect => rect;

        public DAGSampleNodeData(string name, Rect rect)
        {
            this.name = name;
            guid = System.Guid.NewGuid().ToString();
            this.rect = rect;
        }
    }
}