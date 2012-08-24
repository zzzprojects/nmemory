using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Tables
{
    public class RelationConstraint : IRelationContraint
    {
        public RelationConstraint(PropertyInfo primaryField, PropertyInfo foreignField)
        {
            this.PrimaryField = primaryField;
            this.ForeignField = foreignField;
        }

        public PropertyInfo PrimaryField
        {
            get;
            private set;
        }

        public PropertyInfo ForeignField
        {
            get;
            private set;
        }
    }
}
