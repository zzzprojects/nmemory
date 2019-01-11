using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NMemory.Indexes;
using NMemory.Tables;

namespace NMemory.Lab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            MyDatabase myDatabase = new MyDatabase();
			
	        myDatabase.Groups.Insert(new Group() { Id = 1});

			myDatabase.Members.Insert(new Member() { Id = 1, GroupId = 1, Name = "It's Member_1" });

	        myDatabase.Members.Insert(new Member() { Id = 2, Name = "It's Member_2" });

	        myDatabase.Groups.Delete(new Group { Id = 1 });

			var list = myDatabase.Members.Where(x => Functions.Like(x.Name, "%Member_2%")).ToList();

	        var list2 = myDatabase.Members.ToList();  
		}

        public class MyDatabase : Database
        {
            public MyDatabase()
            {
                var members = this.Tables.Create<Member, int>(x => x.Id);
                var groups = base.Tables.Create<Group, int>(g => g.Id);

                this.Members = members;
                this.Groups = groups;

	            RelationOptions options = new RelationOptions(
		            cascadedDeletion: true);

				var peopleGroupIdIndex = members.CreateIndex(
		            new RedBlackTreeIndexFactory(),
		            p => p.GroupId);

	            this.Tables.CreateRelation(
		            groups.PrimaryKeyIndex,
		            peopleGroupIdIndex,
		            x =>  x ?? -1,
		            x => x,
		            options);
			}

            public ITable<Member> Members { get; private set; }

            public ITable<Group> Groups { get; private set; }
        }

        public class Member
        {
            public int Id { get; set; }

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
