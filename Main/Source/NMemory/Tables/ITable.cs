using System;
using System.Collections.Generic;
using NMemory.Indexes;
using NMemory.Statistics;
using System.Linq;

namespace NMemory.Tables
{
    /// <summary>
    /// Defines an interface for a database tables.
    /// </summary>
	public interface ITable : IQueryable
	{
        /// <summary>
        /// Gets the database that contains the table.
        /// </summary>
        Database Database { get; }

        /// <summary>
        /// Gets the type of the entities contained in the table.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        IIndex PrimaryKeyIndex { get; }
        
        /// <summary>
        /// Gets the indexes of the table.
        /// </summary>
		IEnumerable<IIndex> Indexes { get; }

        /// <summary>
        /// Occurs when the indexes of the table are changed.
        /// </summary>
		event EventHandler IndexChanged;

        IStatistics Statistics { get; }

        /// <summary>
        /// Gets the number of entities contained in the table.
        /// </summary>
        long Count { get; } 
	}
}
