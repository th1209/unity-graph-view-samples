using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

namespace MiniSample.SearchProvider.Editor
{
    public class SearchProviderGraphView : GraphView
    {
        private const string StyleSheetPath = "Assets/GraphViewSamples/StyleSheets/MiniSample/background_style_sheet.uss";
        
        private SearchProvider _searchProvider;

        public SearchProviderGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.StretchToParentSize();

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentDragger());
            
            var backGround = new GridBackground();
            Insert(index:0, backGround);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            Assert.IsNotNull(styleSheet);
            styleSheets.Add(styleSheet);
            
            _searchProvider =  ScriptableObject.CreateInstance<SearchProvider>();
            _searchProvider.Initialize();
            
            nodeCreationRequest += OnNodeCreationRequest;
        }
        
        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchProvider);
        }
    }
}