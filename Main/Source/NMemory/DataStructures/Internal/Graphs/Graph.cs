using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures.Internal.Graphs
{
    internal class Graph<T>
    {
        private object syncLock = new object();

        /// <summary>
        /// Public ctor
        /// </summary>
        public Graph()
        {
            this.Connections = new Dictionary<T, List<T>>();
        }

        public Dictionary<T, List<T>> Connections { get; private set; }

        /// <summary>
        /// Új pont hozzáadása gráfhou
        /// </summary>
        /// <param name="node">A hozzáadandó pont</param>
        public void AddNode(T node)
        {
			lock(syncLock)
			{
				this.Connections.Add(node, new List<T>());
			}
        }

        public void AddConnection(T from, T end)
        {
			lock(syncLock)
			{
                if (!Connections.ContainsKey(from))
                {
                    AddNode(from);
                }

				this.Connections[from].Add(end);
			}
        }

        public void RemoveConnection(T from, T end)
        {
			lock(syncLock)
			{
				if(Connections.ContainsKey(from))
				{
					this.Connections[from].Remove(end);
				}
			}
        }

		


        public bool HasCycle()
        {
			lock(syncLock)
			{
				foreach(T start in Connections.Keys)
				{
                    if (HasCycleReq(start, new List<T>()))
                    {
                        return true;
                    }
				}

				return false;
			}
        }

        private bool HasCycleReq(T current, List<T> visited)
        {
            if (visited.Contains(current))
            {
                return true;
            }
            else
            {
                visited.Add(current);
            }

            bool hasCycle = false;

            if (Connections.ContainsKey(current))
            {
                foreach (T node in Connections[current])
                {
                    if (HasCycleReq(node, visited))
                    {
                        hasCycle = true;
                    }
                }
            }

            visited.Remove(current);
            return hasCycle;
        }
    }
}
