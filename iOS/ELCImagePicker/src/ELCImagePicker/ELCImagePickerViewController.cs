﻿using System;
using UIKit;
using AssetsLibrary;
using System.Collections.Generic;
using System.Drawing;
using Foundation;
using System.Threading.Tasks;
using CoreGraphics;

namespace ELCImagePicker
{
	/// <summary>
	/// Asset result.
	/// </summary>
	public class AssetResult
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public String Name
		{
			get;
			set;
		}

		/// <summary>
		/// Selected image
		/// </summary>
		/// <value>The image.</value>
		public UIImage Image { get; set; }

		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		/// <value>The path.</value>
		public String Path
		{
			get;
			set;
		}
	}


	/** 
     * Presents a photo picker dialog capable of selecting multiple images at once.
     * Usage:
     * 
     * var picker = ELCImagePickerViewController.Instance;
     * picker.MaximumImagesCount = 15;
     * picker.Completion.ContinueWith (t => {
     *   if (t.IsCancelled || t.Exception != null) {
     *     // no pictures for you!
     *   } else {
     *      // t.Result is a List<AssetResult>
     *    }
     * });
     * 
     * PresentViewController (picker, true, null);
     */
	public class ELCImagePickerViewController : UINavigationController
	{


		private static Lazy<UIImage> mDefaultOverlayImage = new Lazy<UIImage>(() => UIImage.FromBundle("overlay-image"));

		private static UIImage mOverlayImage;


		/// <summary>
		/// Gets or sets the overlay image.
		/// </summary>
		/// <value>The overlay image.</value>
		public static UIImage OverlayImage
		{
			get
			{
				if (mOverlayImage == null)
					mOverlayImage = mDefaultOverlayImage.Value;

				return mOverlayImage;


			}
			set { mOverlayImage = value; }

		}

		/// <summary>
		/// Gets or sets the maximum images count.
		/// </summary>
		/// <value>The maximum images count.</value>
		public int MaximumImagesCount { get; set; }

		readonly TaskCompletionSource<List<AssetResult>> _TaskCompletionSource = new TaskCompletionSource<List<AssetResult>>();

		public Task<List<AssetResult>> Completion
		{
			get
			{
				return _TaskCompletionSource.Task;
			}
		}

		[Obsolete("Use Create() instead")]
		//static ELCAlbumPickerController albumPicker
		public static ELCImagePickerViewController Instance
		{
			get
			{
				var albumPicker = new ELCAlbumPickerController();

				var picker = new ELCImagePickerViewController(albumPicker);
				albumPicker.Parent = picker;
				picker.MaximumImagesCount = 4;
				//picker.NavigationBar.BarStyle = UIBarStyle.Black;
				return picker;
			}
		}

		/// <summary>
		/// Create a new instance of the Picker
		/// </summary>
		/// <param name="maxImages">Max images.</param>
		/// <param name="selectAlbumTitle">Select album title.</param>
		/// <param name="pickPhotosTitle">Pick photos title.</param>
		/// <param name="backBattonTitle">Back batton title.</param>
		/// <param name="pickPhotoTitle">Pick photo title.</param>
		/// <param name="doneButtonTitle">Done button title.</param>
		/// <param name="loadingtitle">Loadingtitle.</param>
		public static ELCImagePickerViewController Create(int maxImages = 4, string selectAlbumTitle = null, string pickPhotosTitle = null, string backBattonTitle = null, string pickPhotoTitle = null, string doneButtonTitle = null, string loadingtitle = null)
		{
			var albumPicker = new ELCAlbumPickerController()
			{
				SelectAlbumTitle = selectAlbumTitle,
				BackButtonTitle = backBattonTitle,
				DoneButtonTitle = doneButtonTitle,
				LoadingTitle = loadingtitle,
				PickPhotosTitle = pickPhotosTitle,
				PickPhotoTitle = pickPhotoTitle,
			};

			var picker = new ELCImagePickerViewController(albumPicker);
			albumPicker.Parent = picker;
			picker.MaximumImagesCount = maxImages;
			return picker;

		}

		ELCImagePickerViewController(UIViewController rootController) : base(rootController)
		{

		}


		void SelectedAssets(List<ALAsset> assets)
		{
			var results = new List<AssetResult>(assets.Count);
			foreach (var asset in assets)
			{
				var obj = asset.AssetType;
				if (obj == default(ALAssetType))
					continue;

				var rep = asset.DefaultRepresentation;
				if (rep != null)
				{
					var result = new AssetResult();
					UIImageOrientation orientation = UIImageOrientation.Up;
					var cgImage = rep.GetFullScreenImage();

					//set if not null
					if (cgImage != null)
						result.Image = new UIImage(cgImage, 1.0f, orientation);

					result.Name = rep.Filename;
					result.Path = rep.Url.AbsoluteString;

					results.Add(result);
				}
			}

			_TaskCompletionSource.TrySetResult(results);
		}

		void CancelledPicker()
		{
			_TaskCompletionSource.TrySetCanceled();
		}

		bool ShouldSelectAsset(ALAsset asset, int previousCount)
		{
			var shouldSelect = MaximumImagesCount <= 0 || previousCount < MaximumImagesCount;
			if (!shouldSelect)
			{
				var first = NSBundle.MainBundle.LocalizedString("Only", "Only");
				var second = NSBundle.MainBundle.LocalizedString("photos please", "photos please!");

				string title = String.Format("{0} {1} {2}", first, MaximumImagesCount, second);

				var third = NSBundle.MainBundle.LocalizedString("You can only send", "You can only send");
				var fourth = NSBundle.MainBundle.LocalizedString("photos at a time", "photos at a time.");

				string message = String.Format("{0} {1} {2}", third, MaximumImagesCount, fourth);
				var alert = new UIAlertView(title, message, null, null, NSBundle.MainBundle.LocalizedString("Okay", "Okay"));
				alert.Show();
			}
			return shouldSelect;
		}

		public class ELCAlbumPickerController : UITableViewController
		{
			public string DoneButtonTitle { get; set; }	
			public string BackButtonTitle { get; set; }
			public string SelectAlbumTitle { get; set; }
			public string LoadingTitle { get; set; }
			public string PickPhotoTitle { get; set; }
			public string PickPhotosTitle { get; set; }

			static readonly NSObject _Dispatcher = new NSObject();
			readonly List<ALAssetsGroup> AssetGroups = new List<ALAssetsGroup>();

			ALAssetsLibrary Library;

			WeakReference _Parent;

			public ELCImagePickerViewController Parent
			{
				get
				{
					return _Parent == null ? null : _Parent.Target as ELCImagePickerViewController;
				}
				set
				{
					_Parent = new WeakReference(value);
				}
			}

			public ELCAlbumPickerController()
			{
			}

			public override void ViewDidLoad()
			{
				base.ViewDidLoad();
				string loading = string.IsNullOrWhiteSpace(LoadingTitle) ? NSBundle.MainBundle.LocalizedString("Loading", "Loading...") : LoadingTitle;

				NavigationItem.Title = loading;
				var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
				cancelButton.Clicked += CancelClicked;
				NavigationItem.RightBarButtonItem = cancelButton;

				AssetGroups.Clear();

				Library = new ALAssetsLibrary();
				Library.Enumerate(ALAssetsGroupType.All, GroupsEnumerator, GroupsEnumeratorFailed);
			}

			public override void ViewDidDisappear(bool animated)
			{
				base.ViewDidDisappear(animated);
				if (IsMovingFromParentViewController || IsBeingDismissed)
				{
					NavigationItem.RightBarButtonItem.Clicked -= CancelClicked;
				}
			}

			void CancelClicked(object sender = null, EventArgs e = null)
			{
				var parent = Parent;
				if (parent != null)
				{
					parent.CancelledPicker();
				}
			}

			void GroupsEnumeratorFailed(NSError error)
			{
				Console.WriteLine(NSBundle.MainBundle.LocalizedString("Enumerator failed", "Enumerator failed!"));
			}

			void GroupsEnumerator(ALAssetsGroup agroup, ref bool stop)
			{
				if (agroup == null)
				{
					return;
				}

				// added fix for camera albums order
				if (agroup.Name.ToString().ToLower() == "camera roll" && agroup.Type == ALAssetsGroupType.SavedPhotos)
				{
					AssetGroups.Insert(0, agroup);
				}
				else
				{
					AssetGroups.Add(agroup);
				}

				_Dispatcher.BeginInvokeOnMainThread(ReloadTableView);
			}

			void ReloadTableView()
			{
				TableView.ReloadData();
				string selectAlbum = string.IsNullOrWhiteSpace(SelectAlbumTitle) ? NSBundle.MainBundle.LocalizedString("Select an Album", "Select an Album") : SelectAlbumTitle;
				NavigationItem.Title = selectAlbum;
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return AssetGroups.Count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				const string cellIdentifier = "Cell";

				var cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
				}

				// Get count
				var g = AssetGroups[indexPath.Row];
				g.SetAssetsFilter(ALAssetsFilter.AllPhotos);
				var gCount = g.Count;
				cell.TextLabel.Text = string.Format("{0} ({1})", g.Name, gCount);
				try
				{
					cell.ImageView.Image = new UIImage(g.PosterImage);
				}
				catch (Exception e)
				{
					Console.WriteLine("{0} {1}", NSBundle.MainBundle.LocalizedString("Failed to set thumbnail", "Failed to set thumbnail"), e);
				}
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				var assetGroup = AssetGroups[indexPath.Row];
				assetGroup.SetAssetsFilter(ALAssetsFilter.AllPhotos);
				var picker = new ELCAssetTablePicker(assetGroup);

				picker.LoadingTitle = LoadingTitle;
				picker.PickPhotosTitle = PickPhotosTitle;
				picker.PickPhotoTitle = PickPhotoTitle;
				picker.DoneButtonTitle = DoneButtonTitle;

				picker.Parent = Parent;

				string backButtonTitle = string.IsNullOrWhiteSpace(BackButtonTitle) ? NSBundle.MainBundle.LocalizedString("Back", "Back") : BackButtonTitle;

				this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(backButtonTitle, UIBarButtonItemStyle.Plain, null);

				NavigationController.PushViewController(picker, true);
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				return 57;
			}
		}


		class ELCAssetTablePicker : UITableViewController
		{
			private string doneButtonTitle { get; set; }
			private string pickPhotoTitle { get; set; }
			private string pickPhotosTitle { get; set; }
			private string loadingTitle { get; set; }

			public string DoneButtonTitle
			{
				get
				{
					if (string.IsNullOrWhiteSpace(doneButtonTitle))
						return NSBundle.MainBundle.LocalizedString("Done", "Done");

					return doneButtonTitle;
				}
				set { doneButtonTitle = value; }
			}

			public string PickPhotoTitle
			{
				get
				{
					if (string.IsNullOrWhiteSpace(pickPhotoTitle))
						return NSBundle.MainBundle.LocalizedString("Pick Photo", "Pick Photo");

					return pickPhotoTitle;
				}
				set { pickPhotoTitle = value; }
			}

			public string PickPhotosTitle
			{
				get
				{
					if (string.IsNullOrWhiteSpace(pickPhotosTitle))
						return NSBundle.MainBundle.LocalizedString("Pick Photos", "Pick Photos");

					return pickPhotosTitle;
				}
				set { pickPhotosTitle = value; }
			}

			public string LoadingTitle
			{
				get
				{
					if (string.IsNullOrWhiteSpace(loadingTitle))
						return NSBundle.MainBundle.LocalizedString("Loading", "Loading...");

					return loadingTitle;
				}
				set { loadingTitle = value; }
			}

			static readonly NSObject _Dispatcher = new NSObject();

			int Columns = 4;

			public bool SingleSelection { get; set; }

			public bool ImmediateReturn { get; set; }

			readonly ALAssetsGroup AssetGroup;

			readonly List<ELCAsset> ElcAssets = new List<ELCAsset>();

			WeakReference _Parent;

			public ELCImagePickerViewController Parent
			{
				get
				{
					return _Parent == null ? null : _Parent.Target as ELCImagePickerViewController;
				}
				set
				{
					_Parent = new WeakReference(value);
				}
			}

			public ELCAssetTablePicker(ALAssetsGroup assetGroup)
			{
				AssetGroup = assetGroup;
			}

			public override void ViewDidLoad()
			{
				TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
				TableView.AllowsSelection = false;

				if (ImmediateReturn)
				{

				}
				else
				{
					var doneButtonItem = new UIBarButtonItem(DoneButtonTitle, UIBarButtonItemStyle.Done, null);
					doneButtonItem.Clicked += DoneClicked;
					NavigationItem.RightBarButtonItem = doneButtonItem;
					NavigationItem.Title = LoadingTitle;
				}

				Task.Run((Action)PreparePhotos);
			}

			public override void ViewWillAppear(bool animated)
			{
				base.ViewWillAppear(animated);
				Columns = (int)(View.Bounds.Size.Width / 80f);
			}

			public override void ViewDidDisappear(bool animated)
			{
				base.ViewDidDisappear(animated);
				if (IsMovingFromParentViewController || IsBeingDismissed)
				{
					NavigationItem.RightBarButtonItem.Clicked -= DoneClicked;
				}
			}

			public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
			{
				base.DidRotate(fromInterfaceOrientation);
				Columns = (int)(View.Bounds.Size.Width / 80f);
				TableView.ReloadData();
			}

			void PreparePhotos()
			{
				AssetGroup.Enumerate(PhotoEnumerator);

				_Dispatcher.BeginInvokeOnMainThread(() =>
				{
					TableView.ReloadData();
					// scroll to bottom
					var section = NumberOfSections(TableView) - 1;
					var row = TableView.NumberOfRowsInSection(section) - 1;
					if (section >= 0 && row >= 0)
					{
						var ip = NSIndexPath.FromRowSection(row, section);
						TableView.ScrollToRow(ip, UITableViewScrollPosition.Bottom, false);
					}


					NavigationItem.Title = SingleSelection ? PickPhotoTitle : PickPhotosTitle;
				});
			}

			#region Not interested in
			void PhotoEnumerator(ALAsset result, nint index, ref bool stop)
			{
				if (result == null)
				{
					return;
				}

				ELCAsset elcAsset = new ELCAsset(this, result);

				bool isAssetFiltered = false;
				/*if (self.assetPickerFilterDelegate &&
                    [self.assetPickerFilterDelegate respondsToSelector:@selector(assetTablePicker:isAssetFilteredOut:)])
                {
                    isAssetFiltered = [self.assetPickerFilterDelegate assetTablePicker:self isAssetFilteredOut:(ELCAsset*)elcAsset];
                }*/

				if (result.DefaultRepresentation == null)
					isAssetFiltered = true;

				if (!isAssetFiltered)
				{
					ElcAssets.Add(elcAsset);
				}
			}

			void DoneClicked(object sender = null, EventArgs e = null)
			{
				var selected = new List<ALAsset>();

				foreach (var asset in ElcAssets)
				{
					if (asset.Selected)
					{
						selected.Add(asset.Asset);
					}
				}

				var parent = Parent;
				if (parent != null)
				{
					parent.SelectedAssets(selected);
				}
			}

			bool ShouldSelectAsset(ELCAsset asset)
			{
				int selectionCount = TotalSelectedAssets;
				bool shouldSelect = true;

				var parent = Parent;
				if (parent != null)
				{
					shouldSelect = parent.ShouldSelectAsset(asset.Asset, selectionCount);
				}

				return shouldSelect;
			}

			void AssetSelected(ELCAsset asset, bool selected)
			{
				TotalSelectedAssets += (selected) ? 1 : -1;

				if (SingleSelection)
				{
					foreach (var elcAsset in ElcAssets)
					{
						if (asset != elcAsset)
						{
							elcAsset.Selected = false;
						}
					}
				}
				if (ImmediateReturn)
				{
					var parent = Parent;
					var obj = new List<ALAsset>(1);
					obj.Add(asset.Asset);
					parent.SelectedAssets(obj);
				}
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				if (Columns <= 0)
					return 4;
				int numRows = (int)Math.Ceiling((float)ElcAssets.Count / Columns);
				return numRows;
			}

			List<ELCAsset> AssetsForIndexPath(NSIndexPath path)
			{
				int index = path.Row * Columns;
				int length = Math.Min(Columns, ElcAssets.Count - index);
				return ElcAssets.GetRange(index, length);
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				const string cellIdentifier = "Cell";

				var cell = TableView.DequeueReusableCell(cellIdentifier) as ELCAssetCell;
				if (cell == null)
				{
					cell = new ELCAssetCell(UITableViewCellStyle.Default, cellIdentifier);
				}
				cell.SetAssets(AssetsForIndexPath(indexPath), Columns);
				return cell;
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				return 79;
			}

			public int TotalSelectedAssets;

			public class ELCAsset
			{
				public readonly ALAsset Asset;
				readonly WeakReference _Parent;

				bool _Selected;

				public ELCAsset(ELCAssetTablePicker parent, ALAsset asset)
				{
					_Parent = new WeakReference(parent);
					Asset = asset;
				}

				public void ToggleSelected()
				{
					Selected = !Selected;
				}

				public bool Selected
				{
					get
					{
						return _Selected;
					}

					set
					{
						var parent = _Parent.Target as ELCAssetTablePicker;
						if (value && parent != null && !parent.ShouldSelectAsset(this))
						{
							return;
						}

						_Selected = value;

						if (parent != null)
						{
							parent.AssetSelected(this, value);
						}
					}
				}
			}

			class ELCAssetCell : UITableViewCell
			{
				List<ELCAsset> RowAssets;
				int Columns;
				readonly List<UIImageView> ImageViewArray = new List<UIImageView>();
				readonly List<UIImageView> OverlayViewArray = new List<UIImageView>();

				public ELCAssetCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
				{
					UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer(CellTapped);
					AddGestureRecognizer(tapRecognizer);

				}

				public void SetAssets(List<ELCAsset> assets, int columns)
				{
					RowAssets = assets;
					Columns = columns;

					foreach (var view in ImageViewArray)
					{
						view.RemoveFromSuperview();
					}
					foreach (var view in OverlayViewArray)
					{
						view.RemoveFromSuperview();
					}

					//UIImage overlayImage = null;
					for (int i = 0; i < RowAssets.Count; i++)
					{
						var asset = RowAssets[i];

						try
						{

							if (asset.Asset != null
									&& asset.Asset.Thumbnail != null)
							{
								if (i < ImageViewArray.Count)
								{
									var imageView = ImageViewArray[i];
									imageView.Image = new UIImage(asset.Asset.Thumbnail);
								}
								else
								{
									var imageView = new UIImageView(new UIImage(asset.Asset.Thumbnail));
									ImageViewArray.Add(imageView);
								}
							}

						}
						catch (Exception e)
						{
							Console.WriteLine("{0} {1}", NSBundle.MainBundle.LocalizedString("Failed to set thumbnail", "Failed to set thumbnail"), e);
						}

						if (i < OverlayViewArray.Count)
						{
							var overlayView = OverlayViewArray[i];
							overlayView.Hidden = !asset.Selected;
						}
						else
						{
							var overlayView = new UIImageView(OverlayImage);
							OverlayViewArray.Add(overlayView);
							overlayView.Hidden = !asset.Selected;
						}
					}
				}

				void CellTapped(UITapGestureRecognizer tapRecognizer)
				{
					var point = tapRecognizer.LocationInView(this);
					var totalWidth = Columns * 75 + (Columns - 1) * 4;
					var startX = (Bounds.Size.Width - totalWidth) / 2;

					var frame = new CGRect(startX, 2, 75, 75);
					for (int i = 0; i < RowAssets.Count; ++i)
					{
						if (frame.Contains(point))
						{
							ELCAsset asset = RowAssets[i];
							asset.Selected = !asset.Selected;
							var overlayView = OverlayViewArray[i];
							overlayView.Hidden = !asset.Selected;
							break;
						}
						var x = frame.X + frame.Width + 4;
						frame = new CGRect(x, frame.Y, frame.Width, frame.Height);
					}
				}

				public override void LayoutSubviews()
				{
					var totalWidth = Columns * 75 + (Columns - 1) * 4;
					var startX = (Bounds.Size.Width - totalWidth) / 2;

					var frame = new CGRect(startX, 2, 75, 75);

					int i = 0;
					foreach (var imageView in ImageViewArray)
					{
						imageView.Frame = frame;
						AddSubview(imageView);

						var overlayView = OverlayViewArray[i++];
						overlayView.Frame = frame;
						AddSubview(overlayView);

						var x = frame.X + frame.Width + 4;
						frame = new CGRect(x, frame.Y, frame.Width, frame.Height);
					}
				}
			}

			#endregion


		}
	}
}