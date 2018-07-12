// ----------------------------------------------------------------------------------
// <copyright file="StoredProcedureParameterRewriter.cs" company="NMemory Team">
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

namespace NMemory.Execution.Optimization.Rewriters
{
    using System;
    using System.Linq.Expressions;
    using NMemory.Common;
    using NMemory.StoredProcedures;

    internal class StoredProcedureParameterRewriter : ExpressionRewriterBase
    {
        private ITransformationContext context;

        public StoredProcedureParameterRewriter(ITransformationContext context)
        {
            this.context = context;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            //// ----------------------------------------------
            //// original:
            ////  new Parameter<>("name")
            //// ----------------------------------------------
            //// transformed:
            ////  executionContext.GetParameter<>("name")
            //// ----------------------------------------------

            IParameter parameter = ExpressionHelper.FindParameter(node);

            if (parameter == null)
            {
                return base.VisitUnary(node);
            }

            // Replace the paramerer
            return this.CreateParameterReader(parameter.Type, parameter.Name);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            //// ----------------------------------------------
            //// original:
            ////  dbParam (constant)
            //// ----------------------------------------------
            //// transformed:
            ////  executionContext.GetParameter<>("name")
            //// ----------------------------------------------

            IParameter parameter = node.Value as IParameter;

            if (parameter == null)
            {
                return base.VisitConstant(node);
            }
 
            // Replace the parameter
            return this.CreateParameterReader(parameter.Type, parameter.Name);
        }

        private Expression CreateParameterReader(Type type, string name)
        {
            return 
                Expression.Call(
                    this.context.Parameter,
                    DatabaseMembers.ExecutionContext_GetParameter.MakeGenericMethod(type),
                    Expression.Constant(name, typeof(string)));
        }
    }
}
