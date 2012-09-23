using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Tables
{
    public class RelationConstraint<TPrimary, TForeign, TField> : IRelationContraint
    {
        public RelationConstraint(Expression<Func<TPrimary, TField>> primaryField, Expression<Func<TForeign, TField>> foreignField)
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
                UnaryExpression convertExpression = primaryField.Body as UnaryExpression;

                if (convertExpression.NodeType == ExpressionType.Convert)
                {
                    primaryMember = convertExpression.Operand as MemberExpression;
                }
	        }

            if (primaryMember == null)
            {
                throw new ArgumentException("", "primaryField");
            }

            this.PrimaryField = primaryMember.Member;

            if (this.PrimaryField == null)
            {
                throw new ArgumentException("", "primaryField");
            }

            MemberExpression foreignMember = foreignField.Body as MemberExpression;

            if (foreignMember == null)
            {
                UnaryExpression convertExpression = foreignField.Body as UnaryExpression;

                if (convertExpression.NodeType == ExpressionType.Convert)
                {
                    foreignMember = convertExpression.Operand as MemberExpression;
                }
            }

            if (foreignMember == null)
            {
                throw new ArgumentException("", "foreignField");
            }

            this.ForeignField = foreignMember.Member;

            if (this.ForeignField == null)
            {
                throw new ArgumentException("", "foreignField");
            }
        }

        public MemberInfo PrimaryField
        {
            get;
            private set;
        }

        public MemberInfo ForeignField
        {
            get;
            private set;
        }
    }
}
