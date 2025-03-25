using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

namespace MiniSample.GroupAndMinimap.Editor
{
    public class GroupAndMinimapGraphView : GraphView
    {
        private int _nodeCreateCount = 0;
        private int _groupCreateCount = 0;
        
        public MiniMap MiniMap;
        
        public GroupAndMinimapGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            
            MiniMap = new MiniMap
            {
                // anchoredがtrueならドラッグによる移動不可
                anchored = false,
            };
            MiniMap.SetPosition(new Rect(20, 40, 150, 150));
            MiniMap.visible = false;
            MiniMap.style.display =  DisplayStyle.None;
            Add(MiniMap);
        }
        
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var pos = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            evt.menu.AppendAction("Create Node", action => AddNode($"Node{_nodeCreateCount++}", pos));
            evt.menu.AppendAction("Create Group", action => AddGroup($"Group{_groupCreateCount++}", pos));
                
            var selectedNodes = selection.OfType<Node>().ToList();
            if (selectedNodes.Count > 0)
            {
                evt.menu.AppendAction("Remove selected Nodes from Group", action =>
                {
                    foreach (var node in selectedNodes)
                    {
                        if (node.GetContainingScope() is Scope scope)
                        {
                            scope.RemoveElement(node);
                        }
                    }
                }, DropdownMenuAction.AlwaysEnabled);
            }
        }

        private Node AddNode(string title, Vector2 mousePosition)
        {
            var node = new Node { title = title };
            node.SetPosition(new Rect(mousePosition, new Vector2(100, 100)));
            AddElement(node);
            return node;
        }

        private Group AddGroup(string title, Vector2 mousePosition)
        {
            var group = new Group
            {
                title = title,
                autoUpdateGeometry = true // 自動でサイズ調整
            };
            group.SetPosition(new Rect(mousePosition, new Vector2(100, 100)));
            AddElement(group);
            return group;
        }
    }
}