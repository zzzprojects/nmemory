using System;
using System.Linq.Expressions;

namespace NMemory.Tables
{
    public class IdentitySpecification<TEntity>
    {
        public IdentitySpecification(
            Expression<Func<TEntity, long>> identityColumn, 
            int seed = 1, 
            int increment = 1)
        {
            this.IdentityColumn = identityColumn;
            this.Seed = seed;
            this.Increment = increment;
        }

        public int Seed 
        { 
            get;
            private set; 
        }

        public int Increment 
        { 
            get; 
            private set; 
        }

        public Expression<Func<TEntity, long>> IdentityColumn 
        { 
            get; 
            private set; 
        }

    }
}
