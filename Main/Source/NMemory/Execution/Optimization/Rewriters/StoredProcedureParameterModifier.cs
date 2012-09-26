using System;
using System.Linq.Expressions;
using NMemory.Common;
using NMemory.StoredProcedures;

namespace NMemory.Execution.Optimization.Rewriters
{
    internal class StoredProcedureParameterModifier : ExpressionRewriterBase
    {
        private TransformationContext context;

        public StoredProcedureParameterModifier(TransformationContext context)
        {
            this.context = context;
        }


        protected override Expression VisitUnary(UnaryExpression node)
        {
            // original:
            //
            //  new Parameter<>("name")
            //
            // transformed:
            //  
            //  executionContext.GetParameter<>("name")
            //

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
            // original:
            //
            //  dbParam (constant)
            //
            // transformed:
            //  
            //  executionContext.GetParameter<>("name")
            //

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
            return 
                Expression.Call(
                    context.Parameter,
                    DatabaseMembers.ExecutionContext_GetParameter.MakeGenericMethod(type),
                    Expression.Constant(name));
        }
    }
}
