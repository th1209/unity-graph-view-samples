using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MiniSample.GroupAndMinimap.Editor
{
    public class GroupAndMinimapGraphWindow : EditorWindow
    {
        private GroupAndMinimapGraphView _sampleGraphView;
        private Toolbar _toolbar;

        [MenuItem("Window/Mini GraphView Samples/Group and Minimap", false, 4)]
        public static void OpenWindow()
        {
            var window = GetWindow<GroupAndMinimapGraphWindow>();
            window.titleContent = new GUIContent("Group and Minimap Sample");
            window.Show();
        }

        private void CreateGUI()
        {
            _sampleGraphView = new GroupAndMinimapGraphView();
            rootVisualElement.Add(_sampleGraphView);
            
            _toolbar = new Toolbar();
            var toggleMinimapButton = new ToolbarButton(() =>
            {
                _sampleGraphView.MiniMap.visible = !_sampleGraphView.MiniMap.visible;
                _sampleGraphView.MiniMap.style.display = _sampleGraphView.MiniMap.visible ? DisplayStyle.Flex : DisplayStyle.None;
            })
            {
                text = "Toggle Minimap"
            };
            _toolbar.Add(toggleMinimapButton);
            rootVisualElement.Add(_toolbar);
        }
    }
}