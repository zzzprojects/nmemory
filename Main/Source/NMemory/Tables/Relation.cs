// -----------------------------------------------------------------------------------
// <copyright file="Relation.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
// -----------------------------------------------------------------------------------

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Exceptions;
    using NMemory.Indexes;

    /// <summary>
    /// Represents a relation between two database tables.
    /// </summary>
    /// <typeparam name="TPrimary">The type of the entities of the referred table.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
    /// <typeparam name="TForeign">The type of the entities of the referring table.</typeparam>
    /// <typeparam name="TForeignKey">The type of the foreign key.</typeparam>
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
            TForeign foreignEntity = (TForeign)foreign;
            TForeignKey foreignKey = this.foreignIndex.KeyInfo.SelectKey(foreignEntity);

            // Empty foreign key means, that it does not refer to anything
            if (this.foreignIndex.KeyInfo.IsEmptyKey(foreignKey))
            {
                return;
            }

            TPrimaryKey primaryKey = this.convertForeignToPrimary.Invoke(foreignKey);

            if (!this.primaryIndex.Contains(primaryKey))
            {
                // TODO: Proper message
                throw new ForeignKeyViolationException();
            }
        }

        IEnumerable<object> IRelation.GetReferringEntities(object primary)
        {
            TPrimary primaryEntity = (TPrimary)primary;
            TPrimaryKey primaryKey = this.primaryIndex.KeyInfo.SelectKey(primaryEntity);

            TForeignKey foreignKey = this.convertPrimaryToForeign.Invoke(primaryKey);

            return this.foreignIndex.Select(foreignKey);
        }

        IEnumerable<object> IRelation.GetReferredEntities(object foreign)
        {
            TForeign foreignEntity = (TForeign)foreign;
            TForeignKey foreignKey = this.foreignIndex.KeyInfo.SelectKey(foreignEntity);

            // Empty key means that there are no referred entity
            if (this.foreignIndex.KeyInfo.IsEmptyKey(foreignKey))
            {
                return Enumerable.Empty<object>();
            }

            TPrimaryKey primaryKey = this.convertForeignToPrimary.Invoke(foreignKey);

            return this.primaryIndex.Select(primaryKey);
        }
    }
}
