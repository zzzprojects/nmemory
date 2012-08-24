using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Indexes;
using NMemory.Constraints;

namespace NMemory.Test.Data
{
    public class TestDatabase : Database
    {
        private Table<Member, string> members;
        private Table<Group, int> groups;
        private Table<TimestampEntity, int> timestampEntities;


        public TestDatabase(bool createIdentityForGroup = true, bool createNcharContraintForGroup = false)
        {
            var members = this.Tables.Create<Member, string>(x => x.Id);
            var groups = this.Tables.Create<Group, int>(x => x.Id, createIdentityForGroup ? new IdentitySpecification<Group>(x => x.Id, 1, 1) : null);

            if (createNcharContraintForGroup)
            {
                groups.AddConstraint(new NCharConstraint<Group>(x => x.Name, 4));
            }

            this.members = members;
            this.groups = groups;
        }

        public ITable<Member> Members
        {
            get { return this.members; }
        }

        public ITable<Group> Groups
        {
            get { return this.groups; }
        }

        public ITable<TimestampEntity> TimestampEntities
        {
            get 
            {
                if (this.timestampEntities == null)
                {
                    this.timestampEntities = this.Tables.Create<TimestampEntity, int>(x => x.Id);
                }

                return this.timestampEntities; 
            }
        }

        public void AddGroupNameIndex()
        {
            this.groups.CreateUniqueIndex(new RedBlackTreeIndexFactory<Group>(), g => g.Name);
        }

        public void AddMemberGroupRelation(bool createMultiField = false)
        {
            if (createMultiField)
            {
                var uniqueIndex = this.groups.CreateUniqueIndex(new RedBlackTreeIndexFactory<Group>(), g => new { g.Id, g.Id2 });
                var foreignIndex = this.members.CreateIndex(new RedBlackTreeIndexFactory<Member>(), m => new { m.GroupId, m.GroupId2 }); 

                this.Tables.CreateRelation(uniqueIndex, foreignIndex, 
                    x => new { Id = x.GroupId.Value, Id2 = x.GroupId2 }, 
                    x => new { GroupId = (int?)x.Id, GroupId2 = x.Id2  });
            }
            else
            {
                var foreignIndex = this.members.CreateIndex(new RedBlackTreeIndexFactory<Member>(), m => m.GroupId);

                this.Tables.CreateRelation(this.groups.PrimaryKeyIndex, foreignIndex, x => x.Value, x => x); 
            }


        }
    }
}
