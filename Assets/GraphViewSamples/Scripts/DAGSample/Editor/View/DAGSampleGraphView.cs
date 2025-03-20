using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace DAGSample.Editor.View
{
    public class DAGSampleGraphView : GraphView, IDisposable
    {
        private const string StyleSheetPath = "Assets/GraphViewSamples/StyleSheets/DAGSample/dag_sample_style_sheet.uss";
        
        public Func<Vector2, Node> CreateNodeEvent;
        public Action<List<Edge>> CreateEdgesEvent;
        public Action<List<GraphElement>> RemoveElementsEvent;
        public Action<List<GraphElement>> MoveElementsEvent;
        public Func<IEnumerable<GraphElement>, string> SerializeGraphElementsEvent;
        public Func<string, bool> CanPasteSerializedDataEvent;
        public Action<string> UnserializeAndPasteEvent;
        
        DAGSampleGraphSearchWindowProvider _searchWindowProvider;
        
        public DAGSampleGraphView(GraphViewEditorWindow window)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();
            
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());

            var backGround = new GridBackground();
            Insert(0, backGround);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            styleSheets.Add(styleSheet);
            
            _searchWindowProvider = ScriptableObject.CreateInstance<DAGSampleGraphSearchWindowProvider>();
            _searchWindowProvider.Initialize(window, this, OnNodeCrate);

            RegisterCallbacks();
        }

        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Experimental.GraphView.GraphView.GetCompatiblePorts.html
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // UnityEngine.Debug.Log($"{nameof(GetCompatiblePorts)} time:{UnityEngine.Time.time}");
            var compatiblePorts = new List<Port>();
            compatiblePorts.AddRange(ports.ToList().Where(port => startPort.node != port.node && startPort.direction != port.direction && port.portType == startPort.portType));
            return compatiblePorts;
        }

        private void RegisterCallbacks()
        {
            // 右クリックメニューからNodeが作成される際に呼ばれる
            nodeCreationRequest += OnNodeCreationRequest;

            // GraphView上で何らかの変化があった場合に呼ばれる
            graphViewChanged += OnGraphViewChanged;

            // NodeのCopy&Pasteについては以下のコールバック群を使う
            // 要素のCopy時に呼ばれ、Paste処理に必要な情報をstringで返す
            serializeGraphElements += OnSerializeGraphElements;
            // 要素のPaste時に最初に呼ばれる。Paste可能かどうかを返す
            canPasteSerializedData += OnCanPasteSerializedData;
            // 上記コールバックがtrueを返す場合に呼ばれる。実際のPaste処理を行う
            unserializeAndPaste += OnUnserializeAndPaste;
        }
        
        private void UnregisterCallbacks()
        {
            unserializeAndPaste -= OnUnserializeAndPaste;
            canPasteSerializedData -= OnCanPasteSerializedData;
            serializeGraphElements -= OnSerializeGraphElements;
            graphViewChanged -= OnGraphViewChanged;
            nodeCreationRequest -= OnNodeCreationRequest;
        }

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            // UnityEngine.Debug.Log($"{nameof(OnNodeCreationRequest)} time:{UnityEngine.Time.time}");
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindowProvider);
        }

        private Node OnNodeCrate(Vector2 position)
        {
            // UnityEngine.Debug.Log($"{nameof(OnNodeCrate)} time:{UnityEngine.Time.time}");
            if (CreateNodeEvent == null) return null;
            return CreateNodeEvent.Invoke(position);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                // UnityEngine.Debug.Log($"{nameof(OnGraphViewChanged)}(CreateEdge) time:{UnityEngine.Time.time}");
                CreateEdgesEvent?.Invoke(change.edgesToCreate);
            }
            if (change.elementsToRemove != null)
            {
                // UnityEngine.Debug.Log($"{nameof(OnGraphViewChanged)}(RemoveElements) time:{UnityEngine.Time.time}");
                RemoveElementsEvent?.Invoke(change.elementsToRemove);
            }
            if (change.movedElements != null)
            {
                // UnityEngine.Debug.Log($"{nameof(OnGraphViewChanged)}(MoveElements) time:{UnityEngine.Time.time}");
                MoveElementsEvent?.Invoke(change.movedElements);
            }

            return change;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            // UnityEngine.Debug.Log($"{nameof(OnSerializeGraphElements)} time:{UnityEngine.Time.time}");
            if (SerializeGraphElementsEvent == null) return string.Empty;
            return SerializeGraphElementsEvent.Invoke(elements);
        }
        
        private bool OnCanPasteSerializedData(string data)
        {
            // UnityEngine.Debug.Log($"{nameof(OnCanPasteSerializedData)} time:{UnityEngine.Time.time}");
            if (CanPasteSerializedDataEvent == null) return false;
            return CanPasteSerializedDataEvent.Invoke(data);
        }
        
        private void OnUnserializeAndPaste(string operationName, string data)
        {
            // UnityEngine.Debug.Log($"{nameof(OnUnserializeAndPaste)} time:{UnityEngine.Time.time}");
            UnserializeAndPasteEvent?.Invoke(data);
        }

        public void Dispose()
        {
            UnregisterCallbacks();
            
            CreateNodeEvent = null;
            CreateEdgesEvent = null;
            RemoveElementsEvent = null;
            MoveElementsEvent = null;
            SerializeGraphElementsEvent = null;
            CanPasteSerializedDataEvent = null;
            UnserializeAndPasteEvent = null;
        }
    }
}