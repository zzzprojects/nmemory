using System;
using NMemory.Indexes;

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

        public ITable PrimaryTable
        {
            get { return this.primaryIndex.Table; }
        }

        public ITable ForeignTable
        {
            get { return this.foreignIndex.Table; }
        }

        public IIndex PrimaryIndex
        {
            get { return this.primaryIndex; }
        }

        public IIndex ForeignIndex
        {
            get { return this.foreignIndex; }
        }
    }
}
