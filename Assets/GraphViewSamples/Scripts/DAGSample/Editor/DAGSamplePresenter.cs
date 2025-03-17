using System;
using System.Collections.Generic;
using DAGSample.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DAGSample.Editor
{
    public class DAGSamplePresenter : IDisposable
    {
        private const string NodeTitle = "Node";
        private static readonly Vector2 s_nodeSize = new Vector2(100, 100);
        
        // Model
        private DAGSampleGraphData _graphData;
        // View
        private DAGSampleGraphView _graphView;

        private HashSet<Node> _nodes = new HashSet<Node>(16);
        private HashSet<Edge> _edges = new HashSet<Edge>(16);

        private Dictionary<string, DAGSampleNode> _tempIdToNode = new Dictionary<string, DAGSampleNode>(16);

        public DAGSamplePresenter(DAGSampleGraphData graphData, DAGSampleGraphView graphView)
        {
            _graphData = graphData;
            _graphView = graphView;
 
            _graphView.CreateNodeEvent += OnCreateNode;
            _graphView.CreateEdgesEvent += OnCreateEdges;
            _graphView.RemoveElementsEvent += OnRemoveElements;
            _graphView.MoveElementsEvent += OnMoveElements;

            Undo.undoRedoPerformed += ReloadGraphViewNodes;
        }

        private Node OnCreateNode(Vector2 mousePosition)
        {
            var rect = new Rect(mousePosition, s_nodeSize);
            var nodeData = new DAGSampleNodeData(NodeTitle, rect);
            _graphData.Add(nodeData);
            SaveGraphData();

            var node = new DAGSampleNode(NodeTitle, nodeData.Guid, OnUpdateNode);
            node.Initialize();
            node.SetPosition(rect);
            _nodes.Add(node);

            return node;
        }
        
        private void OnUpdateNode(DAGSampleNode node)
        {
            if (!_graphData.TryGetNode(node.Guid, out var nodeData))
            {
                return;
            }

            nodeData.Name = node.title;
            SaveGraphData();
        }

        private void OnCreateEdges(List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                var inputNode = edge.input.node as DAGSampleNode;
                var outputNode = edge.output.node as DAGSampleNode;
                if (inputNode == null || outputNode == null) continue;
                _graphData.Connect(outputNode.Guid, inputNode.Guid);
                _edges.Add(edge);
            }
            SaveGraphData();
        }

        private void OnRemoveElements(List<GraphElement> elements)
        {
            bool RemoveEdge(Edge edge)
            {
                var inputNode = edge.input.node as DAGSampleNode;
                var outputNode = edge.output.node as DAGSampleNode;
                if (inputNode == null || outputNode == null) return false;
                var result = _graphData.Disconnect(outputNode.Guid, inputNode.Guid);
                if (result)
                {
                    _edges.Remove(edge);
                }
                return result;
            }
            
            bool RemoveNode(DAGSampleNode node)
            {
                var result = _graphData.Remove(node.Guid);
                if (result)
                {
                    _nodes.Remove(node);
                }
                return result;
            }

            bool removed = false;
            foreach (var element in elements)
            {
                var edge = element as Edge;
                if (edge != null)
                {
                    removed |= RemoveEdge(edge); 
                }
            }
            
            foreach (var element in elements)
            {
                var node = element as DAGSampleNode;
                if (node != null)
                {
                    removed |= RemoveNode(node);
                }
            }

            if (removed)
            {
                SaveGraphData();
            }
        }

        private void OnMoveElements(List<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var node = element as DAGSampleNode;
                if (node != null && _graphData.TryGetNode(node.Guid, out var nodeData))
                {
                    nodeData.Name = node.title;
                    nodeData.Rect = node.GetPosition();
                }
            }
        }

        public void DeserializeGraphData(DAGSampleGraphData graphData)
        {
            _tempIdToNode.Clear();
            
            foreach (var nodeData in graphData.Nodes)
            {
                var node = new DAGSampleNode(nodeData.Name, nodeData.Guid, OnUpdateNode);
                node.Initialize();
                node.SetPosition(nodeData.Rect);
                _graphView.AddElement(node);
                _nodes.Add(node);
                _tempIdToNode[nodeData.Guid] = node;
            }
            
            foreach (var edgeData in graphData.Edges)
            {
                if (!_tempIdToNode.TryGetValue(edgeData.From, out var outputNode)) continue;
                if (!_tempIdToNode.TryGetValue(edgeData.To, out var inputNode)) continue;
                
                var edge = outputNode.OutputPort.ConnectTo(inputNode.InputPort);
                _graphView.AddElement(edge);
                _edges.Add(edge);
            }
        }

        private void SaveGraphData()
        {
            EditorUtility.SetDirty(_graphData);
            AssetDatabase.SaveAssets();
        }

        private void ReloadGraphViewNodes()
        {
            foreach (var node in _nodes)
            {
                _graphView.RemoveElement(node);
            }
            _nodes.Clear();
            
            foreach (var edge in _edges)
            {
                _graphView.RemoveElement(edge);
            }
            _edges.Clear();
            
            DeserializeGraphData(_graphData);
        }

        public void Dispose()
        {
            Undo.undoRedoPerformed -= ReloadGraphViewNodes;

            if (_graphView!= null)
            {
                _graphView.MoveElementsEvent -= OnMoveElements;
                _graphView.RemoveElementsEvent -= OnRemoveElements;
                _graphView.CreateEdgesEvent -= OnCreateEdges;
                _graphView.CreateNodeEvent -= OnCreateNode;
                _graphView = null;
            }

            _graphData = null;
        }
    }
}