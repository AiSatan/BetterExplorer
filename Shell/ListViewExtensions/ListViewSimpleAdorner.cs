using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BExplorer.Shell
{
	public class ListViewSimpleAdorner : Adorner
	{
		public SolidColorBrush backgroundBrush { get; set; }
		public SolidColorBrush penBrush { get; set; }
		public Pen drawingPen { get; set; }

		// update to this property will automatically trigger underlying OnRender method
		public Rect HighlightArea
		{
			get { return (Rect)GetValue(HighlightAreaProperty); }
			set { SetValue(HighlightAreaProperty, value); }
		}

		public static readonly DependencyProperty HighlightAreaProperty =
			 DependencyProperty.Register("HighlightArea", typeof(Rect),
			 typeof(ListViewSimpleAdorner), new FrameworkPropertyMetadata(new Rect(),
			 FrameworkPropertyMetadataOptions.AffectsRender));

		public ListViewSimpleAdorner(UIElement element)
			: base(element)
		{
			backgroundBrush = new SolidColorBrush(Colors.LightBlue);
			backgroundBrush.Opacity = 0.5;
			penBrush = new SolidColorBrush(Colors.Black);
			penBrush.Opacity = 0.5;
			drawingPen = new Pen(penBrush, 1);
			this.IsHitTestVisible = false;
		}

		protected override void OnRender(DrawingContext dc)
		{
			dc.DrawRectangle(backgroundBrush, drawingPen, HighlightArea);
		}
	}
}
