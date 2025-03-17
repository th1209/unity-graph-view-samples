using System;
using System.Collections.Generic;
using DAGSample.Runtime;
using UnityEngine;

namespace DAGSample.Editor
{
    public class DAGSampleGraphAligner
    {
        public enum AlignDirection
        {
            Horizontal,
            Vertical,
        }

        private Vector2 _nodeOffset;
        private AlignDirection _direction;
        
        public DAGSampleGraphAligner(Vector2 nodeOffset, AlignDirection direction = AlignDirection.Horizontal)
        {
            _nodeOffset = nodeOffset;
            _direction = direction;
        }

        public void Align(DAGSampleGraphData graphData, Action<DAGSampleNodeData> onNodeAligned)
        {
            if (!graphData.IsAcyclic())
            {
                return;
            }
            var nodeAndDepth = CalculateNodeDepths(graphData);
            SetNodesPosition(nodeAndDepth, onNodeAligned);
        }

        private Dictionary<DAGSampleNodeData, int> CalculateNodeDepths(DAGSampleGraphData graphData)
        {
            var sortedNodes = graphData.GetSortedNodes();
            var nodeAndDepth = new Dictionary<DAGSampleNodeData, int>(sortedNodes.Count);

            // ※入次数の小さいノードから順番に処理したいため、トポロジカルソートされている必要がある
            foreach (var node in sortedNodes)
            {
                int depth = 0;
                foreach (var previousNode in graphData.GetPreviousNodes(node.Guid))
                {
                    depth = Math.Max(depth, nodeAndDepth[previousNode] + 1);
                }
                nodeAndDepth[node] = depth;
            }

            return nodeAndDepth;
        }

        private void SetNodesPosition(in Dictionary<DAGSampleNodeData, int> nodeAndDepth, Action<DAGSampleNodeData> onNodeAligned)
        {
            var nodeCountPerDepth = new Dictionary<int, int>(16);
            foreach (var pair in nodeAndDepth)
            {
                if (!nodeCountPerDepth.TryGetValue(pair.Value, out var countPerDepth))
                {
                    countPerDepth = 0;
                }
                var rect = pair.Key.Rect;
                var newPosition = _direction == AlignDirection.Horizontal 
                    ? new Vector2(pair.Value * _nodeOffset.x,  countPerDepth * _nodeOffset.y) 
                    : new Vector2(countPerDepth * _nodeOffset.y, pair.Value * _nodeOffset.x);
                pair.Key.Rect = new Rect(newPosition, rect.size);
                nodeCountPerDepth[pair.Value] = countPerDepth + 1;
                onNodeAligned?.Invoke(pair.Key);
            }
        }
    }
}