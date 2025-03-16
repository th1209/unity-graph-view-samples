using System;
using System.Collections.Generic;
using System.Linq;
using DAGSample.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace DAGSample.Tests
{
    public class DAGSampleGraphDataTest
    {
        [Test]
        public void IsAcyclicSuccessCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            // 空の場合は満たすはず
            Assert.IsTrue(graph.IsAcyclic());
            
            var node1 = new DAGSampleNodeData("Node1", new Rect());
            var node2 = new DAGSampleNodeData("Node2", new Rect());
            var node3 = new DAGSampleNodeData("Node3", new Rect());
            var node4 = new DAGSampleNodeData("Node4", new Rect());
            
            graph.Add(node2);
            graph.Add(node1);
            graph.Add(node3);
            graph.Add(node4);
            
            graph.Connect(node1.Guid, node2.Guid);
            graph.Connect(node1.Guid, node3.Guid);
            graph.Connect(node2.Guid, node3.Guid);
            graph.Connect(node3.Guid, node4.Guid);

            Assert.IsTrue(graph.IsAcyclic());
        }
        
        [Test]
        public void IsAcyclicFailureCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            var node1 = new DAGSampleNodeData("Node1", new Rect());
            var node2 = new DAGSampleNodeData("Node2", new Rect());
            var node3 = new DAGSampleNodeData("Node3", new Rect());
            var node4 = new DAGSampleNodeData("Node4", new Rect());
            
            graph.Add(node2);
            graph.Add(node1);
            graph.Add(node3);
            graph.Add(node4);
            
            graph.Connect(node1.Guid, node2.Guid);
            graph.Connect(node1.Guid, node3.Guid);
            graph.Connect(node2.Guid, node3.Guid);
            graph.Connect(node3.Guid, node4.Guid);
            // 循環するEdge
            graph.Connect(node4.Guid, node1.Guid);

            Assert.IsFalse(graph.IsAcyclic());
        }

        [Test]
        public void IsConnectedSuccessCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            // 空の場合は満たすはず
            Assert.IsTrue(graph.IsConnected());
            
            var node1 = new DAGSampleNodeData("Node1", new Rect());
            var node2 = new DAGSampleNodeData("Node2", new Rect());
            var node3 = new DAGSampleNodeData("Node3", new Rect());
            var node4 = new DAGSampleNodeData("Node4", new Rect());
            
            graph.Add(node2);
            graph.Add(node1);
            graph.Add(node3);
            graph.Add(node4);

            graph.Connect(node1.Guid, node3.Guid);
            graph.Connect(node2.Guid, node3.Guid);
            graph.Connect(node3.Guid, node4.Guid);

            Assert.IsTrue(graph.IsConnected());
        }
        
        [Test]
        public void IsConnectedFailureCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            var node1 = new DAGSampleNodeData("Node1", new Rect());
            var node2 = new DAGSampleNodeData("Node2", new Rect());
            var node3 = new DAGSampleNodeData("Node3", new Rect());
            var node4 = new DAGSampleNodeData("Node4", new Rect());
            
            graph.Add(node2);
            graph.Add(node1);
            graph.Add(node3);
            graph.Add(node4);
            
            graph.Connect(node1.Guid, node3.Guid);
            graph.Connect(node2.Guid, node3.Guid);
            graph.Connect(node3.Guid, node4.Guid);
            // Node2を切り離す
            graph.Disconnect(node2.Guid, node3.Guid);

            Assert.IsFalse(graph.IsConnected());
        }
        
        [Test]
        public void GetSortedNodesSuccessCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            var emptyNodes = graph.GetSortedNodes();
            Assert.IsTrue(emptyNodes.Count == 0);

            var nodeCount = 7;
            var nodes = new List<DAGSampleNodeData>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                var node = new DAGSampleNodeData($"Node{i+1}", new Rect());
                graph.Add(node);
                nodes.Add(node);
            }

            graph.Connect(nodes[2].Guid, nodes[1].Guid);
            graph.Connect(nodes[2].Guid, nodes[0].Guid);
            graph.Connect(nodes[3].Guid, nodes[0].Guid);
            graph.Connect(nodes[3].Guid, nodes[5].Guid);
            graph.Connect(nodes[1].Guid, nodes[4].Guid);
            graph.Connect(nodes[4].Guid, nodes[0].Guid);
            graph.Connect(nodes[0].Guid, nodes[5].Guid);
            graph.Connect(nodes[5].Guid, nodes[6].Guid);
            
            var sortedNodes = graph.GetSortedNodes();
            var actualOrder = string.Join(",", sortedNodes.Select(n => n.Name));
            var expectedOrder = "Node3,Node4,Node2,Node5,Node1,Node6,Node7";
            Assert.AreEqual(expectedOrder, actualOrder);
        }
        
        [Test]
        public void GetSortedNodesFailureCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            var node1 = new DAGSampleNodeData("Node1", new Rect());
            var node2 = new DAGSampleNodeData("Node2", new Rect());
            var node3 = new DAGSampleNodeData("Node3", new Rect());
            var node4 = new DAGSampleNodeData("Node4", new Rect());
            
            graph.Add(node2);
            graph.Add(node1);
            graph.Add(node3);
            graph.Add(node4);
            
            graph.Connect(node1.Guid, node2.Guid);
            graph.Connect(node2.Guid, node3.Guid);
            graph.Connect(node3.Guid, node4.Guid);
            graph.Connect(node4.Guid, node1.Guid);

            bool isExceptionThrown = false;
            try
            {
                graph.GetSortedNodes();
            }
            catch (InvalidOperationException e)
            {
                isExceptionThrown = true;
            }
            Assert.IsTrue(isExceptionThrown);
        }
        
        // 連結条件を満たしていない場合もトポロジカルソートは可能
        [Test]
        public void GetSortedNodesNotConnectedCase()
        {
            var graph = new DAGSampleGraphData(new List<DAGSampleNodeData>(), new List<DAGSampleEdgeData>());

            var emptyNodes = graph.GetSortedNodes();
            Assert.IsTrue(emptyNodes.Count == 0);

            var nodeCount = 5;
            var nodes = new List<DAGSampleNodeData>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                var node = new DAGSampleNodeData($"Node{i+1}", new Rect());
                graph.Add(node);
                nodes.Add(node);
            }

            graph.Connect(nodes[0].Guid, nodes[1].Guid);
            graph.Connect(nodes[1].Guid, nodes[2].Guid);
            graph.Connect(nodes[3].Guid, nodes[4].Guid);
            
            var sortedNodes = graph.GetSortedNodes();
            var actualOrder = string.Join(",", sortedNodes.Select(n => n.Name));
            var expectedOrder = "Node1,Node4,Node2,Node5,Node3";
            Assert.AreEqual(expectedOrder, actualOrder);
        }

    }
}


