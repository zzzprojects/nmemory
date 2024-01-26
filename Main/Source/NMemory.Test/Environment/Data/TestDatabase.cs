// -----------------------------------------------------------------------------------
// <copyright file="TestDatabase.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
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
// -----------------------------------------------------------------------------------

namespace NMemory.Test.Environment.Data
{
    using System;
    using NMemory.Constraints;
    using NMemory.Indexes;
    using NMemory.Tables;
    using NMemory.Utilities;

    public class TestDatabase : Database
    {
        private Table<Member, string> members;
        private Table<Group, int> groups;
        private Table<ReadOnlyGroup, int> roGroups;
        private Table<TimestampEntity, int> timestampEntities;


        public TestDatabase(bool createIdentityForGroup = true, bool createNcharContraintForGroup = false, bool createNotNullConstraint = false)
        {
            var members = this.Tables.Create<Member, string>(x => x.Id, null);
            var groups = this.Tables.Create<Group, int>(x => x.Id, createIdentityForGroup ? new IdentitySpecification<Group>(x => x.Id, 1, 1) : null);
            var roGroups = this.Tables.Create<ReadOnlyGroup, int>(x => x.Id);

            if (createNcharContraintForGroup)
            {
                groups.Contraints.Add(new NCharConstraint<Group>(x => x.Name, 4));
            }

            if (createNotNullConstraint)
            {
                groups.Contraints.Add(new NotNullableConstraint<Group, string>(x => x.Name));
            }

            this.members = members;
            this.groups = groups;
            this.roGroups = roGroups;
        }

        public ITable<Member> Members
        {
            get { return this.members; }
        }

        public ITable<Group> Groups
        {
            get { return this.groups; }
        }

        public ITable<ReadOnlyGroup> ReadOnlyGroups
        {
            get { return this.roGroups; }
        }

        public ITable<TimestampEntity> TimestampEntities
        {
            get 
            {
                if (this.timestampEntities == null)
                {
                    this.timestampEntities = this.Tables.Create<TimestampEntity, int>(x => x.Id, null);
                }

                return this.timestampEntities; 
            }
        }

        public void AddGroupNameIndex()
        {
            this.groups.CreateUniqueIndex(new RedBlackTreeIndexFactory(), g => g.Name);
        }

        public IRelation AddMemberGroupRelation(
            bool createMultiField = false, 
            bool useExpressionFactory = false,
            bool useTuple = false,
            bool cascadedDeletion = false)
        {
            RelationOptions options = new RelationOptions(
                cascadedDeletion: cascadedDeletion);

            if (createMultiField)
            {
                if (useTuple)
                {
                    var uniqueIndex = this.groups.CreateUniqueIndex(
                        new RedBlackTreeIndexFactory(), 
                        g => new Tuple<int, int>(g.Id, g.Id2));

                    var foreignIndex = this.members.CreateIndex(
                        new RedBlackTreeIndexFactory(),
                        m => new Tuple<int?, int>(m.GroupId, m.GroupId2));

                    if (useExpressionFactory)
                    {
                        return this.Tables.CreateRelation(
                            uniqueIndex, 
                            foreignIndex,
                            g => g.Id, m => m.GroupId,
                            g => g.Id2, m => m.GroupId2,
                            options);
                    }
                    else
                    {
                        return this.Tables.CreateRelation(
                            uniqueIndex, 
                            foreignIndex,
                            x => new Tuple<int, int>(x.Item1.Value, x.Item2),
                            x => new Tuple<int?, int>((int?)x.Item1, x.Item2),
                            options);
                    }
                }
                else
                {
                    var uniqueIndex = this.groups.CreateUniqueIndex(
                        new RedBlackTreeIndexFactory(), 
                        g => new { g.Id, g.Id2 });

                    var foreignIndex = this.members.CreateIndex(
                        new RedBlackTreeIndexFactory(), 
                        m => new { m.GroupId, m.GroupId2 });

                    if (useExpressionFactory)
                    {
                        return this.Tables.CreateRelation(
                            uniqueIndex, 
                            foreignIndex,
                            g => g.Id, m => m.GroupId,
                            g => g.Id2, m => m.GroupId2,
                            options);
                    }
                    else
                    {
                        return this.Tables.CreateRelation(
                            uniqueIndex, 
                            foreignIndex,
                            x => new { Id = x.GroupId.Value, Id2 = x.GroupId2 },
                            x => new { GroupId = (int?)x.Id, GroupId2 = x.Id2 }, 
                            options);
                    }
                }
            }
            else
            {
                var foreignIndex = this.members.CreateIndex(
                    new RedBlackTreeIndexFactory(),
                    m => m.GroupId);

                if (useExpressionFactory)
                {
                    return this.Tables.CreateRelation(
                        this.groups.PrimaryKeyIndex, 
                        foreignIndex, 
                        options,
                        new RelationConstraint<Group, Member, int?>(g => g.Id, m => m.GroupId));
                }
                else
                {
                    return this.Tables.CreateRelation(
                        this.groups.PrimaryKeyIndex,
                        foreignIndex,
                        x => x.Value,
                        x => x, 
                        options);
                }
            }

        }


    }
}
