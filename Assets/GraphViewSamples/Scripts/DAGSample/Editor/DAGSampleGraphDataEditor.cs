using DAGSample.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DAGSample.Editor
{
    [CustomEditor(typeof(DAGSampleGraphData))]
    public class DAGSampleGraphDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var graphData = (DAGSampleGraphData) target;
            Assert.IsNotNull(graphData);
            
            if (GUILayout.Button("Open Edit Window"))
            {
                DAGSampleGraphWindow.Open(graphData);
            }

            GUILayout.Space(8);

            base.OnInspectorGUI();
        }
    }
}