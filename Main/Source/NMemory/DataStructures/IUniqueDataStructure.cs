namespace NMemory.DataStructures
{
    public interface IUniqueDataStructure<TKey, TEntity> : IDataStructure<TKey, TEntity>
    {
        new TEntity Select(TKey key);
    }
}
