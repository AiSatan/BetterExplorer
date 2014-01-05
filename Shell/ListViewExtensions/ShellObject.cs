using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BExplorer.Shell
{
	/// <summary>
	/// The base Listview item object for windows filesystem
	/// </summary>
	public class ShellObject : INotifyPropertyChanged
	{
		public ShellItem item;
		public IShellViewEx TheListView;
		public int Index { get; private set; }
		public Boolean VisualStatus {
			get
			{
				return ((ListViewItem)this.TheListView.lv.ItemContainerGenerator.ContainerFromItem(this.TheListView.lv.Items[this.Index])).IsVisible;
			}
		}
		private bool LoadedFromCache { get; set; }

		public Boolean IsSelected { get; set; }

		public int ShellIconSize
		{
			get
			{
				return this.TheListView.IconSize;
			}
		}


		public Boolean IsHidden
		{
			get
			{
				return this.item.IsHidden;
			}
		}


		public Double IconOpacity
		{
			get
			{
				return this.IsHidden ? 0.5 : 1;
			}
		}

		public Double LabelWidth
		{
			get
			{
				return this.TheListView.ItemWidth - 10;
			}
		}

		public CancellationToken token;

		private BitmapSource image;
		public bool isCancel = false;

		public String Type
		{
			get
			{
				return "";
			}
		}
    BitmapSource overlaysrc = null;
		public BitmapSource OverlayIcon
		{
			get
			{
				if (overlaysrc == null)
				{
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
								(Action)(() =>
								{
									int overlay = 0;
									this.TheListView.jumbo.GetIconIndexWithOverlay(this.item.Pidl, out overlay);
									if (overlay > 0)
									{
										int overlayIndex = this.TheListView.jumbo.GetIndexOfOverlay(overlay);
										if (overlayIndex > 0)
										{
											overlaysrc = Helpers.CreateBitmapSourceFromBitmap(this.TheListView.jumbo.GetIcon(overlayIndex).ToBitmap());
											if (PropertyChanged != null)
												PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OverlayIcon"));
										}
									}
								}));
				}
				return overlaysrc;
			}
		}
		public BitmapSource FallbackIcon
		{
			get
			{
				item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
				item.Thumbnail.FormatOption = ShellThumbnailFormatOption.ThumbnailOnly;
				item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.CacheOnly;
				BitmapSource cached = item.Thumbnail.BitmapSource ;

				if (cached != null && cached.Width == this.ShellIconSize)
				{
					LoadedFromCache = true;
					image = cached;
					return cached;
				}

				if (item.Extension != ".exe")
				{
					item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
					item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;

					BitmapSource src = null;
					src = item.Thumbnail.BitmapSource;

					if (src != null && src.Width == this.ShellIconSize)
					{
						LoadedFromCache = false;
						return src;
					}
				}
				else
				{
					return IShellViewEx.ApplicationFallback;
				}
				return null;
			}
		}
		public BitmapSource Thumbnail
		{
			get
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
								(Action)(() =>
								{
										if (image == null || image.Width != ShellIconSize)
										{
											Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
														(ThreadStart)(() =>
														{
															item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
															item.Thumbnail.FormatOption = this.ShellIconSize == 16 ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.Default;
															item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;

															BitmapSource src = null;
															src = item.Thumbnail.BitmapSource;

															if (src != null && src.Width == this.ShellIconSize)
															{
																image = src;
																if (PropertyChanged != null && image != null)
																	PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Thumbnail"));
															}
														}));
										}
								}));
				return image;
					//else
					//{
					//	if (image != null && image.Width == TheListView.IconSize)
					//	{
					//		isCancel = false;
					//		return image;
					//	}
					//	else
					//	{
					//		//if (( flags & IExtractIconpwFlags.GIL_PERINSTANCE) == 0 &&  !item.IsFolder && !item.ParsingName.EndsWith(".jpg") )
					//		//{
					//		//	item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
					//		//	item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					//		//	item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;
					//		//	image = item.Thumbnail.BitmapSource;
					//		//	if (PropertyChanged != null)
					//		//		PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Thumbnail"));
					//		//}
					//		//else

					//		if (item.Extension != ".exe")
					//		{
					//			item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
					//			item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					//			item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;
					//			img = item.Thumbnail.BitmapSource;
					//		}
					//		//Task.Run(() =>
					//		//{
					//		isCancel = true;
					//		Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
					//		(ThreadStart)(() =>
					//		{
					//			item.Thumbnail.CurrentSize = new System.Windows.Size(TheListView.IconSize, TheListView.IconSize);
					//			item.Thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
					//			item.Thumbnail.RetrievalOption = ShellThumbnailRetrievalOption.Default;
					//			BitmapSource g = null;

					//			g = item.Thumbnail.BitmapSource;
					//			if (g != null)
					//				g.Freeze();
					//			image = g;
					//			if (PropertyChanged != null && image != null)
					//				PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Thumbnail"));
					//		}));



					//	//	}, this.token);
					//		return img;
					//	}
					//}
			}
		}

		public String DisplayName
		{
			get
			{
				return item.DisplayName;
			}
		}
		IExtractIconpwFlags flags;
		public ShellObject(ShellItem obj, IShellViewEx listview, int index)
		{
			item = obj;
			this.Index = index;
			flags = item.GetIconType();
			this.TheListView = listview;
		}

		public void UpdateBinding(String propertyname)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyname));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
