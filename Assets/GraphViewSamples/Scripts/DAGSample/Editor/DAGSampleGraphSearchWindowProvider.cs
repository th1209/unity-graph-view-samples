using System;
using System.Collections.Generic;
using DAGSample.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DAGSample.Editor
{
    public class DAGSampleGraphSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private GraphViewEditorWindow _window;
        private GraphView _graphView;

        private Func<Vector2, Node> _nodeFactory;

        private readonly List<SearchTreeEntry> _entries = new List<SearchTreeEntry>(2);

        public void Initialize(GraphViewEditorWindow window, GraphView graphView, Func<Vector2, Node> nodeFactory)
        {
            _window = window;
            _graphView = graphView;
            _nodeFactory = nodeFactory;

            _entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            _entries.Add(new SearchTreeEntry(new GUIContent("DAG Node"))
            {
                // Levelで階層を指定する
                level = 1,
                // 任意のObjectをここに設定できる。これを使って特定のSearchEntryが呼ばれた際に好きな処理ができるようにしている.
                userData = typeof(DAGSampleNode),
            });
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return _entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as Type;
            if (type == null || type != typeof(DAGSampleNode))
            {
                return false;
            }

            if (_nodeFactory == null)
            {
                return false;
            }

            var mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            var node = _nodeFactory.Invoke(graphMousePosition);
            if (node != null)
            {
                _graphView.AddElement(node);
                return true;
            }

            return false;
        }
    }
}