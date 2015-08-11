# ELCImagePicker

*A clone of the UIImagePickerController using the Assets Library Framework allowing for multiple asset selection.*

 * Multi-select of images
 * Support for Xamarin.iOS Unified API
 * Returns UIImage, Name and Asset path of the selected items
 * TPL-friendly Interface
 * Localization support
 
## Usage

The image picker is created and displayed in a very similar manner to the `UIImagePickerController`. The sample application  shows its use. To display the controller you instantiate it and display it modally like so.

     var picker = ELCImagePickerViewController.Instance;
     picker.MaximumImagesCount = 15;
     picker.Completion.ContinueWith (t => {
       if (t.IsCanceled || t.Exception != null) {
         // no pictures for you!
       } else {
          var items = t.Result as List<AssetResult>;
        }
     });
     
     PresentViewController (picker, true, null);

Originally ported to C# by [bjdodson](https://github.com/bjdodson/XamarinSharpPlus)
   
Ported from Original Objective-C Project - [https://github.com/B-Sides/ELCImagePickerController](https://github.com/B-Sides/ELCImagePickerController)