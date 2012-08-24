using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace NMemory.Tables
{
    public class RelationConstraint<TPrimary, TForeign> : IRelationContraint
    {
        public RelationConstraint(Expression<Func<TPrimary, object>> primaryField, Expression<Func<TForeign, object>> foreignField)
        {
            if (primaryField == null)
	        {
                throw new ArgumentNullException("primaryField");
	        }

            if (foreignField == null)
            {
                throw new ArgumentNullException("foreignField");
            }

            MemberExpression primaryMember = primaryField.Body as MemberExpression;
            
            if (primaryMember == null)
	        {
                throw new ArgumentException("", "primaryField");
	        }

            this.PrimaryField = primaryMember.Member as PropertyInfo;

            if (this.PrimaryField == null)
            {
                throw new ArgumentException("", "primaryField");
            }


            MemberExpression foreignMember = primaryField.Body as MemberExpression;

            if (foreignMember == null)
            {
                throw new ArgumentException("", "foreignField");
            }

            this.ForeignField = foreignMember.Member as PropertyInfo;

            if (this.ForeignField == null)
            {
                throw new ArgumentException("", "foreignField");
            }
        }

        public PropertyInfo PrimaryField
        {
            get;
            private set;
        }

        public PropertyInfo ForeignField
        {
            get;
            private set;
        }
    }
}
