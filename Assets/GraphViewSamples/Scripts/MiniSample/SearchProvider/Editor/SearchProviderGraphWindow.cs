using UnityEngine;
using UnityEditor;

namespace MiniSample.SearchProvider.Editor
{
    public class SearchProviderGraphWindow : EditorWindow
    {
        private SearchProviderGraphView _sampleGraphView;

        [MenuItem("Window/Mini GraphView Samples/Search Provider", false, 3)]
        public static void OpenWindow()
        {
            var window = GetWindow<SearchProviderGraphWindow>();
            window.titleContent = new GUIContent("Search Provider Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new SearchProviderGraphView();
            rootVisualElement.Add(_sampleGraphView);
        }
    }
}