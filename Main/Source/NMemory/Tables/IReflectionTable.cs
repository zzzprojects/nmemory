
namespace NMemory.Tables
{
    public interface IReflectionTable
    {
        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that represents the property values of the new entity.</param>
        void Insert(object entity);

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="entity">An entity that represents the new property values.</param>
        void Update(object entity);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        void Delete(object entity);
    }
}
