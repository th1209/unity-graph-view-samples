using System;
using DAGSample.Runtime;
using UnityEditor;
using UnityEngine;

namespace DAGSample.Editor.CustomInspector
{
    [CustomPropertyDrawer(typeof(DAGSampleNodeData))]
    public class DAGSampleNodeDataDrawer : PropertyDrawer
    {
        private const float Space = 4.0f;
        private const int FieldCount = 2;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty guidProp = property.FindPropertyRelative("guid");
            // ※rectはあくまでGraphView上での表示のものなので、インスペクタ上では表示しない

            float y = position.y;
            float width = position.width;
            float height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(new Rect(position.x, y, width, height), nameProp);
            y += height + Space;

            // GUIDは省略表示&編集不可
            var guid = guidProp.stringValue;
            guid = guid.Substring(0, Mathf.Min(7, guid.Length));
            GUI.enabled = false;
            EditorGUI.TextField(new Rect(position.x, y, width, height), "Guid",guid);
            GUI.enabled = true;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight * FieldCount + Space * (FieldCount - 1);
            return height;
        }
    }
}