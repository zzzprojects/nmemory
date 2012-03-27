using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Indexes;

namespace NMemory.Test.Data
{
    public class TestDatabase : Database
    {
        private Table<Member, string> members;
        private Table<Group, int> groups;

        public TestDatabase()
        {
            this.members = this.CreateTable<Member, string>(m => m.Id);
            this.groups = this.CreateTable<Group, int>(m => m.Id, new IdentitySpecification<Group>(m => m.Id, 1, 1));
        }

        public ITable<Member> Members
        {
            get { return this.members; }
        }

        public ITable<Group> Groups
        {
            get { return this.groups; }
        }

        public void AddGroupNameIndex()
        {
            this.groups.CreateUniqueIndex(new RedBlackTreeIndexFactory<Group>(), g => g.Name);
        }

        public void AddMemberGroupRelation()
        {
            var index = this.members.CreateIndex(new RedBlackTreeIndexFactory<Member>(), m => m.GroupId);

            this.CreateRelation(this.groups.PrimaryKeyIndex, index, x => x.Value, x => x); 
        }
    }
}
