using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

namespace MiniSample.NodeFields.Editor
{
    public class NodeFieldsGraphView : GraphView
    {
        private enum Fruit
        {
            Apple = 0,
            Banana,
            Grape,
            Orange
        }
        
        private const string StyleSheetPath = "Assets/GraphViewSamples/StyleSheets/MiniSample/node_fields_sample_style_sheet.uss";
        
        public NodeFieldsGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            var backGround = new GridBackground();
            Insert(index:0, backGround);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            Assert.IsNotNull(styleSheet);
            styleSheets.Add(styleSheet);

            var nodeSize = new Vector2(150, 150);
            
            CreateNode(nameof(IntegerField), "int", "int-field-node", new Rect(new Vector2(000, 0), nodeSize), () => {
                var integerField = new IntegerField();
                integerField.value = 123;
                return integerField;
            });
            
            CreateNode(nameof(FloatField), "float", "float-field-node", new Rect(new Vector2(200, 0), nodeSize), () => {
                var floatField = new FloatField();
                floatField.value = 3.14f;
                return floatField;
            });
            
            CreateNode(nameof(EnumField), "enum", "enum-field-node", new Rect(new Vector2(400, 0), nodeSize), () => {
                var enumField = new EnumField();
                enumField.Init(Fruit.Apple);
                return enumField;
            });
            
            CreateNode(nameof(Vector3Field), "vector3", "vector3-field-node", new Rect(new Vector2(600, 0), nodeSize), () => {
                var vector3Field = new Vector3Field();
                vector3Field.value = new Vector3(1, 2, 3);
                return vector3Field;
            });

            
            CreateNode("TextField", "string", "text-field-node", new Rect(new Vector2(0, 150), nodeSize), () => {
                var singleLineTextField = new TextField();
                singleLineTextField.value = "昔話";
                return singleLineTextField;
            });
            
            CreateNode("TextField(MultiLine)", "string", "text-field-node", new Rect(new Vector2(200, 150), nodeSize), () => {
                var multiLineTextField = new TextField() { multiline = true };
                multiLineTextField.value = "むかしむかし、\nあるところに...";
                return multiLineTextField;
            });
            

            CreateNode(nameof(ColorField), "color", "color-field-node", new Rect(new Vector2(0, 300), nodeSize), () => {
                var colorField = new ColorField();
                colorField.value = Color.white;
                return colorField;
            });
            
            CreateNode(nameof(GradientField),  "gradient",  "gradient-field-node", new Rect(new Vector2(200, 300), nodeSize), () => {
                return new GradientField();
            });
            
            CreateNode(nameof(CurveField),  "curve", "curve-field-node", new Rect(new Vector2(400, 300), nodeSize),
                () =>
                {
                    var curveField = new CurveField();
                    curveField.value = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
                    return curveField;
                });

            CreateNode("ObjectField(GameObject)", "gameObject", "object-field-node",
                new Rect(new Vector2(0, 450), nodeSize), () =>
                {
                    var objectField = new ObjectField();
                    objectField.objectType = typeof(GameObject);
                    objectField.RegisterValueChangedCallback(changeEvent =>
                    {
                        var prevObjectReference = changeEvent.previousValue;
                        var newObjectReference = changeEvent.newValue;
                    });
                    return objectField;
                });
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