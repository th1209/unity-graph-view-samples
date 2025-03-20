using DAGSample.Editor.Presenter;
using DAGSample.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace DAGSample.Editor.View
{
    public class DAGSampleGraphWindow : GraphViewEditorWindow
    {
        private DAGSampleGraphData _graphData;
        private DAGSampleGraphView _graphView;
        private DAGSampleToolbar _toolbar;
        private DAGSamplePresenter _presenter;
        
        public static void Open(DAGSampleGraphData graphData)
        {
                var window = GetWindow<DAGSampleGraphWindow>("DAGSampleGraphWindow", desiredDockNextTo:typeof(SceneView));
                if (window._graphData == null)
                {
                    window.Initialize(graphData);
                    window.ReloadGraphNodes();
                    return;
                }
                
                if (window._graphData == graphData)
                {
                    // 既にWindowが開いており、もう一度開こうとする場合
                    window.Focus();
                    return;
                }
                
                // 既にWindowが開いており、ScriptableObjectで開き直す場合
                window.DestroyGraph();
                window.Initialize(graphData);
                window.Focus();
        }
        
        private void Initialize(DAGSampleGraphData graphData)
        {
            _graphData = graphData;
            _graphView = new DAGSampleGraphView(this);
            rootVisualElement.Add(_graphView);
            _toolbar = new DAGSampleToolbar();
            rootVisualElement.Add(_toolbar);
            _presenter = new DAGSamplePresenter(_graphData, _graphView, _toolbar);
        }

        private void ReloadGraphNodes()
        {
            _presenter?.DeserializeGraphData(_graphData);
        }

        private void OnDisable()
        {
            DestroyGraph();
        }

        private void DestroyGraph()
        {
            _presenter?.Dispose();
            _presenter = null;

            if (_toolbar != null)
            {
                rootVisualElement.Remove(_toolbar);
                _toolbar.Dispose();
                _toolbar = null;
            }

            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
                _graphView.Dispose();
                _graphView = null;
            }

            _graphData = null;
        }
    }
}


