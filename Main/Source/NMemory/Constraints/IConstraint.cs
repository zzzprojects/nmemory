using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Constraints
{
    /// <summary>
    /// Defines contraint against database entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IConstraint<TEntity>
    {
        /// <summary>
        /// Apply the contraint on an entity. 
        /// </summary>
        /// <param name="entity">The entity to apply the contraint on.</param>
        void Apply(TEntity entity);
    }
}
