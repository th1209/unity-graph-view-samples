using System;
using UnityEditor.UIElements;
using UnityEngine;

namespace DAGSample.Editor
{
    public class DAGSampleToolbar : Toolbar, IDisposable
    {
        public Action AlignButtonClickedEvent;

        public DAGSampleToolbar() : base()
        {
            var alignButton = new ToolbarButton(() => AlignButtonClickedEvent?.Invoke()) { text = "Align Nodes" };
            Add(alignButton);
        }

        public void Dispose()
        {
            AlignButtonClickedEvent = null;
        }
    }
}