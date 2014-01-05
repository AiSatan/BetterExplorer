using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

namespace BExplorer.Shell
{
	public class Thumbnails : OptimizedPersistable
	{
		public int Id { get; set; }
		public byte[] thumbnail { get; set; }
	}

	public class Person : OptimizedPersistable
	{
		string firstName;
		string lastName;
		UInt16 age;

		public Person(string firstName, string lastName, UInt16 age)
		{
			this.firstName = firstName;
			this.lastName = lastName;
			this.age = age;
		}
	}
}
