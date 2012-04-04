using System.Linq.Expressions;
using System.Reflection;
using NMemory.StoredProcedures;
using System;
using NMemory.Common;

namespace NMemory.Execution.Optimization.Modifiers
{
    internal class StoredProcedureParameterModifier : ExpressionModifierBase
    {
        private ParameterExpression executionContext;

        public StoredProcedureParameterModifier(ParameterExpression executionContext)
        {
            this.executionContext = executionContext;
        }


        protected override Expression VisitUnary(UnaryExpression node)
        {
            IParameter parameter = ExpressionHelper.FindParameter(node);

            if (parameter == null)
            {
                return base.VisitUnary(node);
            }

            // Replace the paramerer
            return CreateParameterReader(parameter.Type, parameter.Name);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            IParameter parameter = node.Value as IParameter;

            if (parameter == null)
            {
                return base.VisitConstant(node);
            }
 
            // Replace the parameter
            return CreateParameterReader(parameter.Type, parameter.Name);
        }

        private Expression CreateParameterReader(Type type, string name)
        {
            MethodInfo method = typeof(IExecutionContext).GetMethod("GetParameter");

            return 
                Expression.Call(
                    executionContext,
                    method.MakeGenericMethod(type),
                    Expression.Constant(name));
        }
    }
}
