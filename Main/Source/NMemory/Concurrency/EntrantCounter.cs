using System.Collections.Generic;

namespace NMemory.Execution
{
	internal class EntrantCounter
	{
		private Dictionary<long, int> counter;

		public EntrantCounter ()
		{
			this.counter = new Dictionary<long, int>();
		}

		public int Increment(long id)
		{
			int count = 0;
			if (this.counter.TryGetValue(id, out count))
			{
				count++;
			}

			this.counter[id] = count;
			return count;
		}

		public int Decrement(long id)
		{
			int count = 0;
			if (!this.counter.TryGetValue(id, out count))
			{
				return 0;
			}

			if (count > 0)
			{
				count--;
				this.counter[0] = count;
			}

			return count;
		}
	}
}
