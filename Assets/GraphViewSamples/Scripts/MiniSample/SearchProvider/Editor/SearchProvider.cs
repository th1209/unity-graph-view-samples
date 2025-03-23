using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MiniSample.SearchProvider.Editor
{
    // ScriptableObjectの継承を忘れずに
    public class SearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private readonly List<SearchTreeEntry> _entries = new List<SearchTreeEntry>(16);
        
        public void Initialize()
        {
            _entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Custom Node")));
            _entries.Add(new SearchTreeGroupEntry(new GUIContent("Primitive Nodes"))
            {
                level = 1,
            });
            _entries.Add(new SearchTreeEntry(new GUIContent("Int Node"))
            {
                level = 2,
                userData = typeof(IntNode),
            });
            _entries.Add(new SearchTreeEntry(new GUIContent("Float Node"))
            {
                level = 2,
                userData = typeof(FloatNode),
            });
            _entries.Add(new SearchTreeEntry(new GUIContent("String Node"))
            {
                level = 2,
                userData = typeof(StringNode),
            });
            _entries.Add(new SearchTreeGroupEntry(new GUIContent("Unity Nodes"))
            {
                level = 1,
            });
            _entries.Add(new SearchTreeEntry(new GUIContent("Texture Node"))
            {
                level = 2,
                userData = typeof(TextureNode),
            });
            _entries.Add(new SearchTreeEntry(new GUIContent("Material Node"))
            {
                level = 2,
                userData = typeof(MaterialNode),
            });
        }

        // SearchWindow上で表示するメニューのエントリ一覧を返す。
        // SearchWindowが開かれる度に呼ばれる。
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            UnityEngine.Debug.Log($"CreateSearchTree time:{UnityEngine.Time.time}");
            return _entries;
        }

        // ここのメニューが選択された際に、このメソッドが呼ばれる
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData == null)
            {
                return true;
            }

            var userDataName = searchTreeEntry.userData.ToString();
            UnityEngine.Debug.Log($"OnSelectEntry userData:{userDataName} time:{UnityEngine.Time.time}");
            return true;
        }
    }
}