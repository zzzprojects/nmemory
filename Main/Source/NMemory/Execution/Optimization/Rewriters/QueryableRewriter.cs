using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Execution.Optimization.Rewriters
{
    /// <summary>
    /// Represents an expression rewriter that replaces Queryable method calls with 
    /// the corresponding Enumerable method calls
    /// </summary>
    public class QueryableRewriter : ExpressionRewriterBase
    {
        private static ILookup<string, MethodInfo> enumerableLookup;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsStatic && node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.GetGenericMethodDefinition() == QueryMethods.AsQueryable)
                {
                    // Skip AsQueryable
                    return this.Visit(node.Arguments[0]);
                }

                IList<Expression> arguments = this.VisitExpressions(node.Arguments);
                Type[] typeArgs = node.Method.IsGenericMethod ? node.Method.GetGenericArguments() : null;
                MethodInfo info = FindEnumerableMethod(node.Method.Name, arguments, typeArgs);

                arguments = this.FixupQuotedArgs(info, arguments);

                return Expression.Call(null, info, arguments);
            }

            return base.VisitMethodCall(node);
        }

        private static MethodInfo FindEnumerableMethod(string name, IList<Expression> args, params Type[] typeArgs)
        {
            if (enumerableLookup == null)
            {
                enumerableLookup = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).ToLookup<MethodInfo, string>(m => m.Name);
            }

            MethodInfo methodInfo = enumerableLookup[name].FirstOrDefault<MethodInfo>(m => ArgsMatch(m, args, typeArgs));
            
            if (methodInfo == null)
            {
                throw new InvalidOperationException();
            }

            if (typeArgs != null)
            {
                return methodInfo.MakeGenericMethod(typeArgs);
            }

            return methodInfo;
        }

        private IList<Expression> FixupQuotedArgs(MethodInfo methodInfo, IList<Expression> arguments)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length > 0)
            {
                List<Expression> sequence = null;

                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression expression = arguments[i];
                    ParameterInfo info = parameters[i];
                    expression = this.FixupQuotedExpression(info.ParameterType, expression);

                    if (sequence == null && expression != arguments[i])
                    {
                        sequence = new List<Expression>(arguments.Count);
                        for (int j = 0; j < i; j++)
                        {
                            sequence.Add(arguments[j]);
                        }
                    }
                    if (sequence != null)
                    {
                        sequence.Add(expression);
                    }
                }

                if (sequence != null)
                {
                    arguments = sequence;
                }
            }

            return arguments;
        }

        private static bool ArgsMatch(MethodInfo methoInfo, IList<Expression> arguments, Type[] typeArgs)
        {
            ParameterInfo[] parameters = methoInfo.GetParameters();

            if (parameters.Length != arguments.Count)
            {
                return false;
            }

            if (!methoInfo.IsGenericMethod && typeArgs != null && typeArgs.Length > 0)
            {
                return false;
            }

            if (!methoInfo.IsGenericMethodDefinition && methoInfo.IsGenericMethod && methoInfo.ContainsGenericParameters)
            {
                methoInfo = methoInfo.GetGenericMethodDefinition();
            }

            if (methoInfo.IsGenericMethodDefinition)
            {
                if (typeArgs == null || typeArgs.Length == 0)
                {
                    return false;
                }

                if (methoInfo.GetGenericArguments().Length != typeArgs.Length)
                {
                    return false;
                }

                methoInfo = methoInfo.MakeGenericMethod(typeArgs);
                parameters = methoInfo.GetParameters();
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                Type parameterType = parameters[i].ParameterType;

                if (parameterType == null)
                {
                    return false;
                }

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                Expression operand = arguments[i];

                if (!parameterType.IsAssignableFrom(operand.Type))
                {
                    if (operand.NodeType == ExpressionType.Quote)
                    {
                        operand = ((UnaryExpression)operand).Operand;
                    }

                    if (!parameterType.IsAssignableFrom(operand.Type) && 
                        !parameterType.IsAssignableFrom(StripExpression(operand.Type)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        private Expression FixupQuotedExpression(Type type, Expression expression)
        {
            Expression operand = expression;

            while (true)
            {
                if (type.IsAssignableFrom(operand.Type))
                {
                    return operand;
                }

                if (operand.NodeType != ExpressionType.Quote)
                {
                    break;
                }

                operand = ((UnaryExpression)operand).Operand;
            }

            if (!type.IsAssignableFrom(operand.Type) && type.IsArray && operand.NodeType == ExpressionType.NewArrayInit)
            {
                Type c = StripExpression(operand.Type);

                if (!type.IsAssignableFrom(c))
                {
                    return expression;
                }

                Type elementType = type.GetElementType();
                NewArrayExpression newExpression = (NewArrayExpression)operand;
                List<Expression> initializers = new List<Expression>(newExpression.Expressions.Count);

                for (int i = 0; i < newExpression.Expressions.Count; i++)
                {
                    initializers.Add(this.FixupQuotedExpression(elementType, newExpression.Expressions[i]));
                }

                expression = Expression.NewArrayInit(elementType, initializers);
            }
            return expression;
        }

        private static Type StripExpression(Type type)
        {
            bool isArray = type.IsArray;
            Type type2 = isArray ? type.GetElementType() : type;
            Type type3 = FindGenericType(typeof(Expression<>), type2);

            if (type3 != null)
            {
                type2 = type3.GetGenericArguments()[0];
            }

            if (!isArray)
            {
                return type;
            }

            int arrayRank = type.GetArrayRank();

            if (arrayRank != 1)
            {
                return type2.MakeArrayType(arrayRank);
            }

            return type2.MakeArrayType();
        }

        private static Type FindGenericType(Type definition, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == definition)
                {
                    return type;
                }

                if (definition.IsInterface)
                {
                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        Type type3 = FindGenericType(definition, interfaceType);

                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                }

                type = type.BaseType;
            }
            return null;
        }
    }
}
