using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BExplorer.Shell
{
	public class IconsView : ViewBase
	{

		private GridViewColumnCollection _columns = new GridViewColumnCollection();
		public GridViewColumnCollection Columns
		{
			get { return _columns; }
		}

		public static readonly DependencyProperty ColumnHeaderContainerStyleProperty =
						GridView.ColumnHeaderContainerStyleProperty.AddOwner
		(typeof(IconsView));
		public Style ColumnHeaderContainerStyle
		{
			get { return (Style)GetValue(ColumnHeaderContainerStyleProperty); }
			set { SetValue(ColumnHeaderContainerStyleProperty, value); }
		}

		public static readonly DependencyProperty ColumnHeaderTemplateProperty =
				GridView.ColumnHeaderTemplateProperty.AddOwner(typeof(IconsView));

		public static readonly DependencyProperty
			ItemContainerStyleProperty =
			ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(IconsView));

		public Style ItemContainerStyle
		{
			get { return (Style)GetValue(ItemContainerStyleProperty); }
			set { SetValue(ItemContainerStyleProperty, value); }
		}

		public static readonly DependencyProperty ItemTemplateProperty =
				ItemsControl.ItemTemplateProperty.AddOwner(typeof(IconsView));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public static readonly DependencyProperty ItemWidthProperty =
				VirtualWrapPanel.ItemWidthProperty.AddOwner(typeof(IconsView));

		public double ItemWidth
		{
			get { return (double)GetValue(ItemWidthProperty); }
			set { SetValue(ItemWidthProperty, value); }
		}


		public static readonly DependencyProperty ItemHeightProperty =
				VirtualWrapPanel.ItemHeightProperty.AddOwner(typeof(IconsView));

		public double ItemHeight
		{
			get { return (double)GetValue(ItemHeightProperty); }
			set { SetValue(ItemHeightProperty, value); }
		}


		protected override object DefaultStyleKey
		{
			get
			{
				return new ComponentResourceKey(GetType(), "IconsView");
			}
		}

	}
}
