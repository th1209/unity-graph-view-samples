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

        public string Name { get => name; set => name = value; }
        public string Guid => guid;
        public Rect Rect  { get => rect; set => rect = value; }

        public DAGSampleNodeData(string name, Rect rect)
        {
            this.name = name;
            guid = System.Guid.NewGuid().ToString();
            this.rect = rect;
        }
    }
}