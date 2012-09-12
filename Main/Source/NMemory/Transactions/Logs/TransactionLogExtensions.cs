using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using NMemory.Tables;

namespace NMemory.Transactions.Logs
{
    internal static class TransactionLogExtensions
    {
        public static void WriteIndexInsert<TEntity>(this ITransactionLog log, IIndex<TEntity> index, TEntity entity)
            where TEntity : class
        {
            log.Write(new IndexInsertTransactionLogItem<TEntity>(index, entity));
        }

        public static void WriteIndexDelete<TEntity>(this ITransactionLog log, IIndex<TEntity> index, TEntity entity)
            where TEntity : class
        {
            log.Write(new IndexDeleteTransactionLogItem<TEntity>(index, entity));
        }

        public static void WriteEntityUpdate<TEntity>(this ITransactionLog log, EntityPropertyCloner<TEntity> propertyCloner, TEntity storedEntity, TEntity oldEntity)
            where TEntity : class
        {
            log.Write(new UpdateEntityLogItem<TEntity>(propertyCloner, storedEntity, oldEntity));
        }
    }
}
