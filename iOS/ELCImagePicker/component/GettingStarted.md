# ELCImagePicker

*A clone of the UIImagePickerController using the Assets Library Framework allowing for multiple asset selection.*
 
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
     

The `AssetResult` class has three properties 

 * Name (String)
 * Path (String)
 * Image (UIImage)
 
You can use these to access the UIImage for the selected images and also the name and Asset Library path, for loading it later.

