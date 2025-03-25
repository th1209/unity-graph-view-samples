using UnityEngine;
using UnityEditor;

namespace MiniSample.BlackBoard.Editor
{
    public class BlackBoardGraphWindow : EditorWindow
    {
        private BlackBoardGraphView _sampleGraphView;

        [MenuItem("Window/Mini GraphView Samples/BlackBoard", false, 6)]
        public static void OpenWindow()
        {
            var window = GetWindow<BlackBoardGraphWindow>();
            window.titleContent = new GUIContent("BlackBoard Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new BlackBoardGraphView();
            rootVisualElement.Add(_sampleGraphView);
        }
    }
}