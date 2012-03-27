using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NMemory.DataStructures.Internal.Trees
{
    /// <summary>
    /// Red-Black tree data structure
    /// Based on: http://www.jot.fm/issues/issue_2005_03/column6/
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class RedBlackTree<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IComparer<TKey> comparer;

        private RedBlackTreeNode<TKey, TValue> root;
        private int count;

        private RedBlackTreeNode<TKey, TValue> nodeBeingDeleted; // Set in DeleteNode 
        private bool siblingToRight;   // Sibling of curNode 
        private bool parentToRight;    // Of grand parent 
        private bool nodeToDeleteRed;  // Color of deleted node

        private object syncRoot = new object();

        public RedBlackTree()
            : this(Comparer<TKey>.Default)
        {
        }

        public RedBlackTree(IComparer<TKey> comparer)
        {
            this.comparer = comparer;
        }


        internal RedBlackTreeNode<TKey, TValue> RootElement
        {
            get { return this.root; }
        }

        #region IDictionary methods

        #region Keys and Values

        public ICollection<TKey> Keys
        {
            get
            {
                return this.InorderedTraversal()
                    .Select(n => n.Key)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this.InorderedTraversal()
                    .Select(n => n.Value)
                    .ToList()
                    .AsReadOnly();
            }
        }

        #endregion

        #region Contains

        public bool ContainsKey(TKey key)
        {
            return this.FindNode(key, this.root) != null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.ContainsKey(item.Key);
        }

        #endregion

        #region Getter and Setter

        public TValue this[TKey key]
        {
            get
            {
                var node = this.FindNode(key, this.root);

                if (node == null)
                {
                    throw new IndexOutOfRangeException();
                }

                return node.Value;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = this.FindNode(key, this.root);

            if (node != null)
            {
                value = node.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        #endregion

        #region Add and Remove

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
            {
                throw new InvalidOperationException("Duplication key");
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            RedBlackTreeNode<TKey, TValue> insertedNode = this.InsertNode(key, value);

            if (insertedNode == null)
            {
                return false;
            }

            count++;

            if (count > 2)
            {
                RedBlackTreeNode<TKey, TValue> parent, grandParent, greatGrandParent;

                GetNodesAbove(
                    insertedNode,
                    out parent,
                    out grandParent,
                    out greatGrandParent);

                FixTreeAfterInsertion(
                    insertedNode,
                    parent,
                    grandParent,
                    greatGrandParent);
            }

            return true;
        }

        public bool Remove(TKey key)
        {
            ////if (count > 1)
            ////{
            ////    var deleted = DeleteNode(key);

            ////    if (deleted == null)
            ////    {
            ////        return false;
            ////    }

            ////    count--;

            ////    RedBlackTreeNode<TKey, TValue> curNode = null; 
            ////    // Right node being deleted 
            ////    if (nodeBeingDeleted.Right != null)
            ////    {
            ////        curNode = nodeBeingDeleted.Right;
            ////    }

            ////    RedBlackTreeNode<TKey, TValue> parent, sibling, grandParent;
            ////    if (curNode == null)
            ////    {
            ////        parent = nodeBeingDeleted.Parent;
            ////    }
            ////    else
            ////    {
            ////        parent = curNode.Parent;
            ////    }

            ////    GetParentGrandParentSibling(
            ////        curNode,
            ////        parent,
            ////        out sibling,
            ////        out grandParent);

            ////    if (curNode != null && curNode.Red)
            ////    {
            ////        curNode.Red = false;
            ////    }
            ////    else if (!nodeToDeleteRed && !nodeBeingDeleted.Red)
            ////    {
            ////        FixTreeAfterDeletion(
            ////            curNode,
            ////            parent,
            ////            sibling,
            ////            grandParent);
            ////    }

            ////    root.Red = false;
            ////    return true;
            ////}
            ////else if (count == 1)
            ////{
            ////    if (this.comparer.Compare(this.root.Key, key) == 0)
            ////    {
            ////        this.root = null;
            ////        count = 0;

            ////        return true;
            ////    }
            ////}

            ////return false;

            if (count > 1)
            {
                if (!DeleteNode(root, key, null, out root))
                {
                    return false;
                }

                count--;

                RedBlackTreeNode<TKey, TValue> curNode = null; // Right node being deleted 
                if (nodeBeingDeleted.Right != null)
                {
                    curNode = nodeBeingDeleted.Right;
                }

                RedBlackTreeNode<TKey, TValue> parent, sibling, grandParent;
                if (curNode == null)
                {
                    parent = nodeBeingDeleted.Parent;
                }
                else
                {
                    parent = curNode.Parent;
                }

                GetParentGrandParentSibling(
                    curNode,
                    parent,
                    out sibling,
                    out grandParent);

                if (curNode != null && curNode.Red)
                {
                    curNode.Red = false;
                }
                else if (!nodeToDeleteRed && !nodeBeingDeleted.Red)
                {
                    FixTreeAfterDeletion(
                        curNode,
                        parent,
                        sibling,
                        grandParent);
                }

                root.Red = false;
                return true;
            }
            else if (root != null)
            {
                if (this.comparer.Compare(key, root.Key) == 0)
                {
                    this.root = null;
                    this.count = 0;

                    return true;
                }
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        #endregion

        #region Misc

        public void Clear()
        {
            this.root = null;
            this.count = 0;
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return this.count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.InorderedTraversal()
               .Select(n =>
                   new KeyValuePair<TKey, TValue>(
                       n.Key,
                       n.Value))
               .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InorderedTraversal()
               .Select(n =>
                   new KeyValuePair<TKey, TValue>(
                       n.Key,
                       n.Value))
               .GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return syncRoot; }
        }

        #endregion

        #endregion


        #region Search

        public IEnumerable<TValue> IntervalSearch(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            if (this.root == null)
            {
                return new TValue[0];
            }

            return this.InOrderTraversal(this.root, from, to, fromOpen, toOpen).Select(n => n.Value);
        }

        public IEnumerable<TValue> SearchGreater(TKey from, bool open)
        {
            if (this.root == null)
            {
                return new TValue[0];
            }

            return this.InOrderTraversalFrom(this.root, from, open).Select(n => n.Value);
        }

        public IEnumerable<TValue> SearchLess(TKey to, bool open)
        {
            if (this.root == null)
            {
                return new TValue[0];
            }

            return this.InOrderTraversalTo(this.root, to, open).Select(n => n.Value);
        }

        private IEnumerable<RedBlackTreeNode<TKey, TValue>> InOrderTraversal(RedBlackTreeNode<TKey, TValue> node, TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            int fromCompare = this.comparer.Compare(from, node.Key);
            int toCompare = this.comparer.Compare(node.Key, to);

            if (fromCompare < 0 && node.Left != null)
            {
                // Node is greater than 'from'
                foreach (RedBlackTreeNode<TKey, TValue> leftNode in InOrderTraversal(node, from, to, fromOpen, toOpen))
                { 
                    yield return leftNode;
                }
            }

            if (((fromCompare == 0 && fromOpen) || fromCompare < 0) && // Equals to or greater than 'from'
                ((toCompare == 0 && toOpen) || toCompare < 0)) // Equals to or smaller than 'to'
            {
                yield return node;
            }

            if (toCompare < 0 && node.Right != null)
            {
                // Node is smaller than 'to'
                foreach (RedBlackTreeNode<TKey, TValue> rightNode in InOrderTraversal(node.Right, from, to, fromOpen, toOpen))
                {
                    yield return rightNode;
                }
            }
        }

        private IEnumerable<RedBlackTreeNode<TKey, TValue>> InOrderTraversalFrom(RedBlackTreeNode<TKey, TValue> node, TKey from, bool fromOpen)
        {
            int fromCompare = this.comparer.Compare(from, node.Key);

            if (fromCompare < 0 && node.Left != null)
            {
                // Node is greater than 'from'
                foreach (RedBlackTreeNode<TKey, TValue> leftNode in InOrderTraversalFrom(node, from, fromOpen))
                {
                    yield return leftNode;
                }
            }

            // Equals to or greater than 'from'
            if (((fromCompare == 0 && fromOpen) || fromCompare < 0)) 
            {
                yield return node;
            }

            if (node.Right != null)
            {
                // Node is always smaller than 'to'
                foreach (RedBlackTreeNode<TKey, TValue> rightNode in InOrderTraversalFrom(node.Right, from, fromOpen))
                {
                    yield return rightNode;
                }
            }
        }

        private IEnumerable<RedBlackTreeNode<TKey, TValue>> InOrderTraversalTo(RedBlackTreeNode<TKey, TValue> node, TKey to, bool toOpen)
        {
            int toCompare = this.comparer.Compare(node.Key, to);

            if (node.Left != null)
            {
                // Node is always greater than 'from'
                foreach (RedBlackTreeNode<TKey, TValue> leftNode in InOrderTraversalTo(node, to, toOpen))
                {
                    yield return leftNode;
                }
            }

            if (((toCompare == 0 && toOpen) || toCompare < 0)) // Equals to or smaller than 'to'
            {
                yield return node;
            }

            if (toCompare < 0 && node.Right != null)
            {
                // Node is smaller than 'to'
                foreach (RedBlackTreeNode<TKey, TValue> rightNode in InOrderTraversalTo(node.Right, to, toOpen))
                {
                    yield return rightNode;
                }
            }
        }


        private RedBlackTreeNode<TKey, TValue> FindNode(TKey key, RedBlackTreeNode<TKey, TValue> node)
        {
            if (node == null)
            {
                return null;
            }

            var currentNode = node;

            while (currentNode != null)
            {
                var compare = comparer.Compare(key, currentNode.Key);

                if (compare < 0)
                {
                    currentNode = currentNode.Left;
                }
                else if (compare > 0)
                {
                    currentNode = currentNode.Right;
                }
                else
                {
                    return currentNode;
                }
            }

            return null;
        }

        private IEnumerable<RedBlackTreeNode<TKey, TValue>> InorderedTraversal()
        {
            if (this.root == null)
            {
                yield break;
            }

            // Need for this?
            int capacity = (int)Math.Truncate(2 * Math.Log(this.count + 1) / 0.301);
            Stack<RedBlackTreeNode<TKey, TValue>> stack = new Stack<RedBlackTreeNode<TKey, TValue>>(capacity);

            bool done = false;
            RedBlackTreeNode<TKey, TValue> current = this.root;
            int max = 0;
            while (!done)
            {
                max = Math.Max(max, stack.Count);
                if (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    if (stack.Count > 0)
                    {
                        current = stack.Pop();
                        yield return current;

                        current = current.Right;
                    }
                    else
                    {
                        done = true;
                    }
                }

            }
        }

        #endregion

        #region Core insert and delete methods

        private RedBlackTreeNode<TKey, TValue> InsertNode(TKey key, TValue value)
        {
            bool right = false;

            RedBlackTreeNode<TKey, TValue> parent = null;
            RedBlackTreeNode<TKey, TValue> node = this.root;

            while (node != null)
            {
                int compare = this.comparer.Compare(key, node.Key);

                if (compare < 0)
                {
                    parent = node;
                    node = node.Left;
                    right = false;
                }
                else if (compare > 0)
                {
                    parent = node;
                    node = node.Right;
                    right = true;
                }
                else
                {
                    return null;
                }
            }

            RedBlackTreeNode<TKey, TValue> newNode =
                new RedBlackTreeNode<TKey, TValue>(key, value, parent);

            if (parent != null)
            {
                if (right)
                {
                    parent.Right = newNode;
                }
                else
                {
                    parent.Left = newNode;
                }

                newNode.Red = true;
            }
            else
            {
                // no parent, it is the root
                this.root = newNode;
                newNode.Red = false;
            }

            return newNode;
        }

        ////private RedBlackTreeNode<TKey, TValue> DeleteNode(TKey key)
        ////{
        ////    RedBlackTreeNode<TKey, TValue> parent = null;
        ////    RedBlackTreeNode<TKey, TValue> node = this.root;
        ////    bool right = false;

        ////    while(node != null || this.comparer.Compare(node.Key, key) == 0)
        ////    {
        ////        int compare = this.comparer.Compare(key, node.Key);

        ////        if (compare < 0)
        ////        {
        ////            parent = node;
        ////            node = node.Left;
        ////            right = false;
        ////        }
        ////        else if (compare > 0)
        ////        {
        ////            parent = node;
        ////            node = node.Right;
        ////            right = true;
        ////        }
        ////        else
        ////        {
        ////            return null;
        ////        }
        ////    }

        ////    if (node == null)
        ////    {
        ////        return null;
        ////    }

        ////    RedBlackTreeNode<TKey, TValue> change = null;

        ////    if (node.Left == null)
        ////    {
        ////        // Only right child (it can be null too)
        ////        change = node.Right;
        ////    }
        ////    else if (node.Right == null)
        ////    {
        ////        // On left child (it is not null)
        ////        change = node.Left;
        ////    }
        ////    else
        ////    { 
        ////        // Two children 
        ////        // Deletes using the leftmost node of the right subtree 

        ////        change = LeftMost(node.Right);
        ////        // change.Left == null !

        ////        if (change.Parent != node)
        ////        {
        ////            change.Parent.Left = change.Right;
        ////            if (change.Right != null)
        ////            {
        ////                change.Right.Parent = change.Parent;
        ////            }
        ////        }

        ////        change.Left = node.Left;
        ////        node.Left.Parent = change;
        ////    }

        ////    // Set the parent
        ////    change.Parent = parent;

        ////    if (parent != null)
        ////    {
        ////        // Set the child of the parent
        ////        if (right)
        ////        {
        ////            parent.Right = change;
        ////        }
        ////        else
        ////        {
        ////            parent.Left = change;
        ////        }
        ////    }
        ////    else
        ////    {
        ////        // Set as root
        ////        this.root = change;
        ////    }

        ////    return node;
        ////}

        private bool DeleteNode(
          RedBlackTreeNode<TKey, TValue> node,
          TKey item,
          RedBlackTreeNode<TKey, TValue> parent,
          out RedBlackTreeNode<TKey, TValue> result)
        {
            RedBlackTreeNode<TKey, TValue> temp;

            if (node == null)
            {
                result = null;
                return false;
            }

            if (this.comparer.Compare(item, node.Key) < 0)
            {
                if (!DeleteNode(node.Left, item, node, out temp))
                {
                    result = null;
                    return false;
                }

                node.Left = temp;
            }
            else if (this.comparer.Compare(item, node.Key) > 0)
            {
                if (!DeleteNode(node.Right, item, node, out temp))
                {
                    result = null;
                    return false;
                }

                node.Right = temp;
            }
            else if (this.comparer.Compare(item, node.Key) == 0)
            {
                // Item found 
                nodeToDeleteRed = node.Red;
                nodeBeingDeleted = node;
                if (node.Left == null)
                {
                    // No children or only a right child   
                    node = node.Right;
                    if (node != null)
                    {
                        node.Parent = parent;
                    }
                }
                else if (node.Right == null)
                {
                    // Only a left child   
                    node = node.Left;
                    node.Parent = parent;
                }
                else
                { // Two children 
                    // Deletes using the leftmost Node<T,TValue> of the  
                    // right subtree 
                    var replace = LeftMost(node.Right);
                    node.Right = DeleteLeftMost(node.Right, node);

                    node.Key = replace.Key;
                    node.Value = replace.Value;
                }
            }

            result = node;
            return true;
        }

        #endregion

        #region Structure handling

        private void RightRotate(ref RedBlackTreeNode<TKey, TValue> node)
        {
            RedBlackTreeNode<TKey, TValue> nodeParent = node.Parent;
            RedBlackTreeNode<TKey, TValue> left = node.Left;
            RedBlackTreeNode<TKey, TValue> temp = left.Right;

            left.Right = node;
            node.Parent = left;
            node.Left = temp;

            if (temp != null)
            {
                temp.Parent = node;
            }

            if (left != null)
            {
                left.Parent = nodeParent;
            }

            node = left;
        }


        private void LeftRotate(ref RedBlackTreeNode<TKey, TValue> node)
        {
            RedBlackTreeNode<TKey, TValue> nodeParent = node.Parent;
            RedBlackTreeNode<TKey, TValue> right = node.Right;
            RedBlackTreeNode<TKey, TValue> temp = right.Left;

            right.Left = node;
            node.Parent = right;
            node.Right = temp;

            if (temp != null)
            {
                temp.Parent = node;
            }

            if (right != null)
            {
                right.Parent = nodeParent;
            }

            node = right;
        }

        private void GetNodesAbove(
            RedBlackTreeNode<TKey, TValue> curNode,
            out RedBlackTreeNode<TKey, TValue> parent,
            out RedBlackTreeNode<TKey, TValue> grandParent,
            out RedBlackTreeNode<TKey, TValue> greatGrandParent)
        {
            parent = null;
            grandParent = null;
            greatGrandParent = null;
            if (curNode != null)
            {
                parent = curNode.Parent;
            }
            if (parent != null)
            {
                grandParent = parent.Parent;
            }
            if (grandParent != null)
            {
                greatGrandParent = grandParent.Parent;
            }
        }

        private void GetParentGrandParentSibling(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            out RedBlackTreeNode<TKey, TValue> sibling,
            out RedBlackTreeNode<TKey, TValue> grandParent)
        {
            sibling = null;
            grandParent = null;

            if (parent != null)
            {
                if (parent.Right == curNode)
                {
                    siblingToRight = false;
                    sibling = parent.Left;
                }
                if (parent.Left == curNode)
                {
                    siblingToRight = true;
                    sibling = parent.Right;
                }
            }
            if (parent != null)
            {
                grandParent = parent.Parent;
            }
            if (grandParent != null)
            {
                if (grandParent.Right == parent)
                {
                    parentToRight = true;
                }
                if (grandParent.Left == parent)
                {
                    parentToRight = false;
                }
            }
        }

        private void FixTreeAfterInsertion(
            RedBlackTreeNode<TKey, TValue> child,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> grandParent,
            RedBlackTreeNode<TKey, TValue> greatGrandParent)
        {
            if (grandParent != null)
            {
                RedBlackTreeNode<TKey, TValue> uncle =
                    grandParent.Right == parent ?
                        grandParent.Left :
                        grandParent.Right;

                if (uncle != null &&
                    parent.Red &&
                    uncle.Red)
                {
                    uncle.Red = false;
                    parent.Red = false;
                    grandParent.Red = true;

                    RedBlackTreeNode<TKey, TValue> higher = null;
                    RedBlackTreeNode<TKey, TValue> stillHigher = null;

                    if (greatGrandParent != null)
                    {
                        higher = greatGrandParent.Parent;
                    }

                    if (higher != null)
                    {
                        stillHigher = higher.Parent;
                    }

                    FixTreeAfterInsertion(
                        grandParent,
                        greatGrandParent,
                        higher,
                        stillHigher);
                }
                else if (
                    uncle == null ||
                    parent.Red &&
                    !uncle.Red)
                {
                    if (grandParent.Right == parent &&
                        parent.Right == child)
                    {
                        // right-right case 
                        parent.Red = false;
                        grandParent.Red = true;

                        if (greatGrandParent != null)
                        {
                            if (greatGrandParent.Right == grandParent)
                            {
                                LeftRotate(ref grandParent);
                                greatGrandParent.Right = grandParent;
                            }
                            else
                            {
                                LeftRotate(ref grandParent);
                                greatGrandParent.Left = grandParent;
                            }
                        }
                        else
                        {
                            LeftRotate(ref root);
                        }
                    }
                    else if (
                        grandParent.Left == parent &&
                        parent.Left == child)
                    {
                        // left-left case 
                        parent.Red = false;
                        grandParent.Red = true;

                        if (greatGrandParent != null)
                        {
                            if (greatGrandParent.Right == grandParent)
                            {
                                RightRotate(ref grandParent);
                                greatGrandParent.Right = grandParent;
                            }
                            else
                            {
                                RightRotate(ref grandParent);
                                greatGrandParent.Left = grandParent;
                            }
                        }
                        else
                        {
                            RightRotate(ref root);
                        }
                    }
                    else if (
                        grandParent.Right == parent &&
                        parent.Left == child)
                    {
                        // right-left case 
                        child.Red = false;
                        grandParent.Red = true;

                        RightRotate(ref parent);
                        grandParent.Right = parent;

                        if (greatGrandParent != null)
                        {
                            if (greatGrandParent.Right == grandParent)
                            {
                                LeftRotate(ref grandParent);
                                greatGrandParent.Right = grandParent;
                            }
                            else
                            {
                                LeftRotate(ref grandParent);
                                greatGrandParent.Left = grandParent;
                            }
                        }
                        else
                        {
                            LeftRotate(ref root);
                        }
                    }
                    else if (
                        grandParent.Left == parent &&
                        parent.Right == child)
                    {
                        // left-right case 

                        child.Red = false;
                        grandParent.Red = true;
                        LeftRotate(ref parent);
                        grandParent.Left = parent;

                        if (greatGrandParent != null)
                        {
                            if (greatGrandParent.Right == grandParent)
                            {
                                RightRotate(ref grandParent);
                                greatGrandParent.Right = grandParent;
                            }
                            else
                            {
                                RightRotate(ref grandParent);
                                greatGrandParent.Left = grandParent;
                            }
                        }
                        else
                        {
                            RightRotate(ref root);
                        }
                    }
                }
                if (root.Red)
                {
                    root.Red = false;
                }
            }
        }

        private RedBlackTreeNode<TKey, TValue> LeftMost(RedBlackTreeNode<TKey, TValue> node)
        {
            while (node.Left != null)
            {
                node = node.Left;
            }

            return node;
        }

        private RedBlackTreeNode<TKey, TValue> DeleteLeftMost(RedBlackTreeNode<TKey, TValue> node, RedBlackTreeNode<TKey, TValue> parent)
        {
            if (node.Left == null)
            {
                nodeBeingDeleted = node;
                if (node.Right != null)
                {
                    node.Right.Parent = parent;
                }
                return node.Right;

            }
            else
            {
                node.Left = DeleteLeftMost(node.Left, node);
                return node;
            }
        }
        private void FixTreeAfterDeletion(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            RedBlackTreeNode<TKey, TValue> siblingLeftChild = null;
            RedBlackTreeNode<TKey, TValue> siblingRightChild = null;
            if (sibling != null && sibling.Left != null)
            {
                siblingLeftChild = sibling.Left;
            }
            if (sibling != null && sibling.Right != null)
            {
                siblingRightChild = sibling.Right;
            }
            bool siblingRed = (sibling != null && sibling.Red);
            bool siblingLeftRed = (siblingLeftChild != null
                   && siblingLeftChild.Red);
            bool siblingRightRed = (siblingRightChild != null &&
           siblingRightChild.Red);

            if (parent != null &&
                !parent.Red &&
                siblingRed &&
                !siblingLeftRed &&
                !siblingRightRed)
            {
                Case1(curNode, parent, sibling, grandParent);
            }
            else if (
                parent != null &&
                !parent.Red &&
                !siblingRed &&
                !siblingLeftRed &&
                !siblingRightRed)
            {
                Case2A(curNode, parent, sibling, grandParent);
            }
            else if (
                parent != null &&
                parent.Red &&
                !siblingRed &&
                !siblingLeftRed &&
                !siblingRightRed)
            {
                Case2B(curNode, parent, sibling, grandParent);
            }
            else if (
                siblingToRight &&
                !siblingRed &&
                siblingLeftRed &&
                !siblingRightRed)
            {
                Case3(curNode, parent, sibling, grandParent);
            }
            else if (
                !siblingToRight &&
                !siblingRed &&
                !siblingLeftRed &&
                siblingRightRed)
            {
                Case3P(curNode, parent, sibling, grandParent);
            }
            else if (
                siblingToRight &&
                !siblingRed &&
                siblingRightRed)
            {
                Case4(curNode, parent, sibling, grandParent);
            }
            else if (
                !siblingToRight &&
                !siblingRed &&
                siblingLeftRed)
            {
                Case4P(curNode, parent, sibling, grandParent);
            }
        }

        private void Case1(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            if (siblingToRight)
            {
                parent.Red = !parent.Red;
                sibling.Red = !sibling.Red;

                if (grandParent != null)
                {
                    if (parentToRight)
                    {
                        LeftRotate(ref parent);
                        grandParent.Right = parent;
                    }
                    else if (!parentToRight)
                    {
                        LeftRotate(ref parent);
                        grandParent.Left = parent;
                    }
                }
                else
                {
                    LeftRotate(ref parent);
                    root = parent;
                }

                grandParent = sibling;
                parent = parent.Left;
                parentToRight = false;
            }
            else if (!siblingToRight)
            {
                parent.Red = !parent.Red;
                sibling.Red = !sibling.Red;

                if (grandParent != null)
                {
                    if (parentToRight)
                    {
                        RightRotate(ref parent);
                        grandParent.Right = parent;
                    }
                    else if (!parentToRight)
                    {
                        RightRotate(ref parent);
                        grandParent.Left = parent;
                    }
                }
                else
                {
                    RightRotate(ref parent);
                    root = parent;
                }

                grandParent = sibling;
                parent = parent.Right;
                parentToRight = true;
            }

            if (parent.Right == curNode)
            {
                sibling = parent.Left;
                siblingToRight = false;
            }
            else if (parent.Left == curNode)
            {
                sibling = parent.Right;
                siblingToRight = true;
            }

            RedBlackTreeNode<TKey, TValue> siblingLeftChild = null;
            RedBlackTreeNode<TKey, TValue> siblingRightChild = null;

            if (sibling != null && sibling.Left != null)
            {
                siblingLeftChild = sibling.Left;
            }
            if (sibling != null && sibling.Right != null)
            {
                siblingRightChild = sibling.Right;
            }

            bool siblingRed = (sibling != null && sibling.Red);
            bool siblingLeftRed =
                siblingLeftChild != null &&
                siblingLeftChild.Red;

            bool siblingRightRed =
                siblingRightChild != null &&
                siblingRightChild.Red;

            if (parent.Red &&
                !siblingRed &&
                !siblingLeftRed &&
                !siblingRightRed)
            {
                Case2B(curNode, parent, sibling, grandParent);
            }
            else if (
                siblingToRight &&
                !siblingRed &&
                siblingLeftRed &&
                !siblingRightRed)
            {
                Case3(curNode, parent, sibling, grandParent);
            }
            else if (
                !siblingToRight &&
                !siblingRed &&
                !siblingLeftRed &&
                siblingRightRed)
            {
                Case3P(curNode, parent, sibling, grandParent);
            }
            else if (
                siblingToRight &&
                !siblingRed &&
                siblingRightRed)
            {
                Case4(curNode, parent, sibling, grandParent);
            }
            else if (
                !siblingToRight &&
                !siblingRed &&
                siblingLeftRed)
            {
                Case4P(curNode, parent, sibling, grandParent);
            }
        }

        private void Case2A(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            if (sibling != null)
            {
                sibling.Red = !sibling.Red;
            }

            curNode = parent;
            if (curNode != root)
            {
                parent = curNode.Parent;
                GetParentGrandParentSibling(
                    curNode,
                    parent,
                    out sibling,
                    out grandParent);

                RedBlackTreeNode<TKey, TValue> siblingLeftChild = null;
                RedBlackTreeNode<TKey, TValue> siblingRightChild = null;

                if (sibling != null && sibling.Left != null)
                {
                    siblingLeftChild = sibling.Left;
                }

                if (sibling != null && sibling.Right != null)
                {
                    siblingRightChild = sibling.Right;
                }

                bool siblingRed =
                    sibling != null
                    && sibling.Red;

                bool siblingLeftRed =
                    siblingLeftChild != null &&
                    siblingLeftChild.Red;

                bool siblingRightRed =
                    siblingRightChild != null &&
                    siblingRightChild.Red;

                if (parent != null &&
                    !parent.Red &&
                    !siblingRed &&
                    !siblingLeftRed &&
                    !siblingRightRed)
                {
                    Case2A(curNode, parent, sibling, grandParent);
                }
                else if (
                    parent != null &&
                    parent.Red &&
                    !siblingRed &&
                    !siblingLeftRed &&
                    !siblingRightRed)
                {
                    Case2B(curNode, parent, sibling, grandParent);
                }
                else if (
                    siblingToRight &&
                    !siblingRed &&
                    siblingLeftRed &&
                    !siblingRightRed)
                {
                    Case3(curNode, parent, sibling, grandParent);
                }
                else if (
                    !siblingToRight &&
                    !siblingRed &&
                    !siblingLeftRed &&
                    siblingRightRed)
                {
                    Case3P(curNode, parent, sibling, grandParent);
                }
                else if (
                    siblingToRight &&
                    !siblingRed &&
                    siblingRightRed)
                {
                    Case4(curNode, parent, sibling, grandParent);
                }
                else if (
                    !siblingToRight &&
                    !siblingRed &&
                    siblingLeftRed)
                {
                    Case4P(curNode, parent, sibling, grandParent);
                }
            }
        }

        private void Case2B(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
             RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            if (sibling != null)
            {
                sibling.Red = !sibling.Red;

            }
            curNode = parent;
            curNode.Red = !curNode.Red;
        }

        private void Case3(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            if (parent.Left == curNode)
            {
                sibling.Red = true;
                sibling.Left.Red = false;

                RightRotate(ref sibling);
                parent.Right = sibling;
            }

            Case4(curNode, parent, sibling, grandParent);
        }

        private void Case3P(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            if (parent.Right == curNode)
            {
                sibling.Red = true;
                sibling.Right.Red = false;
                LeftRotate(ref sibling);
                parent.Left = sibling;
            }
            Case4P(curNode, parent, sibling, grandParent);
        }

        private void Case4(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            sibling.Red = parent.Red;
            sibling.Right.Red = false;
            parent.Red = false;

            if (grandParent != null)
            {
                if (parentToRight)
                {
                    LeftRotate(ref parent);
                    grandParent.Right = parent;
                }
                else
                {
                    LeftRotate(ref parent);
                    grandParent.Left = parent;
                }
            }
            else
            {
                LeftRotate(ref parent);
                root = parent;
            }
        }

        private void Case4P(
            RedBlackTreeNode<TKey, TValue> curNode,
            RedBlackTreeNode<TKey, TValue> parent,
            RedBlackTreeNode<TKey, TValue> sibling,
            RedBlackTreeNode<TKey, TValue> grandParent)
        {
            sibling.Red = parent.Red;
            sibling.Left.Red = false;
            parent.Red = false;

            if (grandParent != null)
            {
                if (parentToRight)
                {
                    RightRotate(ref parent);
                    grandParent.Right = parent;
                }
                else
                {
                    RightRotate(ref parent);
                    grandParent.Left = parent;
                }
            }
            else
            {
                RightRotate(ref parent);
                root = parent;
            }
        }

        #endregion

    }
}
