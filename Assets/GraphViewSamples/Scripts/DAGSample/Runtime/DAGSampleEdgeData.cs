using System;
using UnityEngine;

namespace DAGSample.Runtime
{
    [Serializable]
    public class DAGSampleEdgeData : IEquatable<DAGSampleEdgeData>
    {
        [SerializeField] private string from;
        [SerializeField] private string to;
        
        public string From => from;
        public string To => to;
        
        public DAGSampleEdgeData(string from, string to)
        {
            this.from = from;
            this.to = to;
        }

        public bool Equals(DAGSampleEdgeData other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return from == other.from && to == other.to;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DAGSampleEdgeData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(from, to);
        }
    }
}