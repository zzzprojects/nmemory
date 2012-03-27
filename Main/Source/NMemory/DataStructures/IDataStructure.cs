using System.Collections.Generic;

namespace NMemory.DataStructures
{
    public interface IDataStructure<TKey, TEntity> : IIntervalSearchable<TKey, TEntity>
    {
		IEnumerable<TKey> AllKeys
		{
			get;
		}
        
		IEnumerable<TEntity> Select(TKey value);

        IEnumerable<TEntity> SelectAll();

		bool ContainsKey(TKey key);

		void Clear();


		void Insert(TKey key, TEntity entity);


		void Delete(TKey key, TEntity entity);

        bool SupportsIntervalSearch { get; }

        long Count { get; }
	}
}
