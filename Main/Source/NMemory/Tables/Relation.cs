using System;
using NMemory.Indexes;
using System.Collections.Generic;
using NMemory.Exceptions;

namespace NMemory.Tables
{
	public class Relation<
        TPrimary, 
        TPrimaryKey, 
        TForeign, 
        TForeignKey> :

        IRelation

        where TPrimary : class
        where TForeign : class
	{

		#region Members

        private IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex;
        private IIndex<TForeign, TForeignKey> foreignIndex;

		private Func<TForeignKey, TPrimaryKey> convertForeignToPrimary;
		private Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign;

		#endregion

		#region Ctor

		internal Relation(
            IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
            IIndex<TForeign, TForeignKey> foreignIndex,
            Func<TForeignKey, TPrimaryKey>  foreignToPrimary,
            Func<TPrimaryKey, TForeignKey> primaryToForeign)
		{
			this.primaryIndex = primaryIndex;
			this.foreignIndex = foreignIndex;

            this.convertForeignToPrimary = foreignToPrimary;
            this.convertPrimaryToForeign = primaryToForeign;
        }

        #endregion

        ITable IRelation.PrimaryTable
        {
            get { return this.primaryIndex.Table; }
        }

        ITable IRelation.ForeignTable
        {
            get { return this.foreignIndex.Table; }
        }

        IIndex IRelation.PrimaryIndex
        {
            get { return this.primaryIndex; }
        }

        IIndex IRelation.ForeignIndex
        {
            get { return this.foreignIndex; }
        }


        void IRelation.ValidateEntity(object foreign)
        {
            TForeign entity = (TForeign)foreign;
            TForeignKey key = this.foreignIndex.KeyInfo.GetKey(entity);

            if (!this.primaryIndex.Contains(this.convertForeignToPrimary(key)))
            {
                // TODO: Proper message
                throw new ForeignKeyViolationException();
            }
        }


        IEnumerable<object> IRelation.GetReferringEntities(object primary)
        {
            TPrimary entity = (TPrimary)primary;
            TPrimaryKey key = this.primaryIndex.KeyInfo.GetKey(entity);

            return this.foreignIndex.Select(this.convertPrimaryToForeign.Invoke(key));
        }

        IEnumerable<object> IRelation.GetReferredEntities(object foreign)
        {
            TForeign entity = (TForeign)foreign;
            TForeignKey key = this.foreignIndex.KeyInfo.GetKey(entity);

            return this.primaryIndex.Select(this.convertForeignToPrimary.Invoke(key));
        }
    }
}
