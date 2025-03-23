using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace MiniSample.NodeContainers.Editor
{
    public class NodeContainersGraphView : GraphView
    {
        public NodeContainersGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());

            var node = CreateNode();
            node.SetPosition(new Rect(100, 100, 150, 150));
            AddElement(node);
        }

        private Node CreateNode()
        {
            var node = new Node();

            node.titleContainer.Insert(0, new Label("titleContainer"));

            var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPort.portName = "inputContainer";
            node.inputContainer.Add(inputPort);

            var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            outputPort.portName = "outputContainer";
            node.outputContainer.Add(outputPort);

            node.extensionContainer.Add(new Label("extensionContainer"));

            node.mainContainer.Add(new Label("mainContainer"));

            node.contentContainer.Add(new Label("contentContainer"));

            node.RefreshExpandedState();

            return node;
        }
    }
}