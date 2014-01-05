using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	public class DemoCustomerProvider : IItemsProvider<ShellObject>
	{
		private readonly int _count;
		private readonly int _fetchDelay;

		/// <summary>
		/// Initializes a new instance of the <see cref="DemoCustomerProvider"/> class.
		/// </summary>
		/// <param name="count">The count.</param>
		/// <param name="fetchDelay">The fetch delay.</param>
		public DemoCustomerProvider(int count, int fetchDelay)
		{
			_count = count;
			_fetchDelay = fetchDelay;
		}

		/// <summary>
		/// Fetches the total number of items available.
		/// </summary>
		/// <returns></returns>
		public int FetchCount()
		{
			//Trace.WriteLine("FetchCount");
			//Thread.Sleep(_fetchDelay);
			return _count;
		}

		/// <summary>
		/// Fetches a range of items.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="count">The number of items to fetch.</param>
		/// <returns></returns>
		public IList<ShellObject> FetchRange(int startIndex, int count)
		{
			//Trace.WriteLine("FetchRange: " + startIndex + "," + count);
			Thread.Sleep(_fetchDelay);

			List<ShellObject> list = new List<ShellObject>();
			for (int i = startIndex; i < startIndex + count; i++)
			{
				ShellObject customer = new ShellObject(null,null,0);
				list.Add(customer);
			}
			return list;
		}
	}

}
