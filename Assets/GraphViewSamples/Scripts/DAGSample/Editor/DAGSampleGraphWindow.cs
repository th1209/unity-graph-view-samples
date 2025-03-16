using DAGSample.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DAGSample.Editor
{
    public class DAGSampleGraphWindow : GraphViewEditorWindow
    {
        private DAGSampleGraphData _graphData;
        private DAGSampleGraphView _graphView;
        private DAGSamplePresenter _presenter;
        
        public static void Open(DAGSampleGraphData graphData)
        {
                var window = GetWindow<DAGSampleGraphWindow>("DAGSampleGraphWindow", desiredDockNextTo:typeof(SceneView));
                if (window._graphData == null)
                {
                    window.Initialize(graphData);
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
            // _presenter = new DAGSamplePresenter(_graphData);
            _graphView = new DAGSampleGraphView(this);
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable()
        {
            DestroyGraph();
        }

        private void DestroyGraph()
        {
            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
                _graphView.Dispose();
                _graphView = null;
            }

            _presenter?.Dispose();
            _presenter = null;

            _graphData = null;
        }
    }
}


