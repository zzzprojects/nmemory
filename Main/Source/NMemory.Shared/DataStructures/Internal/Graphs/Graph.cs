// ----------------------------------------------------------------------------------
// <copyright file="Graph.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.DataStructures.Internal.Graphs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class Graph<T>
    {
        private object syncLock = new object();

        public Graph()
        {
            this.Connections = new Dictionary<T, List<T>>();
        }

        public Dictionary<T, List<T>> Connections { get; private set; }

        public void AddNode(T node)
        {
            lock (this.syncLock)
            {
                this.Connections.Add(node, new List<T>());
            }
        }

        public void AddConnection(T from, T end)
        {
            lock (this.syncLock)
            {
                if (!this.Connections.ContainsKey(from))
                {
                    this.AddNode(from);
                }

                this.Connections[from].Add(end);
            }
        }

        public void RemoveConnection(T from, T end)
        {
            lock (this.syncLock)
            {
                if (this.Connections.ContainsKey(from))
                {
                    this.Connections[from].Remove(end);
                }
            }
        }

        public bool HasCycle()
        {
            lock (this.syncLock)
            {
                foreach (T start in this.Connections.Keys)
                {
                    if (this.HasCycleReq(start, new List<T>()))
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

            if (this.Connections.ContainsKey(current))
            {
                foreach (T node in this.Connections[current])
                {
                    if (this.HasCycleReq(node, visited))
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
