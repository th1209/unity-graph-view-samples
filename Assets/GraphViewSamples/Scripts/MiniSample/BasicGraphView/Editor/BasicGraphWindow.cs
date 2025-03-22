using UnityEngine;
using UnityEditor;

namespace MiniSample.BasicGraphView.Editor
{
    public class BasicGraphWindow : EditorWindow
    {
        private BasicGraphView _sampleGraphView;

        [MenuItem("Window/Mini GraphView Samples/Basic", false, 0)]
        public static void OpenWindow()
        {
            var window = GetWindow<BasicGraphWindow>();
            window.titleContent = new GUIContent("Basic Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new BasicGraphView();
            rootVisualElement.Add(_sampleGraphView);
        }
    }
}