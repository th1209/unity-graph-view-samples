using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace MiniSample.BlackBoard.Editor
{
    public class BlackBoardGraphView : GraphView
    {
        private Blackboard blackboard;
        
        private string _typeNameInt = nameof(Int32);
        private string _typeNameFloat = nameof(Single);
        private string _typeNameBool = nameof(Boolean);
        private string _typeNameString = nameof(String);

        // BlackboardFieldのDrag&Drop時で受け渡すGenericDataのKey
        private const string DragType = "DragSelection";
        
        public BlackBoardGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());
            
            blackboard = new Blackboard(this)
            {
                title = "Node Create",
                scrollable = true
            };
            blackboard.style.height = 150;
            // + ボタンが押された時のメニューを登録する
            blackboard.addItemRequested = (_) => ShowAddMenu();
            blackboard.SetPosition(new Rect(20, 20, 200, 150));
            Add(blackboard);

            // GraphViewに、Drag&Dropされた時のコールバックを登録する
            // (DragUpdateEventとDragPerformEventしか使いませんが、他のイベントもコメントインして試してみてください)
            // RegisterCallback<DragEnterEvent>(OnDragEnter);
            // RegisterCallback<DragLeaveEvent>(OnDragLeave);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            // RegisterCallback<DragExitedEvent>(OnDragExited);

        }
        
        private void ShowAddMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Int"), false, () => AddBlackboardVariable("Int Node", nameof(Int32)));
            menu.AddItem(new GUIContent("Add Float"), false, () => AddBlackboardVariable("Float Node", nameof(Single)));
            menu.AddItem(new GUIContent("Add String"), false, () => AddBlackboardVariable("String Node", nameof(String)));
            menu.ShowAsContext();
        }
        
        private void AddBlackboardVariable(string name, string typeName)
        {
            var field = new BlackboardField { text = name, typeText = typeName };
            blackboard.Add(field);
        }

        
        private void OnDragEnter(DragEnterEvent evt)
        {
            // UnityEngine.Debug.Log($"OnDragEnter phase:{evt.propagationPhase} time:{UnityEngine.Time.time}");
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            // UnityEngine.Debug.Log($"OnDragLeave phase:{evt.propagationPhase} time:{UnityEngine.Time.time}");
        }
        
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            // UnityEngine.Debug.Log($"OnDragUpdated phase:{evt.propagationPhase} time:{UnityEngine.Time.time}");
            
            // 重要:DragPerformEventが発火するには、DragUpdateEventでDragAndDrop.visualModeを設定してやる必要があります
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy; 
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            // UnityEngine.Debug.Log($"OnDragPerform phase:{evt.propagationPhase} time:{UnityEngine.Time.time}");

            // Drag&Drop時に、BlackBoardFieldの種類に応じたNodeを作成する。
            // (※BlackBoardはGraph間で共有する変数群などを扱うもので、本来のBlackBoardの用途からすればナンセンスな例であることにご留意ください...)
            var selection = DragAndDrop.GetGenericData(DragType) as List<ISelectable>;
            if (selection == null)
            {
                return;
            }
            foreach (var selectable in selection)
            {
                if (selectable is BlackboardField field)
                {
                    CreateNodeFromBlackboardField(field, evt.localMousePosition);
                }
            }
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            // UnityEngine.Debug.Log($"OnDragExited phase:{evt.propagationPhase} time:{UnityEngine.Time.time}");
        }

        private Node CreateNodeFromBlackboardField(BlackboardField blackBoardField, Vector2 position)
        {
            switch (blackBoardField.typeText)
            {
                case nameof(Int32):
                    return CreateNode(blackBoardField.text, blackBoardField.text, "int-node", new Rect(position, new Vector2(100, 100)), () =>
                    {
                        var integerField = new IntegerField();
                        return integerField;
                    });
                    break;
                case nameof(Single):
                    return CreateNode(blackBoardField.text, blackBoardField.text, "float-node", new Rect(position, new Vector2(100, 100)), () =>
                    {
                        var floatField = new FloatField();
                        return floatField;
                    });
                    break;
                case nameof(String):
                    return CreateNode(blackBoardField.text, blackBoardField.text, "string-node", new Rect(position, new Vector2(100, 100)), () =>
                    {
                        var textField = new TextField();
                        return textField;
                    });
                    break;
                default:
                    UnityEngine.Debug.LogError($"Unknown field. type:{blackBoardField.typeText}");
                    return null;
                    break;
            }
        }

        private Node CreateNode(string title, string fieldLabel, string styleClass, Rect rect, Func<VisualElement> fieldsProvider)
        {
            var node = new Node();

            node.title = title;

            var label = new Label(fieldLabel);
            label.AddToClassList("field-label");
            node.mainContainer.Add(label);
            
            node.mainContainer.Add(fieldsProvider.Invoke());
            
            node.AddToClassList(styleClass);

            node.RefreshExpandedState();
            
            node.SetPosition(rect);
            AddElement(node);

            return node;
        }
    }
}