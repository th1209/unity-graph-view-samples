using DAGSample.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

namespace DAGSample.Editor.CustomInspector
{
    [CustomEditor(typeof(DAGSampleGraphData))]
    public class DAGSampleGraphDataEditor : UnityEditor.Editor
    {
        private const float ElementSpace = 4.0f;
        
        private DAGSampleGraphData _graphData;
        private SerializedProperty _nodesProperty;
        private SerializedProperty _edgesProperty;
        private ReorderableList _nodeList;
        private ReorderableList _edgeList;

        private void OnEnable()
        {
            _graphData = (DAGSampleGraphData) target;
            Assert.IsNotNull(_graphData);
            
            _nodesProperty = serializedObject.FindProperty("nodes");
            _edgesProperty = serializedObject.FindProperty("edges");
            
            InitializeNodesList();
            InitializeEdgesList();
        }

        private void InitializeNodesList()
        {
            _nodeList = new ReorderableList(serializedObject, _nodesProperty, true, true, false, true);
            _nodeList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Nodes");
            _nodeList.onRemoveCallback += list =>
            {
                var index = list.index;
                _nodesProperty.DeleteArrayElementAtIndex(index);
            };
            _nodeList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            _nodeList.elementHeightCallback += index =>
            {
                var element = _nodeList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + ElementSpace;
            };
        }
        
        private void InitializeEdgesList()
        {
            _edgeList = new ReorderableList(serializedObject, _edgesProperty, true, true, false, true);
            _edgeList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Edges");
            _edgeList.onRemoveCallback += list =>
            {
                var index = list.index;
                _edgesProperty.DeleteArrayElementAtIndex(index);
            };
            _edgeList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                var element = _edgeList.serializedProperty.GetArrayElementAtIndex(index);
                var fromGuid = element.FindPropertyRelative("from").stringValue;
                var toGuid = element.FindPropertyRelative("to").stringValue;
                string fromNodeName = string.Empty;
                string toNodeName = string.Empty;
                if (_graphData.TryGetNode(fromGuid, out var fromNode))
                {
                    fromNodeName = fromNode.Name;
                }
                if (_graphData.TryGetNode(toGuid, out var toNode))
                {
                    toNodeName = toNode.Name;
                }
                fromGuid = fromGuid.Substring(0, Mathf.Min(7, fromGuid.Length));
                toGuid = toGuid.Substring(0, Mathf.Min(7, toGuid.Length));
                EditorGUI.LabelField(rect, $"{fromNodeName}({fromGuid}) -> {toNodeName}({toGuid})");
            };
            _edgeList.elementHeightCallback += index =>
            {
                return EditorGUIUtility.singleLineHeight + ElementSpace;
            };
        }

        private void OnDisable()
        {
            _edgeList = null;
            _nodeList = null;
            _edgesProperty = null;
            _nodesProperty = null;
            _graphData = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var graphData = (DAGSampleGraphData) target;
            Assert.IsNotNull(graphData);
            
            if (GUILayout.Button("Open Edit Window"))
            {
                DAGSampleGraphWindow.Open(graphData);
            }

            GUILayout.Space(8);
            
            _nodeList.DoLayoutList();
            
            GUILayout.Space(8);
            
            _edgeList.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}