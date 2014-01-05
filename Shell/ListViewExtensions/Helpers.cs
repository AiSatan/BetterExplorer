using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BExplorer.Shell
{
	public static class Helpers
	{

		public static T GetParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parent = VisualTreeHelper.GetParent(child);
			if (parent != null)
			{
				if (parent is T)
				{
					return parent as T;
				}

				return GetParent<T>(parent);

			}
			else
			{
				return null;
			}
		}
		public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			lock (bitmap)
			{
				IntPtr hBitmap = bitmap.GetHbitmap();

				try
				{
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
							hBitmap,
							IntPtr.Zero,
							Int32Rect.Empty,
							BitmapSizeOptions.FromEmptyOptions());
				}
				finally
				{
					Gdi32.DeleteObject(hBitmap);
				}
			}
		}
		public static Visual GetDescendantByType(Visual element, Type type)
		{
			if (element == null)
			{
				return null;
			}
			if (element.GetType() == type)
			{
				return element;
			}
			Visual foundElement = null;
			if (element is FrameworkElement)
			{
				(element as FrameworkElement).ApplyTemplate();
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
				foundElement = GetDescendantByType(visual, type);
				if (foundElement != null)
				{
					break;
				}
			}
			return foundElement;
		}
		public static List<T> GetItemAt<T>(this ListView listbox, Rect areaOfInterest)
		{
			var list = new List<T>();
			var rect = new RectangleGeometry(areaOfInterest);
			var hitTestParams = new GeometryHitTestParameters(rect);
			var resultCallback = new HitTestResultCallback(x => HitTestResultBehavior.Continue);
			var filterCallback = new HitTestFilterCallback(x =>
			{
				if (x is ListViewItem)
				{
					var item = (T)((ListViewItem)x).Content;
					list.Add(item);
				}
				return HitTestFilterBehavior.Continue;
			});

			VisualTreeHelper.HitTest(listbox, filterCallback, resultCallback, hitTestParams);
			return list;
		} 
	}
}
