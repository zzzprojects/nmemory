using System;
using System.Collections.Generic;
using System.Linq;
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
            MethodInfo method = node.Method;

            // First remove all the AsQueryable method calls
            // Check if its is a AsQueryable call
            if (method.IsGenericMethod &&
                method.GetGenericMethodDefinition() == QueryMethods.AsQueryable)
            {
                // Skip AsQueryable
                return this.Visit(node.Arguments[0]);
            }

            Expression instance = this.Visit(node.Object);
            IList<Expression> arguments = this.VisitExpressions(node.Arguments);

            // Check if everything is unchanged
            // It is possible that an AsQueryable method call was removed
            if (node.Object == instance && node.Arguments == arguments)
            {
                //No changes was made, return the original expression
                return node;
            }

            MethodInfo methodInfo = null;

            // Check if it is a Queryable extension method
            if (method.IsStatic && method.DeclaringType == typeof(Queryable))
            {
                // Seacrch for the corresponding Enumerable extension method
                Type[] genericArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
                methodInfo = FindEnumerableMethod(method.Name, arguments, genericArguments);
            }

            // If no method was found, use the previous
            if (methodInfo == null)
            {
                methodInfo = node.Method;
            }

            arguments = FixupArguments(methodInfo, arguments);

            return Expression.Call(instance, methodInfo, arguments);
        }

        private static MethodInfo FindEnumerableMethod(string name, IList<Expression> arguments, params Type[] genericArguments)
        {
            if (enumerableLookup == null)
            {
                enumerableLookup = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).ToLookup<MethodInfo, string>(m => m.Name);
            }

            MethodInfo methodInfo = enumerableLookup[name].FirstOrDefault<MethodInfo>(m => ArgsMatch(m, arguments, genericArguments));
            
            if (methodInfo != null && genericArguments != null)
            {
                methodInfo = methodInfo.MakeGenericMethod(genericArguments);
            }

            return methodInfo;
        }

        
        private IList<Expression> FixupArguments(MethodInfo methodInfo, IList<Expression> arguments)
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

        private static bool ArgsMatch(MethodInfo methodInfo, IList<Expression> arguments, Type[] typeArgs)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length != arguments.Count)
            {
                return false;
            }

            if (!methodInfo.IsGenericMethod && typeArgs != null && typeArgs.Length > 0)
            {
                return false;
            }

            if (!methodInfo.IsGenericMethodDefinition && methodInfo.IsGenericMethod && methodInfo.ContainsGenericParameters)
            {
                methodInfo = methodInfo.GetGenericMethodDefinition();
            }

            if (methodInfo.IsGenericMethodDefinition)
            {
                if (typeArgs == null || typeArgs.Length == 0)
                {
                    return false;
                }

                if (methodInfo.GetGenericArguments().Length != typeArgs.Length)
                {
                    return false;
                }

                methodInfo = methodInfo.MakeGenericMethod(typeArgs);
                parameters = methodInfo.GetParameters();
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

            Type[] argumentGenericArguments = expression.Type.IsGenericType ? expression.Type.GetGenericArguments() : null;

            // Add AsQueryable method call if needed
            // Check if the method parameter is Queryable<>
            // Check if the argument is Enumerable<>
            if (!type.IsAssignableFrom(operand.Type) && 
                operand.Type.IsAssignableFrom(type) &&
                type.IsGenericType &&
                typeof(IQueryable<>) == type.GetGenericTypeDefinition() &&
                ReflectionHelper.IsGenericEnumerable(expression.Type))
            {
                Type genericArg = type.GetGenericArguments()[0];
                MethodInfo asQueryableMethod = QueryMethods.AsQueryable.MakeGenericMethod(genericArg);

                // Add AsQueryable
                return Expression.Call(null, asQueryableMethod, expression);
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
