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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            MyDatabase myDatabase = new MyDatabase();

            myDatabase.Members.Insert(new Member() {Id = "1", Name = "abc"});
            var list = myDatabase.Members.Where(x => x.Name == "Abc").ToList();

        }

        public class MyDatabase : Database
        {
            public MyDatabase()
            {
                var members = this.Tables.Create<Member, string>(x => x.Id, null);
                var groups = base.Tables.Create<Group, int>(g => g.Id, null);

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
}
