using System;
using System.Collections.Generic;

namespace NMemory.DataStructures.Internal.Trees
{
    /// <summary>
    /// Implemented by TTreeNodes and the TTreeRoot
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ITTreeNode<TKey, TValue>:IEnumerable<TKey>// : ICollection<TKey>
        where TKey : IComparable
    {
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if the item was added or false if it already existed and was not </returns>
        bool AddItem(TKey key, TValue value);

        IEnumerable<TValue> Select(TKey from, TKey to, bool fromOpen, bool toOpen);

        ///// <summary>
        ///// Search for an item using a custom comparison function
        ///// </summary>
        ///// <typeparam name="TSearch">The type of the search.</typeparam>
        ///// <param name="item">The item.</param>
        ///// <param name="comparer">The comparer.</param>
        ///// <returns></returns>
        //TValue Search<TSearch>( TSearch item, Func<TSearch, TKey, int> comparer );

        ///// <summary>
        ///// Search for an item using a custom comparison function
        ///// </summary>
        ///// <typeparam name="TSearch">The type of the search.</typeparam>
        ///// <param name="item">The item.</param>
        ///// <param name="comparer">The comparer.</param>
        ///// <returns></returns>
        //SearchResult<TValue> SearchFor<TSearch>( TSearch item, Func<TSearch, T, int> comparer );

        ///// <summary>
        ///// Searches for the specified item.
        ///// </summary>
        ///// <param name="item">The item.</param>
        ///// <returns></returns>
        //SearchResult<TValue> SearchFor( TKey key );

        /// <summary>
        /// Searches for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
      //  TValue Search(TKey key);

        /// <summary>
        /// Copies the items from the current node only
        /// </summary>
        /// <param name="destinationArray">The destination array.</param>
        /// <param name="index">The index.</param>
       // void CopyItems(TKey[] destinationArray, int index);

        /// <summary>
        /// Gets number of items in this node
        /// </summary>
        /// <value>The item count.</value>
        //int ItemCount { get; }

        //string ToDot(Func<TKey, string> toString);
        //string ToDot();

        int Height { get; }
        bool IsLeaf { get; }
        bool IsHalfLeaf { get; }
        bool IsInternal { get; }
        int MaxItems { get; }
        TTree<TKey, TValue> Root { get; }
        TTreeNode<TKey, TValue> Left { get; }
        TTreeNode<TKey, TValue> Right { get; }
        TTreeNode<TKey, TValue> Parent { get; }
        int BalanceFactor { get; }
    }
}
