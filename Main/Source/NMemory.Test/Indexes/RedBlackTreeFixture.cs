using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.DataStructures.Internal.Trees;

namespace NMemory.Test.Indexes
{
    [TestClass]
    public class RedBlackTreeFixture
    {
        [TestMethod]
        public void CreateEmpty()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
        }

        [TestMethod]
        public void AddItem()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            int temp;

            tree.Add(1, 1);

            Assert.AreEqual(tree.Count, 1);
            Assert.IsTrue(tree.TryGetValue(1, out temp));
            Assert.IsFalse(tree.TryGetValue(2, out temp));
        }

        [TestMethod]
        public void RemoveItem()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(1, 1);
            int temp;

            
            bool removed = tree.Remove(1);

            Assert.IsTrue(removed);
            Assert.AreEqual(tree.Count, 0);
            Assert.IsFalse(tree.TryGetValue(1, out temp));
            Assert.IsFalse(tree.TryGetValue(2, out temp));
        }

        [TestMethod]
        public void RemoveNonExistingItem()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(1, 1);
            int temp;

            
            bool removed = tree.Remove(2);

            Assert.IsFalse (removed);
            Assert.AreEqual(tree.Count, 1);
            Assert.IsTrue(tree.TryGetValue(1, out temp));
            Assert.IsFalse(tree.TryGetValue(2, out temp));
        }


        [TestMethod]
        public void AddCaseRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            

            tree.Add(1, 1);

            //   B1
            //

            var e1 = tree.RootElement;

            AssertRoot(e1);
            Assert.IsTrue(!e1.Red);
        }

        [TestMethod]
        public void AddCaseBlackParent()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(1, 1);


            tree.Add(2, 2);

            //    B1
            //       R2  
            //

            var e1 = tree.RootElement;
            var e2 = e1.Right;

            AssertOrder(e1, e2);

            AssertRoot(e1);
            AssertParent(e2, e1);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(e2.Red);
        }


        [TestMethod]
        public void AddCaseRightRightRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(1, 1);
            tree.Add(2, 2);


            tree.Add(3, 3);

            //    B1
            //       R2  
            //          R3
            //
            // right-right case, root rotate 
            //
            //    B2
            // R1    R3
            //

            var e2 = tree.RootElement;
            var e3 = e2.Right;
            var e1 = e2.Left;

            AssertOrder(e1, e2, e3);

            AssertRoot(e2);
            AssertParent(e1, e2);
            AssertParent(e3, e2);

            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e1.Red);
            Assert.IsTrue(e3.Red);
        }

        [TestMethod]
        public void AddCaseRightLeftRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(1, 1);
            tree.Add(3, 3);


            tree.Add(2, 2);

            //    B1
            //        R3  
            //      R2
            //
            // right-right case, root rotate 
            //
            //    B2
            // R1    R3
            //

            var e2 = tree.RootElement;
            var e3 = e2.Right;
            var e1 = e2.Left;

            AssertOrder(e1, e2, e3);

            AssertRoot(e2);
            AssertParent(e1, e2);
            AssertParent(e3, e2);

            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e1.Red);
            Assert.IsTrue(e3.Red);
        }

        [TestMethod]
        public void AddCaseParentAndUncleRedRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(2, 2);
            tree.Add(3, 3);
            tree.Add(1, 1);


            tree.Add(4, 4);

            //    B2
            // R1    R3  
            //          R4
            //            
            // parent and uncle red
            //
            //    B2
            // B1    B3
            //          R4

            var e2 = tree.RootElement;
            var e3 = e2.Right;
            var e1 = e2.Left;
            var e4 = e3.Right;

            AssertOrder(e1, e2, e3, e4);

            AssertRoot(e2);
            AssertParent(e4, e3);
            AssertParent(e3, e2);
            AssertParent(e1, e2);

            AssertLeaves(e1, e4);
            Assert.IsNull(e3.Left);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(!e3.Red);
            Assert.IsTrue(e4.Red);
        }

        [TestMethod]
        public void AddCaseRightRight()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(2, 2);
            tree.Add(3, 3);
            tree.Add(1, 1);
            tree.Add(4, 4);


            tree.Add(5, 5);

            //    B2
            // B1    B3
            //          R4
            //             R5
            //
            // right-right case
            //
            //    B2
            // B1    B4
            //     R3   R5

            var e2 = tree.RootElement;
            var e4 = e2.Right;
            var e1 = e2.Left;
            var e3 = e4.Left;
            var e5 = e4.Right;

            AssertOrder(e1, e2, e3, e4, e5);

            AssertRoot(e2);
            AssertParent(e4, e2);
            AssertParent(e3, e4);
            AssertParent(e1, e2);
            AssertParent(e5, e4);

            AssertLeaves(e1, e3, e5);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e3.Red);
            Assert.IsTrue(!e4.Red);
            Assert.IsTrue(e5.Red);
        }

        [TestMethod]
        public void AddCaseRightLeft()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(2, 2);
            tree.Add(3, 3);
            tree.Add(1, 1);
            tree.Add(5, 5);


            tree.Add(4, 4);

            //    B2
            // B1    B3
            //          R5
            //         R3
            //
            // right-right case
            //
            //    B2
            // B1    B4
            //     R3  R5

            var e2 = tree.RootElement;
            var e4 = e2.Right;
            var e1 = e2.Left;
            var e3 = e4.Left;
            var e5 = e4.Right;

            AssertOrder(e1, e2, e3, e4, e5);

            AssertRoot(e2);
            AssertParent(e4, e2);
            AssertParent(e3, e4);
            AssertParent(e1, e2);
            AssertParent(e5, e4);

            AssertLeaves(e1, e3, e5);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e3.Red);
            Assert.IsTrue(!e4.Red);
            Assert.IsTrue(e5.Red);
        }

        

        [TestMethod]
        public void AddCaseLeftLeftRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(3, 3); 
            tree.Add(2, 2);
            tree.Add(1, 1);

            //      B3
            //    R2  
            //  R1
            //
            // left-left case, root rotate
            //
            //    B2
            // R1    R3
            //

            var e2 = tree.RootElement;
            var e3 = e2.Right;
            var e1 = e2.Left;

            AssertOrder(e1, e2, e3);

            AssertRoot(e2);
            AssertParent(e1, e2);
            AssertParent(e3, e2);

            AssertLeaves(e1, e3);

            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e1.Red);
            Assert.IsTrue(e3.Red);
        }

        [TestMethod]
        public void AddCaseLeftRightRoot()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(3, 3);
            tree.Add(1, 1);
            tree.Add(2, 2);

            //        B3
            //    R1  
            //      R2
            //
            // left-left case, root rotate
            //
            //    B2
            // R1    R3
            //

            var e2 = tree.RootElement;
            var e3 = e2.Right;
            var e1 = e2.Left;

            AssertOrder(e1, e2, e3);

            AssertRoot(e2);
            AssertParent(e1, e2);
            AssertParent(e3, e2);

            AssertLeaves(e1, e3);

            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e1.Red);
            Assert.IsTrue(e3.Red);
        }


        [TestMethod]
        public void AddCaseParentAndUncleRedRoot2()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(3, 3);
            tree.Add(4, 4);
            tree.Add(2, 2);

            tree.Add(1, 1);

            //       B3
            //    R2    R4
            // R1
            // left-left case, root rotate
            //
            //       B3
            //    B2    B4
            // R1 

            var e3 = tree.RootElement;
            var e4 = e3.Right;
            var e2 = e3.Left;
            var e1 = e2.Left;

            AssertOrder(e1, e2, e3, e4);

            AssertRoot(e3);
            AssertParent(e1, e2);
            AssertParent(e2, e3);
            AssertParent(e4, e3);

            AssertLeaves(e1, e4);
            Assert.IsNull(e2.Right);

            Assert.IsTrue(e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(!e3.Red);
            Assert.IsTrue(!e4.Red);
        }

        [TestMethod]
        public void AddCaseLeftLeft()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(4, 4);
            tree.Add(3, 3);
            tree.Add(5, 5);
            tree.Add(2, 2);

            tree.Add(1, 1);

            //          B4
            //       B3    B5
            //    R2
            // R1 
            // left-left case
            //
            //          B4
            //      B2       B5
            //    R1  R3


            var e4 = tree.RootElement;
            var e5 = e4.Right;
            var e2 = e4.Left;
            var e1 = e2.Left;
            var e3 = e2.Right;

            AssertOrder(e1, e2, e3, e4, e5);

            AssertRoot(e4);
            AssertParent(e1, e2);
            AssertParent(e2, e4);
            AssertParent(e3, e2);
            AssertParent(e5, e4);

            AssertLeaves(e1, e3, e5);

            Assert.IsTrue(e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e3.Red);
            Assert.IsTrue(!e4.Red);
            Assert.IsTrue(!e5.Red);
        }

        [TestMethod]
        public void AddCaseLeftRight()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(4, 4);
            tree.Add(3, 3);
            tree.Add(5, 5);
            tree.Add(1, 1);

            tree.Add(2, 2);

            //            B4
            //        B3      B5
            //    R1
            //      R2 
            // left-left case
            //
            //          B4
            //      B2       B5
            //    R1  R3


            var e4 = tree.RootElement;
            var e5 = e4.Right;
            var e2 = e4.Left;
            var e1 = e2.Left;
            var e3 = e2.Right;

            AssertOrder(e1, e2, e3, e4, e5);

            AssertRoot(e4);
            AssertParent(e1, e2);
            AssertParent(e2, e4);
            AssertParent(e3, e2);
            AssertParent(e5, e4);

            AssertLeaves(e1, e3, e5);

            Assert.IsTrue(e1.Red);
            Assert.IsTrue(!e2.Red);
            Assert.IsTrue(e3.Red);
            Assert.IsTrue(!e4.Red);
            Assert.IsTrue(!e5.Red);
        }


        [TestMethod]
        public void AddCaseParentAndUncleRed()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(5, 5);
            tree.Add(2, 2);
            tree.Add(6, 6);
            tree.Add(1, 1);
            tree.Add(3, 3);

            tree.Add(4, 4);

            //            B5
            //       B2         B6
            //    R1   R3
            //           R4 
            // left-left case
            //            B5
            //       R2         B6
            //    B1   B3
            //           R4 


            var e5 = tree.RootElement;
            var e2 = e5.Left;
            var e6 = e5.Right;
            var e1 = e2.Left;
            var e3 = e2.Right;
            var e4 = e3.Right;

            AssertOrder(e1, e2, e3, e4, e5, e6);

            AssertRoot(e5);
            AssertParent(e2, e5);
            AssertParent(e6, e5);
            AssertParent(e1, e2);
            AssertParent(e3, e2);
            AssertParent(e4, e3);

            AssertLeaves(e1, e4, e6);
            Assert.IsNull(e3.Left);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(e2.Red);
            Assert.IsTrue(!e3.Red);
            Assert.IsTrue(e4.Red);
            Assert.IsTrue(!e5.Red);
            Assert.IsTrue(!e6.Red);
        }


        [TestMethod]
        public void AddCaseParentAndUncleRed2()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();
            tree.Add(5, 5);
            tree.Add(2, 2);
            tree.Add(6, 6);
            tree.Add(1, 1);
            tree.Add(4, 4);

            tree.Add(3, 3);

            //            B5
            //       B2         B6
            //    R1   R4
            //        R3 
            // left-left case
            //            B5
            //       R2         B6
            //    B1   B4
            //        R3 


            var e5 = tree.RootElement;
            var e2 = e5.Left;
            var e6 = e5.Right;
            var e1 = e2.Left;
            var e4 = e2.Right;
            var e3 = e4.Left;

            AssertOrder(e1, e2, e3, e4, e5, e6);

            AssertRoot(e5);
            AssertParent(e2, e5);
            AssertParent(e6, e5);
            AssertParent(e1, e2);
            AssertParent(e3, e4);
            AssertParent(e4, e2);

            AssertLeaves(e1, e3, e6);
            Assert.IsNull(e4.Right);

            Assert.IsTrue(!e1.Red);
            Assert.IsTrue(e2.Red);
            Assert.IsTrue(e3.Red);
            Assert.IsTrue(!e4.Red);
            Assert.IsTrue(!e5.Red);
            Assert.IsTrue(!e6.Red);
        }


        [TestMethod]
        public void StructureIntegrity()
        {
            RedBlackTree<int, int> tree = new RedBlackTree<int, int>();

            int range = 10;
            int[] order = new[] { 1, 3, 2, 5, 6 };

            var q = order
                .SelectMany(x => Enumerable.Range((x - 1) * range + 1, range));


            foreach (int i in q)
            {
                Assert.IsFalse(tree.ContainsKey(i));

                tree.Add(i, i);

                AssertStructure(tree.RootElement);
                Assert.IsTrue(tree.ContainsKey(i));
            }

            foreach (int i in q)
            {
                AssertStructure(tree.RootElement);
                Assert.IsTrue(tree.ContainsKey(i));

                tree.Remove(i);

                Assert.IsFalse(tree.ContainsKey(i));
            }

        }

        #region Assert methods

        private void AssertOrder(params RedBlackTreeNode<int, int>[] orderedNodes)
        {
            for (int i = 0; i < orderedNodes.Length; i++)
            {
                var node = orderedNodes[i];

                Assert.IsNotNull(node);
                Assert.AreEqual(node.Key, i + 1);
            }
        }

        private void AssertLeaves(params RedBlackTreeNode<int, int>[] leaves)
        {
            for (int i = 0; i < leaves.Length; i++)
            {
                Assert.AreEqual(leaves[i].Left, null);
                Assert.AreEqual(leaves[i].Right, null);
            }
        }

        private void AssertRoot(RedBlackTreeNode<int, int> root)
        {
            Assert.AreEqual(root.Parent, null);
        }

        private void AssertParent(RedBlackTreeNode<int, int> child, RedBlackTreeNode<int, int> parent)
        {
            Assert.AreEqual(child.Parent, parent);
        }

        private void AssertStructure(RedBlackTreeNode<int, int> elem)
        {
            if (elem.Left != null)
            {
                Assert.IsTrue(elem.Left.Value < elem.Value);
                Assert.AreEqual(elem.Left.Parent, elem);
                AssertStructure(elem.Left);
            }

            if (elem.Right != null)
            {
                Assert.IsTrue(elem.Value < elem.Right.Value);
                Assert.AreEqual(elem.Right.Parent, elem);
                AssertStructure(elem.Right);
            }
        }

        #endregion



    }
}
