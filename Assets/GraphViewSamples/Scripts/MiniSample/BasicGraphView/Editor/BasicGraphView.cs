using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
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

            // 背景を設定
            var backGround = new GridBackground();
            Insert(index:0, backGround);
            
            // StyleSheetを適用する
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            Assert.IsNotNull(styleSheet);
            styleSheets.Add(styleSheet);
        }
    }
}