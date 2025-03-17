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

        private List<string> _tempVisited;
        private List<string> TempVisited => _tempVisited ??= new List<string>(nodes.Count);

        private Dictionary<string, bool> _tempStack;
        private Dictionary<string, bool> TempStack => _tempStack ??= new Dictionary<string, bool>(nodes.Count);

        private Queue<DAGSampleNodeData> _tempQueue; 
        private Queue<DAGSampleNodeData> TempQueue => _tempQueue ??= new Queue<DAGSampleNodeData>(nodes.Count);

        private Dictionary<DAGSampleNodeData, int> _tempInDegree;
        private Dictionary<DAGSampleNodeData, int> TempInDegree => _tempInDegree ??= new Dictionary<DAGSampleNodeData, int>(nodes.Count);

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
            TempVisited.Clear();
            TempStack.Clear();

            foreach (var node in nodes)
            {
                if (!TempVisited.Contains(node.Guid) && DetectCycle(node, TempVisited, TempStack))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool DetectCycle(DAGSampleNodeData node, List<string> visited, Dictionary<string, bool> stack)
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
                if (DetectCycle(nextNode, visited, stack))
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
            
            TempVisited.Clear();
            TempQueue.Clear();

            TempQueue.Enqueue(nodes[0]);

            while (TempQueue.Count > 0)
            {
                var node = TempQueue.Dequeue();
                if (TempVisited.Contains(node.Guid))
                {
                    continue;
                }
                TempVisited.Add(node.Guid);

                foreach (var nextNode in GetNextNodes(node.Guid))
                {
                        TempQueue.Enqueue(nextNode);
                }

                // ※先頭Nodeが必ずしも入次数0のNodeとは限らないため、逆方向からの走査も必要
                foreach (var prevNode in GetPreviousNodes(node.Guid))
                {
                    TempQueue.Enqueue(prevNode);
                }
            }

            return TempVisited.Count == nodes.Count;
        }
        
        // トポロジカルソートされた状態のノード群を返す
        public IReadOnlyCollection<DAGSampleNodeData> GetSortedNodes()
        {
            if (!IsAcyclic())
            {
                throw new InvalidOperationException();
            }

            TempInDegree.Clear();
            TempQueue.Clear();
            var sortedNodes = new List<DAGSampleNodeData>(nodes.Count);

            // 各ノードの入次数を調べ、先頭ノード群から開始する
            foreach (var node in nodes)
            {
                int prevCount = GetPreviousNodes(node.Guid).Count;
                TempInDegree[node] = prevCount;
                if (prevCount == 0)
                {
                    TempQueue.Enqueue(node);
                }
            }

            while(TempQueue.Count > 0)
            {
                // 入次数の少ないものからキューに積まれる
                var node = TempQueue.Dequeue();
                sortedNodes.Add(node);

                // 次Nodeを調べ、入次数が0になるなら次回の処理対象に
                foreach (var nextNode in GetNextNodes(node.Guid))
                {
                    TempInDegree[nextNode]--;
                    if (TempInDegree[nextNode] == 0)
                    {
                        TempQueue.Enqueue(nextNode);
                    }
                }
            }

            return sortedNodes;
        }
    }
}
