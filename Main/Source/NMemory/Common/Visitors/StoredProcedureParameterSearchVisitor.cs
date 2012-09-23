using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.StoredProcedures;

namespace NMemory.Common.Visitors
{
    public class StoredProcedureParameterSearchVisitor : ExpressionVisitor
    {
        public static IList<IParameter> FindParameters(Expression expression)
        {
            StoredProcedureParameterSearchVisitor visitor = new StoredProcedureParameterSearchVisitor();
            return visitor.SearchParameters(expression);
        }

        private Dictionary<string, IParameter> parameters;

        public StoredProcedureParameterSearchVisitor()
        {
            this.parameters = new Dictionary<string, IParameter>();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            IParameter parameter = ExpressionHelper.FindParameter(node);

            if (parameter != null && !this.parameters.ContainsKey(parameter.Name))
            {
                this.parameters.Add(parameter.Name, parameter);
            }

            return base.VisitUnary(node);
        }

        public IList<IParameter> SearchParameters(Expression expression)
        {
            this.Visit(expression);
            return this.parameters.Values.ToList();
        }
    }
}
