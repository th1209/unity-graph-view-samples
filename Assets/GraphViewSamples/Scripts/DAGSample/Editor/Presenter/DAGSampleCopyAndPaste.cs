using System;
using System.Collections.Generic;
using DAGSample.Editor.View;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DAGSample.Editor.Presenter
{
    public class DAGSampleCopyAndPaste : IDisposable
    {
        [Serializable]
        private class IntermediateGraph
        {
            public IntermediateNode[] nodes;
        }

        [Serializable]
        private class IntermediateNode
        {
            public string title;
            public Rect rect;
        }

        private static Vector2 s_positionOffset = new Vector2(50, 50);
        
        private Func<string, Rect, Node> _nodeFactory;
        private Action _pasteFinished;
        private int _pasteCount = 0;
        private IntermediateGraph _graph;

        public DAGSampleCopyAndPaste(Func<string, Rect, Node> nodeFactoryMethod, Action pasteFinished)
        {
            _nodeFactory = nodeFactoryMethod;
            _pasteFinished = pasteFinished;
        }

        public string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            _pasteCount = 0;

            var nodes = new List<IntermediateNode>(16);
            foreach (var element in elements)
            {
                if (element is DAGSampleNode node)
                {
                    nodes.Add(new IntermediateNode
                    {
                        title = node.title,
                        rect = node.GetPosition()
                    });
                }
            }
            var graph = new IntermediateGraph
            {
                nodes = nodes.ToArray()
            };
            return JsonUtility.ToJson(graph);
        }
        
        public bool OnCanPasteSerializedData(string data)
        {
            try
            {
                _graph = JsonUtility.FromJson<IntermediateGraph>(data);
            }
            catch (ArgumentException e)
            {
                Debug.LogException(e);
                return false;
            }
            return !string.IsNullOrEmpty(data);
        }
        
        public void OnUnserializeAndPaste(string data)
        {
            if (_graph == null)
            {
                return;
            }

            foreach (var node in _graph.nodes)
            {
                UnityEngine.Debug.Log($"title:{node.title} rect:{node.rect.x},{node.rect.y},{node.rect.width},{node.rect.height}");
                // 元ノードより少しずらした位置にCopy&Pasteする
                var position = node.rect.position + s_positionOffset * (_pasteCount + 1);
                _nodeFactory(node.title, new Rect(position, node.rect.size));
            }

            // 複数回Pasteされた場合の考慮のため、ペースト回数を覚えておく
            _pasteCount++;
            _graph = null;

            _pasteFinished?.Invoke();
        }

        public void Dispose()
        {
            _pasteFinished = null;
            _nodeFactory = null;
        }
    }
}