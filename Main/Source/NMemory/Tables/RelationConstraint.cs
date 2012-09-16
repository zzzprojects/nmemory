using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Tables
{
    public class RelationConstraint : IRelationContraint
    {
        public RelationConstraint(MemberInfo primaryField, MemberInfo foreignField)
        {
            this.PrimaryField = primaryField;
            this.ForeignField = foreignField;
        }

        public MemberInfo PrimaryField
        {
            get;
            private set;
        }

        public MemberInfo ForeignField
        {
            get;
            private set;
        }
    }
}
