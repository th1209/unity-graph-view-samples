using UnityEngine;
using UnityEditor;

namespace MiniSample.NodeContainers.Editor
{
    public class NodeContainersGraphWindow : EditorWindow
    {
        private NodeContainersGraphView _sampleGraphView;

        [MenuItem("Window/Mini GraphView Samples/Node Containers", false, 2)]
        public static void OpenWindow()
        {
            var window = GetWindow<NodeContainersGraphWindow>();
            window.titleContent = new GUIContent("Node Containers Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new NodeContainersGraphView();
            rootVisualElement.Add(_sampleGraphView);
        }
    }
}