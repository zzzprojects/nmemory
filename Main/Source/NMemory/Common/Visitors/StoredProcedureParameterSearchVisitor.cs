// ----------------------------------------------------------------------------------
// <copyright file="StoredProcedureParameterSearchVisitor.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
// ----------------------------------------------------------------------------------

namespace NMemory.Common.Visitors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.StoredProcedures;

    public class StoredProcedureParameterSearchVisitor : ExpressionVisitor
    {
        private Dictionary<string, IParameter> parameters;

        public StoredProcedureParameterSearchVisitor()
        {
            this.parameters = new Dictionary<string, IParameter>();
        }

        public static IList<IParameter> FindParameters(Expression expression)
        {
            StoredProcedureParameterSearchVisitor visitor = new StoredProcedureParameterSearchVisitor();
            return visitor.SearchParameters(expression);
        }

        public IList<IParameter> SearchParameters(Expression expression)
        {
            this.Visit(expression);
            return this.parameters.Values.ToList();
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
    }
}
