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
        private DAGSampleToolbar _toolbar;
        
        private DAGSampleGraphAligner _graphAligner;
        private DAGSampleCopyAndPaste _copyAndPaste;

        private HashSet<DAGSampleNode> _nodes = new HashSet<DAGSampleNode>(16);
        private HashSet<Edge> _edges = new HashSet<Edge>(16);

        private Dictionary<string, DAGSampleNode> _tempIdToNode = new Dictionary<string, DAGSampleNode>(16);

        public DAGSamplePresenter(DAGSampleGraphData graphData, DAGSampleGraphView graphView, DAGSampleToolbar toolbar)
        {
            _graphData = graphData;
            _graphView = graphView;
            _toolbar = toolbar;

            _graphAligner = new DAGSampleGraphAligner(new Vector2(150, 150));
 
            _graphView.CreateNodeEvent += OnCreateNode;
            _graphView.CreateEdgesEvent += OnCreateEdges;
            _graphView.RemoveElementsEvent += OnRemoveElements;
            _graphView.MoveElementsEvent += OnMoveElements;

            _copyAndPaste = new DAGSampleCopyAndPaste(CreateNode, SaveGraphData);
            _graphView.SerializeGraphElementsEvent += _copyAndPaste.OnSerializeGraphElements;
            _graphView.CanPasteSerializedDataEvent += _copyAndPaste.OnCanPasteSerializedData;
            _graphView.UnserializeAndPasteEvent += _copyAndPaste.OnUnserializeAndPaste;

            _toolbar.AlignButtonClickedEvent += AlignNodes;
            _toolbar.IsAcyclicButtonClickedEvent += _graphData.IsAcyclic;
            _toolbar.IsConnectedButtonClickedEvent += _graphData.IsConnected;
            
            Undo.undoRedoPerformed += ReloadGraphViewNodes;
            Undo.postprocessModifications += OnPostprocessModifications;
        }

        private Node OnCreateNode(Vector2 mousePosition)
        {
            var node =CreateNode(NodeTitle, new Rect(mousePosition, s_nodeSize));
            SaveGraphData();
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

        private void AlignNodes()
        {
            _graphAligner.Align(_graphData, (nodeData) =>
            {
                foreach (var node in _nodes)
                {
                    if (node.Guid == nodeData.Guid)
                    {
                        node.SetPosition(nodeData.Rect);
                    }
                }
            });
            SaveGraphData();
        }

        private Node CreateNode(string nodeTitle, Rect rect)
        {
            var nodeData = new DAGSampleNodeData(nodeTitle, rect);
            _graphData.Add(nodeData);

            var node = new DAGSampleNode(nodeTitle, nodeData.Guid, OnUpdateNode);
            node.Initialize();
            node.SetPosition(rect);
            _nodes.Add(node);

            return node;
        }

        private UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var mod in modifications)
            {
                if (mod.currentValue.target is DAGSampleGraphData graphData && graphData == _graphData)
                {
                    ReloadGraphViewNodes();
                }
            }
            return modifications;
        }

        public void Dispose()
        {
            Undo.postprocessModifications -= OnPostprocessModifications;
            Undo.undoRedoPerformed -= ReloadGraphViewNodes;

            if (_copyAndPaste != null)
            {
                if (_graphView != null)
                {
                    _graphView.UnserializeAndPasteEvent -= _copyAndPaste.OnUnserializeAndPaste;
                    _graphView.CanPasteSerializedDataEvent -= _copyAndPaste.OnCanPasteSerializedData;
                    _graphView.SerializeGraphElementsEvent -= _copyAndPaste.OnSerializeGraphElements;
                }

                _copyAndPaste.Dispose();
                _copyAndPaste = null;
            }
            
            if (_toolbar != null)
            {
                if (_graphData != null)
                {
                    _toolbar.IsConnectedButtonClickedEvent -= _graphData.IsConnected;
                    _toolbar.IsAcyclicButtonClickedEvent -= _graphData.IsAcyclic;
                }
                _toolbar.AlignButtonClickedEvent -= AlignNodes;
                _toolbar = null;
            }

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