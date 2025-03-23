using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MiniSample.NodeContainers.Editor
{
    public class ContainerSampleNode : Node
    {
        public ContainerSampleNode()
        {
            titleContainer.Insert(0, new Label("titleContainer"));

            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPort.portName = "inputContainer";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            outputPort.portName = "outputContainer";
            outputContainer.Add(outputPort);
            
            extensionContainer.Add(new Label("extensionContainer"));

            mainContainer.Add(new Label("mainContainer"));

            contentContainer.Add(new Label("contentContainer"));

            RefreshExpandedState();
                
        }
    }
}