using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DAGSample.Runtime
{
    [CreateAssetMenu(fileName = "graph_data", menuName = "ScriptableObjects/DAG Sample GraphData", order = 1)]
    public class DAGSampleGraphData : ScriptableObject
    {
        [SerializeField] private List<DAGSampleNodeData> nodes;
        [SerializeField] private List<DAGSampleEdgeData> edges;

        public IReadOnlyCollection<DAGSampleNodeData> Nodes => nodes;
        public IReadOnlyCollection<DAGSampleEdgeData> Edges => edges;

        private List<string> _tempVisited = new List<string>(16);
        private Dictionary<string, bool> _tempStack = new Dictionary<string, bool>(16);
        private readonly Queue<DAGSampleNodeData> _tempQueue = new Queue<DAGSampleNodeData>(16); 
        private readonly Dictionary<DAGSampleNodeData, int> _tempInDegree = new Dictionary<DAGSampleNodeData, int>(16);

        public DAGSampleGraphData(List<DAGSampleNodeData> nodes, List<DAGSampleEdgeData> edges)
        {
            this.nodes = new List<DAGSampleNodeData>(nodes);
            this.edges = new List<DAGSampleEdgeData>(edges);
        }
        
        public IReadOnlyCollection<DAGSampleNodeData> GetNodes()
        {
            return nodes;
        }
        
        public IReadOnlyCollection<DAGSampleNodeData> GetPreviousNodes(string nodeGuid)
        {
            var prevNodes = new List<DAGSampleNodeData>(nodes.Count);
            foreach (var edge in edges)
            {
                if (edge.To == nodeGuid)
                {
                    TryGetNode(edge.From, out var prevNode);
                    Assert.IsNotNull(prevNode);
                    prevNodes.Add(prevNode);
                }
            }
            return prevNodes;
        }
        
        public IReadOnlyCollection<DAGSampleNodeData> GetNextNodes(string nodeGuid)
        {
            var nextNodes = new List<DAGSampleNodeData>(nodes.Count);
            foreach (var edge in edges)
            {
                if (edge.From == nodeGuid)
                {
                    TryGetNode(edge.To, out var nextNode);
                    Assert.IsNotNull(nextNode);
                    nextNodes.Add(nextNode);
                }
            }
            return nextNodes;
        }

        public void Add(DAGSampleNodeData node)
        {
            if (Contains(node.Guid))
            {
                throw new InvalidOperationException();
            }
            Undo.RecordObject(this, $"{name} Add Node");
            nodes.Add(node);
        }
        
        public bool Remove(string nodeGuid)
        {
            TryGetNode(nodeGuid, out var node);
            if (node == null)
            {
                return false;
            }
            Undo.RecordObject(this, $"{name} Remove Node");
            return nodes.Remove(node);
        }
        
        public bool Contains(string nodeGuid)
        {
            return TryGetNode(nodeGuid, out _);
        }
        
        public void Connect(string fromGuid, string toGuid)
        {
            if (fromGuid == toGuid)
            {
                throw new InvalidOperationException();
            }

            if (!Contains(fromGuid) || !Contains(toGuid))
            {
                throw new InvalidOperationException();
            }
            
            if (TryGetEdge(fromGuid, toGuid, out  _))
            {
                throw new InvalidOperationException();
            }

            Undo.RecordObject(this, $"{name} Add Edge");
            edges.Add(new DAGSampleEdgeData(fromGuid, toGuid));
        }
        
        public bool Disconnect(string fromGuid, string toGuid)
        {
            if (fromGuid == toGuid)
            {
                throw new InvalidOperationException();
            }

            // ※ここではNodeより先にEdgeを削除する仕様とする
            if (!Contains(fromGuid) || !Contains(toGuid))
            {
                throw new InvalidOperationException();
            }

            if (TryGetEdge(fromGuid, toGuid, out var edge))
            {
                Undo.RecordObject(this, $"{name} Remove Edge");
                return edges.Remove(edge);
            }

            return false;
        }
        
        public bool TryGetNode(string nodeGuid, out DAGSampleNodeData node)
        {
            node = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.Guid == nodeGuid)
                {
                    node = n;
                    return true;
                }
            }
            return false;
        }
        
        private bool TryGetEdge(string fromGuid, string toGuid, out DAGSampleEdgeData edge)
        {
            edge = null;
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e.From == fromGuid && e.To == toGuid)
                {
                    edge = e;
                    return true;
                }
            }
            return false;
        }

        // 非循環グラフ条件を満たしているか
        public bool IsAcyclic()
        {
            _tempVisited.Clear();
            _tempStack.Clear();

            foreach (var node in nodes)
            {
                if (!_tempVisited.Contains(node.Guid) && DetectCycle(node, ref _tempVisited, ref _tempStack))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool DetectCycle(DAGSampleNodeData node, ref List<string> visited, ref Dictionary<string, bool> stack)
        {
            // ※次Nodeの走査中に同じNodeが現れた場合は循環しているのでNG
            if (stack.ContainsKey(node.Guid) && stack[node.Guid])
            {
                return true;
            }

            if (visited.Contains(node.Guid))
            {
                return false;
            }
        
            visited.Add(node.Guid);
            stack[node.Guid] = true;
    
            foreach (var nextNode in GetNextNodes(node.Guid))
            {
                if (DetectCycle(nextNode, ref visited, ref stack))
                {
                    return true;
                }
            }
        
            stack[node.Guid] = false;
            return false;
        }

        // 連結グラフ条件を満たしているか
        public bool IsConnected()
        {
            if (nodes.Count == 0)
            {
                return true;
            }
            
            _tempVisited.Clear();
            _tempQueue.Clear();

            _tempQueue.Enqueue(nodes[0]);

            while (_tempQueue.Count > 0)
            {
                var node = _tempQueue.Dequeue();
                if (_tempVisited.Contains(node.Guid))
                {
                    continue;
                }
                _tempVisited.Add(node.Guid);

                foreach (var nextNode in GetNextNodes(node.Guid))
                {
                        _tempQueue.Enqueue(nextNode);
                }

                // ※先頭Nodeが必ずしも入次数0のNodeとは限らないため、逆方向からの走査も必要
                foreach (var prevNode in GetPreviousNodes(node.Guid))
                {
                    _tempQueue.Enqueue(prevNode);
                }
            }

            return _tempVisited.Count == nodes.Count;
        }
        
        // トポロジカルソートされた状態のノード群を返す
        public IReadOnlyCollection<DAGSampleNodeData> GetSortedNodes()
        {
            if (!IsAcyclic())
            {
                throw new InvalidOperationException();
            }

            _tempInDegree.Clear();
            _tempQueue.Clear();
            var sortedNodes = new List<DAGSampleNodeData>(nodes.Count);

            // 各ノードの入次数を調べ、先頭ノード群から開始する
            foreach (var node in nodes)
            {
                int prevCount = GetPreviousNodes(node.Guid).Count;
                _tempInDegree[node] = prevCount;
                if (prevCount == 0)
                {
                    _tempQueue.Enqueue(node);
                }
            }

            while(_tempQueue.Count > 0)
            {
                // 入次数の少ないものからキューに積まれる
                var node = _tempQueue.Dequeue();
                sortedNodes.Add(node);

                // 次Nodeを調べ、入次数が0になるなら次回の処理対象に
                foreach (var nextNode in GetNextNodes(node.Guid))
                {
                    _tempInDegree[nextNode]--;
                    if (_tempInDegree[nextNode] == 0)
                    {
                        _tempQueue.Enqueue(nextNode);
                    }
                }
            }

            return sortedNodes;
        }
    }
}
