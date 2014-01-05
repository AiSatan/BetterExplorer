using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BExplorer.Shell
{
	/// <summary>
	/// Interaction logic for IShellViewEx.xaml
	/// </summary>
	public partial class IShellViewEx : System.Windows.Controls.UserControl, INotifyPropertyChanged
	{
		public ObservableCollection<ShellObject> Items { get; set; }
		public ImageList jumbo = new ImageList(ImageListSize.Jumbo);
		public static BitmapSource ApplicationFallback;
		public int LastSortedColumnIndex { get; set; }

		public int ItemWidth
		{
			get
			{
				return this.IconSize > 80 ? this.IconSize + 10 : this.IconSize + 27;
			}
		}
		public int ItemHeight
		{
			get
			{
				return this.IconSize + 40;
			}
		}

		public int IconSize { get; set; }
		public SortOrder LastSortOrder { get; set; }
		ListViewSimpleAdorner myAdorner { get; set; }

		public ShellItem CurrentFolder { get; set; }

		public Boolean ShowCheckboxes { get; set; }

		private ShellViewStyle _view;
		public ShellViewStyle View { 
			get 
			{
				return _view;
			} 
			set
			{
				switch (value)
				{
					case ShellViewStyle.ExtraLargeIcon:
						ResizeIcons(256);
						this.lv.View = this.lv.FindResource("iconView") as ViewBase;
						break;
					case ShellViewStyle.LargeIcon:
						ResizeIcons(96);
						this.lv.View = this.lv.FindResource("iconView") as ViewBase;
						break;
					case ShellViewStyle.Medium:
						ResizeIcons(48);
						this.lv.View = this.lv.FindResource("iconView") as ViewBase;

						break;
					case ShellViewStyle.SmallIcon:
						ResizeIcons(16);
						this.lv.View = this.lv.FindResource("smallView") as ViewBase;
						break;
					case ShellViewStyle.List:
						break;
					case ShellViewStyle.Details:
						ResizeIcons(16);
						this.lv.View = this.lv.FindResource("detailsView") as GridView;
						break;
					case ShellViewStyle.Thumbnail:
						break;
					case ShellViewStyle.Tile:
						ResizeIcons(48);
						this.lv.View = this.lv.FindResource("tilesView") as ViewBase;
						break;
					case ShellViewStyle.Thumbstrip:
						break;
					case ShellViewStyle.Content:
						break;
					default:
						break;
				}
			  _view = value;
				OnViewChanged(new ViewChangedEventArgs(value, this.IconSize));
			} 
		}

		public List<ShellItem> SelectedItems
		{
			get
			{
				return this.lv.SelectedItems.OfType<ShellObject>().Select(s => s.item).ToList();
			}
		}
		public List<Collumns> AllAvailableColumns = new List<Collumns>();
		/// <summary>
		/// Occurs when the control gains focus
		/// </summary>
		public event EventHandler GotFocus;

		/// <summary>
		/// Occurs when the control loses focus
		/// </summary>
		public event EventHandler LostFocus;

		/// <summary>
		/// Occurs when the <see cref="ShellView"/> control navigates to a 
		/// new folder.
		/// </summary>
		public event EventHandler<NavigatedEventArgs> Navigated;


		/// <summary>
		/// Occurs when the <see cref="ShellView"/>'s current selection 
		/// changes.
		/// </summary>
		/// 
		/// <remarks>
		/// <b>Important:</b> When <see cref="ShowWebView"/> is set to 
		/// <see langref="true"/>, this event will not occur. This is due to 
		/// a limitation in the underlying windows control.
		/// </remarks>
		public event EventHandler SelectionChanged;

		public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

		public List<Collumns> Collumns = new List<Collumns>();
		Point? myDragStartPoint { get; set; }
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		CancellationToken token;
		public IShellViewEx()
		{
			InitializeComponent();
			token = tokenSource.Token;
			this.Loaded += IShellViewEx_Loaded;
			this.Items = new ObservableCollection<ShellObject>();
			DataContext = this;
			m_History = new ShellHistory();
				Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
				defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
				Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
				var hbitmap = jumbo.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
				ApplicationFallback = Helpers.CreateBitmapSourceFromBitmap(hbitmap);
		}
		public int GetItemsCount()
		{
			return this.Items.Count;
		}

		public int GetSelectedCount()
		{
			return this.lv.SelectedItems.Count;
		}

		public void SelectAll()
		{
			this.lv.SelectAll();
		}

		public void DeSelectAllItems()
		{
			this.lv.UnselectAll();
		}

		public void InvertSelection()
		{

		}

		ShellHistory m_History;
		ShellItem m_CurrentFolder;
		ShellViewStyle m_View;
		/// <summary>
		/// Gets a value indicating whether a previous page in navigation 
		/// history is available, which allows the <see cref="NavigateBack"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateBack
		{
			get { return m_History.CanNavigateBack; }
		}

		/// <summary>
		/// Gets a value indicating whether a subsequent page in navigation 
		/// history is available, which allows the <see cref="NavigateForward"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateForward
		{
			get { return m_History.CanNavigateForward; }
		}

		/// <summary>
		/// Gets a value indicating whether the folder currently being browsed
		/// by the <see cref="ShellView"/> has parent folder which can be
		/// navigated to by calling <see cref="NavigateParent"/>.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateParent
		{
			get
			{
				return this.CurrentFolder != ShellItem.Desktop;
			}
		}

		public void SetSortCollumn(int colIndex, SortOrder Order)
		{

		}
		public void RefreshContents()
		{
			Navigate(this.CurrentFolder);
		}
		public void Navigate(ShellItem destination)
		{
			this.Items.Clear();
			
			
			//lv.SelectedIndex = 0;
			//Task.Run(() =>
			//{

			try
			{
				Dispatcher.BeginInvoke(DispatcherPriority.Background,
					(ThreadStart)(() =>
					{

						//var items = (selectedItem).OrderByDescending(o => o.IsFolder).Select(s => new ShellItem(s) { token = token, isCancel = running }).ToList();
						int i = -1;
						foreach (ShellItem item in destination.OrderByDescending(o => o.IsFolder))
						{
							var newItem = new ShellObject(item, this, i++) { token = token, isCancel = false };
							Items.Add(newItem);
						}
						//items.ForEach(x => this.Items.Add(x));
					}));
				//});

				GC.WaitForPendingFinalizers();
				GC.Collect();
				Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

				// Get scrollviewer
				var scrollViewer = Helpers.GetDescendantByType(lv, typeof(ScrollViewer)) as ScrollViewer;
				if (null != scrollViewer)
				{
					scrollViewer.UpdateLayout();
					scrollViewer.ScrollToTop();
				}
				this.CurrentFolder = destination;
				this.OnNavigated(new NavigatedEventArgs(destination));
			}
			catch (Exception)
			{
				
				throw;
			}
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the previous folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a <b>Back</b> button similar to the one in 
		/// Windows Explorer, which will allow your users to return to a 
		/// previous folder in the navigation history. 
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateBack"/> property to determine whether 
		/// the navigation history is available and contains a previous page. 
		/// This property is useful, for example, to change the enabled state 
		/// of a Back button when the ShellView control navigates to or leaves 
		/// the beginning of the navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate backwards through.
		/// </exception>
		public void NavigateBack()
		{
			this.CurrentFolder = m_History.MoveBack();
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control backwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a drop-down menu on a <b>Back</b> button similar 
		/// to the one in Windows Explorer, which will allow your users to return 
		/// to a previous folder in the navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'back' history.
		/// </exception>
		public void NavigateBack(ShellItem folder)
		{
			m_History.MoveBack(folder);
			this.CurrentFolder = folder;
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the next folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateForward"/> 
		/// method to implement a <b>Forward</b> button similar to the one 
		/// in Windows Explorer, allowing your users to return to the next 
		/// folder in the navigation history after navigating backward.
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateForward"/> property to determine 
		/// whether the navigation history is available and contains a folder 
		/// located after the current one.  This property is useful, for 
		/// example, to change the enabled state of a <b>Forward</b> button 
		/// when the ShellView control navigates to or leaves the end of the 
		/// navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate forwards through.
		/// </exception>
		public void NavigateForward()
		{
			this.CurrentFolder = m_History.MoveForward();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control forwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the 
		/// <see cref="NavigateForward"/> method to implement a drop-down menu 
		/// on a <b>Forward</b> button similar to the one in Windows Explorer, 
		/// which will allow your users to return to a folder in the 'forward'
		/// navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'forward' history.
		/// </exception>
		public void NavigateForward(ShellItem folder)
		{
			m_History.MoveForward(folder);
			this.CurrentFolder = folder;

			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates to the parent of the currently displayed folder.
		/// </summary>
		public void NavigateParent()
		{
			Navigate(this.CurrentFolder.Parent);
		}
		async void OnNavigated(NavigatedEventArgs e)
		{
			if (Navigated != null)
			{
				Navigated(this, e);

			}
		}

		internal void OnSelectionChanged()
		{

			if (SelectionChanged != null)
			{
				SelectionChanged(this, EventArgs.Empty);
			}

		}

		void OnViewChanged(ViewChangedEventArgs e)
		{
			if (ViewStyleChanged != null)
			{
				ViewStyleChanged(this, e);
			}
		}
		public void ResizeIcons(int value)
		{
			this.IconSize = value;
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ItemWidth"));
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ItemHeight"));
			foreach (var item in this.lv.Items)
			{
				var a = (System.Windows.Controls.ListViewItem)this.lv.ItemContainerGenerator.ContainerFromItem(item);
				//System.Windows.MessageBox.Show("A");
				if (a.IsVisible)
				{
					var itemReal = (ShellObject)item;
					itemReal.UpdateBinding("ShellIconSize");
					itemReal.UpdateBinding("Thumbnail");
					itemReal.UpdateBinding("LabelWidth");
				}
				
			}
			OnViewChanged(new ViewChangedEventArgs(this.View, value));
			
			//_iconSize = value;
			//this.Cancel = true;
			//waitingThumbnails.Clear();
			////this.cache.Clear();
			//System.Windows.Forms.ImageList il = new System.Windows.Forms.ImageList();
			//il.ImageSize = new System.Drawing.Size(value, value);
			//System.Windows.Forms.ImageList ils = new System.Windows.Forms.ImageList();
			//ils.ImageSize = new System.Drawing.Size(16, 16);
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
			//this.Cancel = false;

		}
		void IShellViewEx_Loaded(object sender, RoutedEventArgs e)
		{
			AdornerLayer.GetAdornerLayer(lv).Add(myAdorner = new ListViewSimpleAdorner(lv));

			lv.PreviewMouseDown += (o, ex) =>
			{
				if (ex.ChangedButton == MouseButton.Left)
				{
					if (!(ex.OriginalSource is ScrollViewer))
					{
						return;
					}

					myDragStartPoint = ex.GetPosition(lv);
					if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
					{
						lv.SelectedItems.Clear();
					}
				}
			};

			lv.MouseMove += (o, ex) =>
			{
				if (myDragStartPoint.HasValue)
				{
					Rect r = new Rect(myDragStartPoint.Value,
					ex.GetPosition(lv) - myDragStartPoint.Value);
					myAdorner.HighlightArea = r;
					var items = lv.GetItemAt<ShellObject>(r);
					if (items.Count > 0)
					{
						lv.SelectedItems.Clear();
						foreach (var i in items)
							lv.SelectedItems.Add(i);
					}
					else
						lv.SelectedItems.Clear();
				}
			};

			lv.MouseUp += (o, ex) =>
			{
				if (ex.ChangedButton == MouseButton.Left)
				{
					myDragStartPoint = null;
					myAdorner.HighlightArea = new Rect();
				}
			};

			lv.MouseLeave += (o, ex) =>
			{
				myDragStartPoint = null;
				myAdorner.HighlightArea = new Rect();
			};
			this.IconSize = 48;
			this.lv.View = this.lv.FindResource("iconView") as ViewBase;
		}

		
		private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			OnSelectionChanged();
		}

		private void lv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (lv.SelectedItems.Count == 1)
			{

				ShellItem selectedItem = (lv.SelectedItems[0] as ShellObject).item;
				if (selectedItem.IsFolder)
				{
					Navigate(selectedItem);
				}
				else
				{
					Process.Start(selectedItem.ParsingName);
				}

				
				
			}
		}

		private void lv_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			ShellContextMenu cm = new ShellContextMenu(this.SelectedItems.ToArray());
			Point eGetPosition = e.GetPosition(this);
			var relativepoint = this.lv.PointToScreen(eGetPosition);
			cm.ShowContextMenu(new System.Drawing.Point((int)relativepoint.X, (int)relativepoint.Y));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
