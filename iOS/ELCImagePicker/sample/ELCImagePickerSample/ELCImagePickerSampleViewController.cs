using System;
using System.Drawing;
using Foundation;
using UIKit;
using ELCImagePicker;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ELCImagePickerSample
{
	public partial class ELCImagePickerSampleViewController : UIViewController
	{
		public ELCImagePickerSampleViewController(IntPtr handle) : base(handle)
		{
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
			
			// Release any cached data, images, etc that aren't in use.
		}

		partial void OnClickBrowse(UIButton sender)
		{

		    var picker = ELCImagePickerViewController.Instance;
		    picker.MaximumImagesCount = 15;

		    picker.Completion.ContinueWith (t => {
			      if (t.IsCanceled || t.Exception != null) 
					{
			            
					} else {
						var items = t.Result is List<AssetResult>;
					}
				 });
			
			this.PresentViewController (picker, true, null);

		}

		#region View lifecycle

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
		}

		#endregion
	}
}

