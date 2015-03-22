using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ELCImagePicker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ELCImagePickerSample
{
	partial class ELImagePickerDemoViewController : UITableViewController
	{
		#region Fields

		private List<AssetResult> mResults = new List<AssetResult>();

		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ELCImagePickerSample.ELImagePickerDemoViewController"/> class.
		/// </summary>
		/// <param name="handle">Handle.</param>
		public ELImagePickerDemoViewController (IntPtr handle) : base (handle)
		{
			this.Title = "ELCImagePicker Demo";
		}

		#endregion

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var aButton = new UIBarButtonItem(UIBarButtonSystemItem.Add,(s,e)=>
			{

				//create a new instance of the picker view controller
				var picker = ELCImagePickerViewController.Instance;

				//set the maximum number of images that can be selected
				picker.MaximumImagesCount = 15;

				//setup the handling of completion once the items have been picked or the picker has been cancelled
				picker.Completion.ContinueWith (t =>
				{
					//execute any UI code on the UI thread
					this.BeginInvokeOnMainThread(()=>
					{
						//dismiss the picker
						picker.DismissViewController(true,null);

						if (t.IsCanceled || t.Exception != null) 
						{
							//cancelled or error
						} else 
						{
							//get the selected items
							var items = t.Result as List<AssetResult>;

							foreach (AssetResult aItem in items)
							{
								mResults.Add(aItem);
							}

							TableView.ReloadData();

						}
					});
				});


				this.PresentViewController (picker, true, null);

			});


			this.NavigationItem.RightBarButtonItem = aButton;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return mResults.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			const string cellIdentifier = "Cell";

			var cell = tableView.DequeueReusableCell (cellIdentifier);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
			}

			var asset = mResults[indexPath.Row];

			cell.ImageView.Image = asset.Image;
			cell.TextLabel.Text = asset.Name;

			return cell;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var asset = mResults[indexPath.Row];
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 100;
		}
	}
}
