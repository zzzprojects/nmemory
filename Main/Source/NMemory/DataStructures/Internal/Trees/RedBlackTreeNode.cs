using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures.Internal.Trees
{
    internal class RedBlackTreeNode<TKey, TValue>
    {
        private WeakReference parent; 

        public RedBlackTreeNode(TKey key, TValue value, RedBlackTreeNode<TKey, TValue> parent)
        {
            this.parent = new WeakReference(parent);

            this.Key = key;
            this.Value = value;

            this.Red = true;

            this.Left = null;
            this.Right = null;
        }


        public RedBlackTreeNode<TKey, TValue> Left
        {
            get;
            set;
        }

        public RedBlackTreeNode<TKey, TValue> Right
        {
            get;
            set;
        }

        public RedBlackTreeNode<TKey, TValue> Parent
        {
            get
            {
                return this.parent.Target as RedBlackTreeNode<TKey, TValue>;
            }
            set
            {
                this.parent.Target = value;
            }
        }

        public bool Red
        {
            get;
            set;
        }

        public TKey Key
        {
            get;
            set;
        }

        public TValue Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} ({2})", this.Key, this.Value, this.Red ? "R" : "B");
        }
    }
}
