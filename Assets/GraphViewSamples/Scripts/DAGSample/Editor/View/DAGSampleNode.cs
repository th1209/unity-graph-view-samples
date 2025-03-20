using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DAGSample.Editor.View
{
    public class DAGSampleNode : Node
    {
        public Port InputPort => _inputPort;
        
        public Port OutputPort => _outputPort;
        
        public string Guid => _guid;

        private Port _inputPort;
        private Port _outputPort;

        private TextField _titleField;
        
        private string _guid;

        private Action<DAGSampleNode> _updateEvent;

        public DAGSampleNode(string title,  string guid, Action<DAGSampleNode> updateEvent)
        {
            base.title = title;
            _guid = guid;
            _updateEvent = updateEvent;
        }

        // ※コンストラクタ内でVirtualメンバを呼ぶのがよろしくないので、明示的な初期化メソッドを追加
        public void Initialize()
        {
            _titleField = new TextField { value = title, label = string.Empty };
            _titleField.RegisterValueChangedCallback(evt =>
            {
                title = evt.newValue;
                _updateEvent?.Invoke(this);
            });
            titleContainer.Insert(0, _titleField);

            // ※
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            _inputPort.portName = "from";
            inputContainer.Add(_inputPort);

            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            _outputPort.portName = "to";
            outputContainer.Add(_outputPort);

            var primitiveFieldFoldout = new Foldout { text = "Additional Fields", value = true };
            primitiveFieldFoldout.value = false; // 初期状態は閉じた状態に
            extensionContainer.Add(primitiveFieldFoldout);
            var guidField = new TextField("GUID");
            guidField.value = _guid;
            guidField.SetEnabled(false); // 入力不可
            primitiveFieldFoldout.Add(guidField);

            RefreshExpandedState();
        }
    }
}