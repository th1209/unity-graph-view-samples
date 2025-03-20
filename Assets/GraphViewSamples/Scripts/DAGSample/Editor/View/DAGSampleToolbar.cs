using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DAGSample.Editor.View
{
    public class DAGSampleToolbar : Toolbar, IDisposable
    {
        public Action AlignButtonClickedEvent;
        public Func<bool> IsAcyclicButtonClickedEvent;
        public Func<bool> IsConnectedButtonClickedEvent;

        private Texture2D _noneTexture;
        private Texture2D _successTexture;
        private Texture2D _failedTexture;
        
        private Image _isAcyclicResultIcon;
        private Image _isConnectedResultIcon;
        
        public DAGSampleToolbar() : base()
        {
            _noneTexture = EditorGUIUtility.Load("TestNormal") as Texture2D;
            _successTexture = EditorGUIUtility.Load("TestPassed") as Texture2D;
            _failedTexture = EditorGUIUtility.Load("TestFailed") as Texture2D;

            var alignButton = new ToolbarButton(() => AlignButtonClickedEvent?.Invoke()) { text = "Align Nodes" };
            Add(alignButton);
            
            var checkIsAcyclicButton = new ToolbarButton(() =>
            {
                if (IsConnectedButtonClickedEvent != null)
                {
                    var result = IsAcyclicButtonClickedEvent.Invoke();
                    _isAcyclicResultIcon.image = result ? _successTexture : _failedTexture;
                }
            }) { text = "Is Acyclic" };
            checkIsAcyclicButton.style.marginLeft = 8;
            Add(checkIsAcyclicButton);
            _isAcyclicResultIcon = new Image();
            _isAcyclicResultIcon.style.width = 16;
            _isAcyclicResultIcon.style.height = 16;
            _isAcyclicResultIcon.image = _noneTexture;
            Add(_isAcyclicResultIcon);
            
            var checkIsConnectedButton = new ToolbarButton(() =>
            {
                if (IsConnectedButtonClickedEvent != null)
                {
                    var result = IsConnectedButtonClickedEvent.Invoke();
                    _isConnectedResultIcon.image = result ? _successTexture : _failedTexture;
                }
            }) { text = "Is Connected" };
            checkIsConnectedButton.style.marginLeft = 8;
            Add(checkIsConnectedButton);
            _isConnectedResultIcon = new Image();
            _isConnectedResultIcon.style.width = 16;
            _isConnectedResultIcon.style.height = 16;
            _isConnectedResultIcon.image = _noneTexture;
            Add(_isConnectedResultIcon);
        }

        public void Dispose()
        {
            IsConnectedButtonClickedEvent = null;
            IsAcyclicButtonClickedEvent = null;
            AlignButtonClickedEvent = null;
            
            _failedTexture = null;
            _successTexture = null;
            _noneTexture = null;
        }
    }
}