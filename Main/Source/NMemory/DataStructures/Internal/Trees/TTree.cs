using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NMemory.DataStructures.Internal.Trees
{
	/// <summary>
	/// Root of a TTree. This class delegates all calls to the root node of the TTree.
	/// The reason for using it is that as the TTree is modified and balanced the root 
	/// node will change. Without this class you would always have to call 
	/// currentNode.Root.Method(). 
	/// 
	/// Using this class means that you are always working with the root node as 
	/// you should be. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class TTree<TKey, TValue> : ITTreeNode<TKey, TValue>
		where TKey : IComparable
	{
		public TTree( int minimum, int maximum )
		{
			RootNode = new TTreeNode<TKey, TValue>( minimum, maximum, this );
		}

		public TTreeNode<TKey, TValue> RootNode { get; internal set; }

		#region ITreeNode delegating to actual root node
		
        public bool AddItem( TKey key, TValue value )
		{
            return RootNode.AddItem(key, value);
		}

        //public T Search<TSearch>( TSearch item, Func<TSearch, T, int> comparer )
        //{
        //    return RootNode.Search<TSearch>( item, comparer );
        //}

        public TValue Search(TKey key)
        {
            return RootNode.Search(key);
        }

        public TTreeSearchResult<TKey, TValue> SearchFor(TKey key)
        {
            return RootNode.SearchFor(key);
        }

        //public SearchResult<TKey, TValue> SearchFor<TSearch>(TSearch item, Func<TSearch, T, int> comparer)
        //{
        //    return RootNode.SearchFor(item, comparer);
        //}

		public void CopyItems( TKey[] destinationArray, int index )
		{
			RootNode.CopyItems( destinationArray, index );
		}

		//public bool Remove( TKey key )
		//{
			//return RootNode.Remove( key );
		//}

		public void Add( TKey key, TValue value )
		{
            RootNode.Add(key, value);
		}

		public void Clear()
		{
			RootNode.Clear();
		}

        public bool Contains(TKey key)
		{
            return RootNode.Contains(key);
		}

		public void CopyTo( TKey[] array, int arrayIndex )
		{
			RootNode.CopyTo( array, arrayIndex );
		}

        //public string ToDot( Func<TKey, string> toString )
        //{
        //    return RootNode.ToDot( toString );
        //}

        //public string ToDot()
        //{
        //    return RootNode.ToDot();
        //}

        public IEnumerable<TValue> Select(TKey from, TKey to, bool fromOpen, bool toOpen) 
        {
            return RootNode.Select(from, to, fromOpen, toOpen);
        }

   

   
		public TTreeNode<TKey, TValue> Left
		{
			get { return RootNode.Left; }
			internal set { RootNode.Left = value; }
		}

		public TTreeNode<TKey, TValue> Right
		{
			get { return RootNode.Right; }
			internal set { RootNode.Right = value; }
		}

		public TTreeNode<TKey, TValue> Parent
		{
			get { return RootNode.Parent; }
			internal set { RootNode.Parent = value; }
		}

		public int Count { get { return RootNode.Count; } }
		
        public int ItemCount { get { return RootNode.ItemCount; } }
		
        public int Height { get { return RootNode.ItemCount; } }
		
        public bool IsLeaf { get { return RootNode.IsLeaf; } }
		
        public bool IsHalfLeaf { get { return RootNode.IsHalfLeaf; } }
		
        public bool IsInternal { get { return RootNode.IsInternal; } }
		
        public int MaxItems { get { return RootNode.MaxItems; } }
		
        public TTree<TKey, TValue> Root { get { return RootNode.Root; } }
		
        public int BalanceFactor { get { return RootNode.BalanceFactor; } }
		
        public bool IsReadOnly { get { return RootNode.IsReadOnly; } }
		
        #endregion

		#region IEnumerable delegating to root node
		public IEnumerator<TKey> GetEnumerator()
		{
			return RootNode.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return RootNode.GetEnumerator();
		}
		#endregion
	}
}
