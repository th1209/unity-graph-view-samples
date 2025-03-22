using UnityEditor.Experimental.GraphView;

namespace MiniSample.BasicGraphView.Editor
{
    public class CustomNode : Node
    {
        private Port _inputPort;
        private Port _outputPort;

        public CustomNode(string title)
        {
            base.title = title;

            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            _inputPort.portName = "input";
            inputContainer.Add(_inputPort);

            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            _outputPort.portName = "output";
            outputContainer.Add(_outputPort);
        }
    }
}