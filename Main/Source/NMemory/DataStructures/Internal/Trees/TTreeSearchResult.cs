using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures.Internal.Trees
{
	internal class TTreeSearchResult<TKey, TValue>
		where TKey : IComparable
	{
		public TTreeSearchResult( TKey key, TValue value, TTreeNode<TKey, TValue> node, int index )
		{
			Value = value;
			Node = node;
			Index = index;
		}

		public TValue Value { get; private set; }
        public TKey Key { get; private set; }
		public TTreeNode<TKey, TValue> Node { get; private set; }
		public int Index { get; private set; }
	}
}
