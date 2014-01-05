﻿using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BExplorer.Shell
{
	/// <summary>
	/// Class containing definition for columns
	/// </summary>
	[Serializable]
	public class Collumns : GridViewColumn
	{
		public PROPERTYKEY pkey { get; set; }
		public string Name { get; set; }
		public int Width { get; set; }
		public Boolean IsColumnHandler { get; set; }

		public Type CollumnType { get; set; }
	}
}
