using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NMemory.Tables;

namespace NMemory.Lab
{
    public partial class Form_BaseExample : Form
    {
        public Form_BaseExample()
        {
            InitializeComponent();
        }
    }

    public class MyDatabase : Database
    {
        public MyDatabase()
        {
            var members = base.Tables.Create<Member, int>(p => p.Id);
            var groups = base.Tables.Create<Group, int>(g => g.Id);

            var peopleGroupIdIndex = members.CreateIndex(
                new RedBlackTreeIndexFactory<Member>(),
                p => p.GroupId);

            this.Tables.CreateRelation(
                groups.PrimaryKeyIndex,
                peopleGroupIdIndex,
                x => x,
                x => x);

            this.Members = members;
            this.Groups = groups;
        }

        public ITable<Member> Members { get; private set; }

        public ITable<Group> Groups { get; private set; }
    }

    public class Member
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int? GroupId { get; set; }

        public int GroupId2 { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }

        public int Id2 { get; set; }

        public string Name { get; set; }

    }
}
