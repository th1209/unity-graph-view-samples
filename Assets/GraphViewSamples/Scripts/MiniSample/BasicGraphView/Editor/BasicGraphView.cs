using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace MiniSample.BasicGraphView.Editor
{
    public class BasicGraphView : GraphView
    {
        private const string StyleSheetPath = "Assets/GraphViewSamples/StyleSheets/MiniSample/background_style_sheet.uss";
        
        public BasicGraphView()
        {
            // ズーム倍率を設定する(デフォルト値の場合、0.25~1.0倍)
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            // 各種マニピュレーターを設定
            // 要素を選択、移動可能にする
            this.AddManipulator(new SelectionDragger());
            // 要素を矩形選択可能にする
            this.AddManipulator(new RectangleSelector());
            // 描画範囲自体を移動可能にする
            this.AddManipulator(new ContentDragger());
            // コンテキストメニュー呼び出し時の挙動をカスタマイズする
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            // 背景を設定
            var backGround = new GridBackground();
            Insert(index:0, backGround);
            
            // StyleSheetを適用する
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            Assert.IsNotNull(styleSheet);
            styleSheets.Add(styleSheet);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // ノード作成メニューを作り、押された時にNodeを生成する
            var pos = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            evt.menu.AppendAction("Create Node", action => CreateNode("Node", pos));
            
            // デフォルトのメニューを表示したい場合は以下をコメントインしてください
            base.BuildContextualMenu(evt);
        }

        // Port間が接続可能かどうかは、GraphViewのGetCompatiblePortsをオーバーライドして判定する
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // 接続開始Portが引数で渡ってくる。
            // GraphView上のPort群のうち、接続可能なPort群を返すように実装すればOK。
            var compatiblePorts = new List<Port>(ports.Count());
            foreach (var port in ports)
            {
                if (startPort.node == port.node || startPort.direction == port.direction || startPort.portType != port.portType)
                {
                    continue;
                }
                compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }

        private Node CreateNode(string title, Vector2 position)
        {
            var node = new CustomNode(title);
            node.SetPosition(new Rect(position, new Vector2(150, 100)));
            AddElement(node);
            return node;
        }
    }
}