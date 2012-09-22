using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Common;

namespace NMemory.Execution.Optimization.Modifiers
{
    public class PropertyAccessModifier : ExpressionModifierBase
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (!ReflectionHelper.IsNullable(node.Type))
            {
                return base.VisitMember(node);
            }

            return CreateSafeMemberAccessExpression(node, node.Type);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Convert && node.NodeType != ExpressionType.ConvertChecked)
            {
                return base.VisitUnary(node);
            }

            if (node.Operand.NodeType != ExpressionType.MemberAccess)
            {
                return base.VisitUnary(node);
            }

            if (!ReflectionHelper.IsNullable(node.Type))
            {
                return base.VisitUnary(node);
            }

            return CreateSafeMemberAccessExpression(node.Operand as MemberExpression, node.Type);
        }

        private Expression CreateSafeMemberAccessExpression(MemberExpression memberNode, Type resultType)
        {
            Expression[] blockContent = new Expression[3];

            // original:
            //
            //  x.Member
            //
            // transformed:
            //
            //  {
            //      var source = x;
            //      var result;
            //
            //      if (source == null)
            //          result = source.Member;
            //      else
            //          result = null
            //
            //      return result;
            // }

            ParameterExpression source = Expression.Variable(memberNode.Expression.Type);
            ParameterExpression result = Expression.Variable(resultType);

            Expression evaluation =  Expression.PropertyOrField(source, memberNode.Member.Name);

            if (evaluation.Type != resultType)
            {
                evaluation = Expression.ConvertChecked(evaluation, resultType);
            }

            blockContent[0] = Expression.Assign(source, this.Visit(memberNode.Expression));
            blockContent[1] = Expression.IfThenElse(
                Expression.Equal(source, Expression.Constant(null, source.Type)),
                Expression.Assign(result, Expression.Constant(null, resultType)),
                Expression.Assign(result, evaluation));
            blockContent[2] = result;

            return Expression.Block(resultType, new ParameterExpression[] { source, result }, blockContent);
        }
    }
}
