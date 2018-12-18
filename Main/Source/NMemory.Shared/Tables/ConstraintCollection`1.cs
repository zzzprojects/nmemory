// ----------------------------------------------------------------------------------
// <copyright file="ConstraintCollection`1.cs" company="NMemory Team">
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

using System;
using System.Reflection;

namespace NMemory.Tables
{
    using System.Collections.Generic;
    using NMemory.Constraints;
    using NMemory.Execution;

    /// <summary>
    ///     Represents a collection of contraints.
    /// </summary>
    /// <typeparam name="T"> 
    ///     The type of the entity the constraints are applied on. 
    /// </typeparam>
    public class ConstraintCollection<T>
    {
        private readonly List<IConstraint<T>> constraints;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContraintCollection{T}"/> class.
        /// </summary>
        public ConstraintCollection()
        {
            this.constraints = new List<IConstraint<T>>();
        }

        /// <summary>
        ///     Applys all contraints on the specified entity.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="context"> The execution context. </param>
        public void Apply(T entity, IExecutionContext context, ITable table)
        {

	        var _identityEnabled = table.GetType().GetField("identityEnabled",
		        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

	        bool identityEnabled = true;

	        if (_identityEnabled != null)
	        {
		        identityEnabled = (bool)_identityEnabled.GetValue(table);
	        }

			foreach (IConstraint<T> constraint in this.constraints)
            {
				//{NMemory.Constraints.GeneratedGuidConstraint<T>}
	            if (constraint.GetType().FullName.Contains("NMemory.Constraints.GeneratedGuidConstraint") && !identityEnabled)
	            {
		            var _memberName = constraint.GetType().GetProperty("MemberName",
			            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		            string memberName = "";

					if (_memberName != null)
		            {
			            memberName = (string)_memberName.GetValue(constraint, new Object[]{}); 
		            }

		            bool isIndex = false;

		            if (!String.IsNullOrEmpty(memberName))
		            {
			            foreach (var index in table.Indexes)
			            {
				            foreach (var keyMember in index.KeyInfo.EntityKeyMembers)
				            {
					            if (keyMember.Name.Equals(memberName))
					            {
						            isIndex = true;
									break;
					            }

							}

							if (isIndex)
				            break; 
						}
		            }

					if (!isIndex)
		            {
			            constraint.Apply(entity, context);
					}
	            }
	            else
	            {
					constraint.Apply(entity, context);
				}
            }
        }

        /// <summary>
        ///     Adds a table constraint.
        /// </summary>
        /// <param name="constraint">
        ///     The constraint. Note that you must not share this constraint instance across 
        ///     multiple tables.
        /// </param>
        public void Add(IConstraint<T> constraint)
        {
            this.constraints.Add(constraint);
        }

        /// <summary>
        ///     Adds a table constraint.
        /// </summary>
        /// <param name="constraintFactory"> 
        ///     The constraint factory that instantiates a dedicated constraint instance for
        ///     this table.
        ///     </param>
        public void Add(IConstraintFactory<T> constraintFactory)
        {
            IConstraint<T> constraint = constraintFactory.Create();

            this.Add(constraint);
        }
    }
}
