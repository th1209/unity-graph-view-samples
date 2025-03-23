using UnityEngine;
using UnityEditor;

namespace MiniSample.NodeFields.Editor
{
    public class NodeFieldsGraphWindow : EditorWindow
    {
        private NodeFieldsGraphView _sampleGraphView;

        [MenuItem("Window/Mini GraphView Samples/Node Fields", false, 1)]
        public static void OpenWindow()
        {
            var window = GetWindow<NodeFieldsGraphWindow>();
            window.titleContent = new GUIContent("NodeFields Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new NodeFieldsGraphView();
            rootVisualElement.Add(_sampleGraphView);
        }
    }
}