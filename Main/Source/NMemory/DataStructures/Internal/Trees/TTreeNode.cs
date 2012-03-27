using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace NMemory.DataStructures.Internal.Trees
{
    /// <summary>
    /// T-Tree implementation.
    /// 
    /// “A T-tree is a balanced index tree data structure optimized for cases where both the 
    /// index and the actual data are fully kept in memory, just as a B-tree is an index structure 
    /// optimized for storage on block oriented external storage devices like hard disks. T-trees 
    /// seek to gain the performance benefits of in-memory tree structures such as AVL trees while 
    /// avoiding the large storage space overhead which is common to them.” 
    /// (from http://en.wikipedia.org/wiki/T-tree)
    ///
    /// Also google “A Study of Index Structures for Main Memory Database Management Systems” 
    /// for a comprehensive discussion of T-Trees
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TTreeNode<TKey, TValue> : ITTreeNode<TKey, TValue>
        where TKey : IComparable
    {
        private readonly TValue[] values;
        private readonly TKey[] keys;

        private readonly int minimum;
        private int m_height = 0;

        private readonly TTree<TKey, TValue> parentTree;

        public TTreeNode(int minimum, int maximum, TTree<TKey, TValue> root)
        {
            #region param checks

            if (minimum < 1)
                throw new ArgumentOutOfRangeException("minimum", "Expecting a minimum of at least 1.");

            if (maximum < minimum)
                throw new ArgumentOutOfRangeException("maximum", "Maximum value must be greater than the minimum. ");

            if (root == null)
                throw new ArgumentNullException("root");

            #endregion

            ItemCount = 0;
            keys = new TKey[maximum];
            values = new TValue[maximum];
            this.minimum = minimum;
            parentTree = root;
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if the item was added or false if it already existed and was not </returns>
        public bool AddItem(TKey key, TValue value)
        {
            bool isBoundingNode;
            int comparedToFirst = 0;

            //Is this the bounding node for the new item? If the node is empty it is considered to be the bounding node.
            if (ItemCount == 0)
            {
                isBoundingNode = true;
            }
            else
            {
                //Compare the item to be inserted to the first item in the data
                comparedToFirst = key.CompareTo(keys[0]);
                isBoundingNode = ((comparedToFirst >= 0) && (key.CompareTo(keys[ItemCount - 1]) <= 0));
            }

            if (isBoundingNode)
            {
                //Is there space in this node?
                if (ItemCount < keys.Length)
                {
                    //This is the bounding node, add the new item
                    return InsertInCurrentNode(key, value, false);
                }
                else
                {
                    //Copy the old minimum. This current item will be inserted into this node
                    TKey oldMinimumKey = keys[0];
                    TValue oldMinimumValue = values[0];

                    if (!InsertInCurrentNode(key, value, true))
                        return false;

                    //Add the old minimum
                    if (Left == null)
                    {
                        //There is no left child, so create it
                        Left = CreateChild(oldMinimumKey, oldMinimumValue);
                        UpdateHeight(false);
                        Rebalance(true, false);
                        return true;
                    }
                    else
                    {
                        //Add the old minimum to the left child
                        return Left.AddItem(oldMinimumKey, oldMinimumValue);
                    }
                }
            }
            else
            {
                //If the item is less than the minimum and there is a left node, follow it
                if ((Left != null) && (comparedToFirst < 0))
                {
                    return Left.AddItem(key, value);
                }

                //If the item is less than the maximum and there is a right node, follow it
                if ((Right != null) && (comparedToFirst > 0))
                {
                    return Right.AddItem(key, value);
                }

                //If we are here then, there is no bounding node for this value.
                //Is there place in this node
                if (ItemCount < keys.Length)
                {
                    //There is place in this node so add the new value. However since this value
                    // must be the new minimum or maximum (otherwise it would have found a bounding
                    // node) dont call InsertInCurrentNode which would do a binary search to find
                    // an insert location. Rather check for min/max and add it here.
                    if (comparedToFirst > 0)
                    {
                        //The item is greater than the minimum, therfore it must be the new maximum.
                        // Add it to the end of the array
                        keys[ItemCount] = key;
                        values[ItemCount] = value;
                    }
                    else
                    {
                        //The item is the new miminum. Shift the array up and insert it.
                        Array.Copy(values, 0, values, 1, ItemCount);
                        Array.Copy(keys, 0, keys, 1, ItemCount);
                        values[0] = value;
                        keys[0] = key;
                    }

                    ItemCount++;
                }
                else
                {
                    TTreeNode<TKey, TValue> newChild = CreateChild(key, value);

                    //Add it as the the left or the right child
                    if (comparedToFirst < 0)
                    {
                        Left = newChild;
                    }
                    else
                    {
                        Right = newChild;
                    }

                    UpdateHeight(true);
                    Rebalance(true, false);
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            AddItem(key, value);
        }

        /// <summary>
        /// Inserts an item into the current node. This method can be called in two instances
        /// 1 - When this node is not full
        /// 2 - When this node is full and the first item has been copied so that it can be
        /// used as the new insert value. 
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="fullShiftToLeft">if set to <c>true</c> [full shift to left].</param>
        /// <returns></returns>
        private bool InsertInCurrentNode(TKey key, TValue value, bool fullShiftToLeft)
        {
            Debug.Assert(fullShiftToLeft ? (ItemCount == keys.Length) : (ItemCount < keys.Length), "If doing a shift left, the node should have been full, otherwise it should not be called if the node is full");

            //TODO profile and see if there is any advantage to checking the min and max values here and setting closest=0 or closest=Count. Remember to check that the item was not already added

            //Find the position with the closest value to the item being added.
            int closest = Array.BinarySearch<TKey>(keys, 0, ItemCount, key);

            //If closest is positive then the item already exists at this level, so
            // no need to add it again
            if (closest >= 0)
                return false;

            //Negate the result, which gives info about where the closest match is
            closest = ~closest;

            //If closest is greater than the count then there is no item in the array than the item being added,
            // so add it to the end of the array. Otherwise the negated value is the position to add the item to
            if (closest > ItemCount)
                closest = ItemCount;

            if (!fullShiftToLeft)
            {
                //Shift the items up by one place to make space for the new item. This also works when adding
                // an item to the end of the array.
                Array.Copy(keys, closest, keys, closest + 1, ItemCount - closest);
                Array.Copy(values, closest, values, closest + 1, ItemCount - closest);
                keys[closest] = key;
                values[closest] = value;

                //An item has been added.
                ItemCount++;
            }
            else
            {
                //Shift the items before the insertion point to the left.
                Array.Copy(keys, 1, keys, 0, closest - 1);
                Array.Copy(values, 1, values, 0, closest - 1);
                keys[closest - 1] = key;
                values[closest - 1] = value;
            }

            return true;
        }

        private void Rebalance(bool stopAfterFirstRotate, bool stopAfterEvenBalanceFound)
        {
            if (BalanceFactor > 1)
            {
                if (Left.BalanceFactor > 0)
                {
                    RotateLL();

                    if (stopAfterFirstRotate)
                        return;
                }
                else
                {
                    RotateLR();

                    if (stopAfterFirstRotate)
                        return;
                }
            }
            else if (BalanceFactor < -1)
            {
                if (Right.BalanceFactor < 0)
                {
                    RotateRR();

                    if (stopAfterFirstRotate)
                        return;
                }
                else
                {
                    RotateRL();

                    if (stopAfterFirstRotate)
                        return;
                }
            }
            else
            {
                if (stopAfterEvenBalanceFound)
                    return;
            }

            if (Parent != null)
            {
                Parent.Rebalance(true, false);
            }
        }

        private void RotateRL()
        {
            //Check if a T-Tree sliding rotate must be done first.
            if (IsHalfLeaf && Right.IsHalfLeaf && Right.Left.IsLeaf)
            {
                var nodeB = Right;
                var nodeC = Right.Left;

                int maxLen = keys.Length;
                int delta = maxLen - nodeC.ItemCount;

                Array.Copy(nodeB.keys, 0, nodeC.keys, nodeC.ItemCount, delta);
                Array.Copy(nodeB.values, 0, nodeC.values, nodeC.ItemCount, delta);

                Array.Copy(nodeB.keys, delta, nodeB.keys, 0, nodeC.ItemCount);
                Array.Copy(nodeB.values, delta, nodeB.values, 0, nodeC.ItemCount);

                nodeB.ItemCount = nodeC.ItemCount;
                nodeC.ItemCount = maxLen;
            }

            Right.RotateLL();
            RotateRR();
        }

        private void RotateLR()
        {
            //Check if a T-Tree sliding rotate must be done first.
            if (IsHalfLeaf && Left.IsHalfLeaf && Left.Right.IsLeaf)
            {
                var nodeB = Left;
                var nodeC = Left.Right;

                int maxLen = keys.Length;
                int delta = maxLen - nodeC.ItemCount;
                Array.Copy(nodeC.keys, 0, nodeC.keys, delta, nodeC.ItemCount);
                Array.Copy(nodeC.values, 0, nodeC.values, delta, nodeC.ItemCount);

                Array.Copy(nodeB.keys, nodeC.ItemCount, nodeC.keys, 0, delta);
                Array.Copy(nodeB.values, nodeC.ItemCount, nodeC.values, 0, delta);

                nodeB.ItemCount = nodeC.ItemCount;
                nodeC.ItemCount = maxLen;
            }

            Left.RotateRR();
            RotateLL();
        }

        private void RotateRR()
        {
            TTreeNode<TKey, TValue> b = Right;
            TTreeNode<TKey, TValue> c = b.Left;

            if (Parent != null)
            {
                if (Parent.Left == this)
                    Parent.Left = b;
                else
                    Parent.Right = b;
            }

            b.Parent = Parent;
            b.Left = this;
            Parent = b;
            Right = c;

            if (c != null)
            {
                c.Parent = this;
            }

            if (b.Parent == null)
            {
                Root.RootNode = b;
            }

            UpdateHeight(true);
        }

        private void RotateLL()
        {
            TTreeNode<TKey, TValue> left = Left;
            TTreeNode<TKey, TValue> leftsRight = left.Right;

            if (Parent != null)
            {
                if (Parent.Left == this)
                    Parent.Left = left;
                else
                    Parent.Right = left;
            }

            left.Parent = Parent;
            left.Right = this;
            Parent = left;
            Left = leftsRight;

            if (leftsRight != null)
            {
                leftsRight.Parent = this;
            }

            if (left.Parent == null)
            {
                Root.RootNode = left;
            }

            UpdateHeight(true);
        }

        public bool Remove(TKey key)
        {
            TTreeSearchResult<TKey, TValue> searchResult = SearchFor(key);

            if (searchResult == null)
            {
                return false;
            }

            TTreeNode<TKey, TValue> rebalanceFrom = searchResult.Node;

            //If the remove will not cause an underflow then, delete the value and stop
            if (searchResult.Node.ItemCount > searchResult.Node.minimum)
            {
                DeleteFoundValue(searchResult);
                return true;
            }

            if (searchResult.Node.IsInternal)
            {
                //Shift the array to the right "delete"
                // This is faster than calling DeleteFoundValue() because now there is no need to shift
                // the array a second time to make space for the greatest lower bound
                if (searchResult.Index > 0)
                {
                    Array.Copy(searchResult.Node.keys, searchResult.Index - 1, searchResult.Node.keys, searchResult.Index, searchResult.Node.keys.Length - 1 - searchResult.Index);
                    Array.Copy(searchResult.Node.values, searchResult.Index - 1, searchResult.Node.values, searchResult.Index, searchResult.Node.values.Length - 1 - searchResult.Index);
                }

                //Insert the greatest lower bound
                keys[0] = searchResult.Node.Left.CutGreatestLowerBound();
            }
            else  //This is a leaf or half-leaf so just delete the value (leaves and half-leaves are permitted to underflow)
            {
                DeleteFoundValue(searchResult);

                //If this is a half leaf and it can be merged with a leaf, then combine
                if (searchResult.Node.IsHalfLeaf)
                {
                    var child = (searchResult.Node.Left == null) ? searchResult.Node.Right : searchResult.Node.Left;

                    //If all the child items can fit into this node
                    if (searchResult.Node.ItemCount + child.ItemCount <= MaxItems)
                    {
                        //TODO consider not looping - the child is sorted, insert all the items at once, either at the begining or at the end (left/right)
                        for (int i = 0; i < child.ItemCount; ++i)
                        {
                            searchResult.Node.InsertInCurrentNode(child.keys[i], child.values[i], false);
                        }

                        //Remove the child
                        searchResult.Node.Left = searchResult.Node.Right = null;
                        searchResult.Node.m_height = 0;
                    }
                }
                else //Is leaf
                {
                    if (searchResult.Node.ItemCount != 0)
                    {
                        //This is a non-empty leaf. Nothing more to do
                        return true;
                    }
                    else
                    {
                        //The node is empty. So, unles this is the root, remove the node from its parent.
                        if (searchResult.Node.Parent != null)
                        {
                            if (searchResult.Node.Parent.Left == searchResult.Node)
                            {
                                searchResult.Node.Parent.Left = null;
                            }
                            else
                            {
                                searchResult.Node.Parent.Right = null;
                            }

                            //The current node has been deleted, so rebalance from its parent
                            rebalanceFrom = rebalanceFrom.Parent;
                        }
                    }
                }
            }

            rebalanceFrom.Rebalance(false, true);
            return true;
        }

        private TKey CutGreatestLowerBound()
        {
            if (Right == null)
            {
                ItemCount--;

                if (ItemCount == 0)
                {
                    //If there is a left node then the parent should now point to it.
                    if (Parent.Right == this)
                        Parent.Right = Left;
                    else
                        Parent.Left = Left;

                    if (Left != null)
                        Left.Parent = Parent;

                    UpdateHeight(true);
                }

                return keys[ItemCount];
            }
            else
            {
                return Right.CutGreatestLowerBound();
            }
        }

        private void DeleteFoundValue(TTreeSearchResult<TKey, TValue> searchResult)
        {
            Array.Copy(searchResult.Node.keys, searchResult.Index + 1, searchResult.Node.keys, searchResult.Index, searchResult.Node.keys.Length - 1 - searchResult.Index);
            Array.Copy(searchResult.Node.values, searchResult.Index + 1, searchResult.Node.values, searchResult.Index, searchResult.Node.values.Length - 1 - searchResult.Index);
            searchResult.Node.ItemCount--;
        }

        /// <summary>
        /// Search for an item using a custom comparison function
        /// </summary>
        /// <typeparam name="TSearch">The type of the search.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        //public T Search<TSearch>( TSearch item, Func<TSearch, T, int> comparer )
        //{
        //    var result = SearchFor( item, comparer );
        //    return (result == null) ? default( T ) : result.Value;
        //}

        //public SearchResult<TKey, TValue> SearchFor<TSearch>( TSearch item, Func<TSearch, T, int> comparer )
        //{
        //    if( ItemCount == 0 )
        //        return null;

        //    int compare = comparer( item, m_data[ 0 ] );

        //    if( compare == 0 )
        //        return new SearchResult<TKey, TValue>( m_data[ 0 ], this, 0 );

        //    if( compare < 0 )
        //    {
        //        if( Left != null )
        //            return Left.SearchFor( item, comparer );
        //        else
        //            return null;
        //    }

        //    compare = comparer( item, m_data[ ItemCount - 1 ] );

        //    if( compare == 0 )
        //        return new SearchResult<TKey, TValue>( m_data[ ItemCount - 1 ], this, ItemCount - 1 );

        //    if( compare > 0 )
        //    {
        //        if( Right != null )
        //            return Right.SearchFor( item, comparer );
        //        else
        //            return null;
        //    }

        //    int closest = BinarySearch<TSearch>( m_data, 0, ItemCount, item, comparer );

        //    if( closest >= 0 )
        //        return new SearchResult<TKey, TValue>( m_data[ closest ], this, closest );

        //    return null;
        //}

        /// <summary>
        /// Searches for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public TValue Search(TKey key)
        {
            var result = SearchFor(key);
            return (result == null) ? default(TValue) : result.Value;
        }

        public TTreeSearchResult<TKey, TValue> SearchFor(TKey key)
        {
            //This code is not shared with the other Search() method to keep things as fast as possible

            if (ItemCount == 0)
                return null;

            int compare = key.CompareTo(keys[0]);

            if (compare == 0)
                return new TTreeSearchResult<TKey, TValue>(keys[0], values[0], this, 0);

            if (compare < 0)
            {
                if (Left != null)
                    return Left.SearchFor(key);
                else
                    return null;
            }

            compare = key.CompareTo(keys[ItemCount - 1]);

            if (compare == 0)
                return new TTreeSearchResult<TKey, TValue>(keys[ItemCount - 1], values[ItemCount - 1], this, ItemCount - 1);

            if (compare > 0)
            {
                if (Right != null)
                    return Right.SearchFor(key);
                else
                    return null;
            }

            int closest = Array.BinarySearch<TKey>(keys, 0, ItemCount, key);

            if (closest >= 0)
                return new TTreeSearchResult<TKey, TValue>(keys[closest], values[closest], this, closest);

            return null;
        }

        /// <summary>
        /// Copies the items from the current node only
        /// </summary>
        /// <param name="destinationArray">The destination array.</param>
        /// <param name="index">The index.</param>
        public void CopyItems(TKey[] destinationArray, int index)
        {
            keys.CopyTo(destinationArray, index);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            ItemCount = 0;
            Left = Right = null;
            m_height = 0;

            UpdateHeight(true);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(TKey key)
        {
            return (SearchFor(key) != null);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.
        /// -or-
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// -or-
        /// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(TKey[] array, int arrayIndex)
        {
            foreach (TKey key in this)
            {
                array[arrayIndex++] = key;
            }
        }

        //public string ToDot()
        //{
        //    return ToDot(i => i.ToString());
        //}

        //public string ToDot(Func<TKey, string> toString)
        //{
        //    var dot = new StringBuilder();

        //    dot.AppendLine("digraph structs {{");
        //    dot.AppendLine("	node [shape=record, fontsize=9, fontname=Ariel];");
        //    ToDot(dot, toString);
        //    dot.AppendLine("}}");

        //    return dot.ToString();
        //}

        //  private void ToDot(StringBuilder dot, Func<T, string> toString)
        // {
        //dot.AppendFormat("	struct{0} [shape=record, label=\"{{ {{ ", GetHashCode().ToString("X"));

        //for (int i = 0; i < keys.Length; ++i)
        //{
        //    if (i > 0)
        //    {
        //        dot.Append("| ");
        //    }

        //    if (i < ItemCount)
        //    {
        //        dot.AppendFormat("{0}", toString(keys[i]));
        //    }
        //}

        //dot.AppendFormat(" }} | {{ <left> . | <m> h={0}, bf={1} | <right> . }} }}\"];", Height, BalanceFactor).AppendLine();

        //if (Left != null)
        //    Left.ToDot(dot, toString);

        //if (Right != null)
        //    Right.ToDot(dot, toString);

        //if (Left != null)
        //    dot.AppendFormat("	\"struct{0}\":left -> struct{1};", GetHashCode().ToString("X"), Left.GetHashCode().ToString("X")).AppendLine();

        //if (Right != null)
        //    dot.AppendFormat("	\"struct{0}\":right -> struct{1};", GetHashCode().ToString("X"), Right.GetHashCode().ToString("X")).AppendLine();

        //if (Parent != null)
        //    dot.AppendFormat("	struct{0} -> \"struct{1}\":m [style=dotted, color=saddlebrown, arrowsize=0.4];", GetHashCode().ToString("X"), Parent.GetHashCode().ToString("X")).AppendLine();
        //  }

        /// <summary>
        /// Creates a new child node.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private TTreeNode<TKey, TValue> CreateChild(TKey key, TValue value)
        {
            //Create a new child node
            TTreeNode<TKey, TValue> newChild = new TTreeNode<TKey, TValue>(minimum, keys.Length, Root);
            newChild.keys[0] = key;
            newChild.values[0] = value;
            newChild.ItemCount = 1;
            newChild.Parent = this;

            return newChild;
        }

        private void UpdateHeight(bool updateAllUpwards)
        {
            if (ItemCount == 0)
            {
                m_height = -1;
            }
            else
            {
                int lheight = (Left != null) ? (Left.Height) : -1;
                int rheight = (Right != null) ? (Right.Height) : -1;

                m_height = 1 + Math.Max(lheight, rheight);
            }

            if (updateAllUpwards && (Parent != null))
            {
                Parent.UpdateHeight(updateAllUpwards);
            }
        }

        /// <summary>
        /// Binary search implementation using a custom compare function
        /// </summary>
        /// <typeparam name="TSearch">The type of the search.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="length">The length.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        private int BinarySearch<TSearch>(TKey[] array, int index, int length, TSearch value, Func<TSearch, TKey, int> comparer)
        {
            int num1 = index;
            int num2 = (index + length) - 1;

            while (num1 <= num2)
            {
                int num3 = num1 + ((num2 - num1) >> 1);
                int num4 = -comparer(value, array[num3]);

                if (num4 == 0)
                    return num3;

                if (num4 < 0)
                    num1 = num3 + 1;
                else
                    num2 = num3 - 1;
            }

            return ~num1;
        }

        public int BalanceFactor
        {
            get
            {
                if (ItemCount == 0)
                {
                    return 0;
                }
                else
                {
                    int lheight = (Left != null) ? (Left.Height) : -1;
                    int rheight = (Right != null) ? (Right.Height) : -1;

                    return lheight - rheight;
                }
            }
        }

        #region IEnumerable<TKey>

        public IEnumerator<TKey> GetEnumerator()
        {
            if (Left != null)
            {
                foreach (var item in Left)
                    yield return item;
            }

            for (int i = 0; i < ItemCount; ++i)
                yield return keys[i];

            if (Right != null)
            {
                foreach (var item in Right)
                    yield return item;
            }
        }

        public IEnumerable<TValue> Select(TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            foreach (TTreeNode<TKey, TValue> node in InOrderTraversal(this, from, to, fromOpen, toOpen))
            {
                for (int i = 0; i < node.keys.Length; i++)
                {
                    if (((node.keys[i].CompareTo(from) == 0 && fromOpen) || node.keys[i].CompareTo(from) == 1)
                        &&
                        ((node.keys[i].CompareTo(to) == 0 && toOpen) || node.keys[i].CompareTo(to) == -1))
                        yield return node.values[i];
                }
            }
        }

        private static IEnumerable<TTreeNode<TKey, TValue>> InOrderTraversal(TTreeNode<TKey, TValue> node, TKey from, TKey to, bool fromOpen, bool toOpen)
        {
            if (node.MaxKeyInNode.CompareTo(from) >= 0)
                if (node.Left != null)
                {
                    foreach (TTreeNode<TKey, TValue> leftNode in InOrderTraversal(node.Left, from, to, fromOpen, toOpen))
                        yield return leftNode;
                }

            if (node.ItemCount > 0 &&
                ((node.MaxKeyInNode.CompareTo(from) == 0 && fromOpen) || node.MaxKeyInNode.CompareTo(from) == 1) &&
                ((node.MinKeyInNode.CompareTo(to) == 0 && toOpen) || node.MinKeyInNode.CompareTo(to) == -1))
                yield return node;

            if (node.MinKeyInNode.CompareTo(from) <= 0)
                if (node.Right != null)
                {
                    foreach (TTreeNode<TKey, TValue> rightNode in InOrderTraversal(node.Right, from, to, fromOpen, toOpen))
                        yield return rightNode;
                }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                int count = ItemCount;

                if (Right != null)
                    count += Right.Count;

                if (Left != null)
                    count += Left.Count;

                return count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly { get { return false; } }

        public int Height { get { return m_height; } }
        public int ItemCount { get; protected set; }
        public int MaxItems { get { return keys.Length; } }
        public TTreeNode<TKey, TValue> Left { get; internal set; }
        public TTreeNode<TKey, TValue> Right { get; internal set; }
        public TTreeNode<TKey, TValue> Parent { get; internal set; }
        public TTree<TKey, TValue> Root { get { return parentTree; } }
        public bool IsLeaf { get { return (Left == null) && (Right == null); } }
        public bool IsHalfLeaf { get { return !IsLeaf && ((Left == null) || (Right == null)); } }
        public bool IsInternal { get { return (Left != null) && (Right != null); } }
        public TKey MaxKeyInNode
        {
            get
            {
                return this.keys[ItemCount - 1];
            }
        }

        public TKey MinKeyInNode
        {
            get
            {
                return this.keys[0];
            }
        }
    }
}