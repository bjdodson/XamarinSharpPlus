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
  
**Customisation**

You can now override the default overlay image by setting the static `OverlayImage` property on the `ELCImagePickerViewController` class.  


**Localisation**

Most of the labels and strings support localisation.  The sample provides an english string file to use as the basis for implementing other languages.

Simply create a folder in the resources folder within your iOS project which is prefixed with the ISO language code for the required language.  Create an empty file called "Localizable.strings" and then duplicate the keys for the english language and then replace the phrases with the localised versions.  



