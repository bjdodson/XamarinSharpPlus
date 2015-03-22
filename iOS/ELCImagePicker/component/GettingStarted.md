DSGridView is a fast lightweight and customisable data grid component for all your data presentation needs for iOS(Unified and Classic) and Android.

Classes are provided at a View level(DSGridView) and a Controller level (DSGridViewController, DSGridViewActivity) to get you started in as few steps as possible on each platform.  

The Data layer is cross-platform so you can use it on both platforms too.

# Android Beta #

Android support is currently considered beta.  There may be elements that do not function as expected and performance on large datasets is, currently, not as good as it is on iOS.  

If you find any issues then please get in contact so we can get them fixed, details are in the "Contact Us" section below.


### Upgrade Notes v2.x - v2.6 - DSDataTable sub-classes

To ensure that the selection list functions correctly when sorting a column we have had to change the way in which it stores references to the selected items.  In versions of DSGridView up to 2.5 this was done using the index of the row in the dataset.  This however doesn't work when you resort the data for a Large dataset that is loaded Just-In-Time.

As of 2.6 we will now store the new `RowId` of the selected item(s) instead.  This means that when the data is sorted the link to the selected items can be re-established and the items can remain selected in the Grid View.

If you notice that the selection mechanism is no longer working then please review the Create Datasource section below.

*Please Note this only affects users who have sub-classed DSDataTable*

### Upgrade Notes v1.x - v2.x

As part of the move to a cross platform solution for iOS, Android and Windows Phone we have had to make some changes to the exiting codebase.  This is partly to enable us to reuse as much code as possible and partly due to incompatibilities with some of the code on different platforms.  

We have tried to minimise as much as possible and below are the steps that you may need to go through when upgrading your project.

* Add a reference to DSoft.UI.dll
* Add a reference to DSoft.Themes.dll

*Potential issues*

* Formatters have moved from using DSoft.UI.Grid.Formatters to DSoft.Datatypes.Formatters
* Enums have moved from DSoft.UI.Grid.Enums to using DSoft.Datatypes.Enums
* Cell Tap and Row selectors now return type object as sender not DSGridCellView or DSGridRowView
* DSCellView.X has been replaced with DSCellView.RowIndex
* DSCellView.Y has been replaced with DSCellView.ColumnIndex
* Themes in DSoft.UI.Grid.Themes are deprecated and will be removed in future.  New cross-platform base themes have been created in DSoft.Themes.Grid using DSSoft datatypes to provide cross-platform support.  Extensions are provided to convert from Native types(UIImage, DSFont etc) to DSSoft types (DSImage, DSFont) for overriding the theme values(see below for details))



### Datasources

A Datasource can be one of two types of objects(or sub-classes thereof)

* DSDataTable
* DSDataSet

A DSDataSet is a collection of DSDataTable objects and the current table can be switched by passing the tables "name"" to the TableName property of the DSGridView instance.

**Create Datasource**

For small datasets create a data source object to use with the data grid, a DSDataTable in this case.

Add the columns first then add the rows of data.


	using DSoft.Datatypes.Grid.Data;
	
	public override void ViewDidLoad ()
	{
		//create the data table object and set a name
		var aDataSource = new DSDataTable("ADT");
		//add a column
		var dc1 = new DSDataColumn("Title");
		dc1.Caption = "Title";
		dc1.ReadOnly = true;
		dc1.DataType = typeof(String);
		dc1.AllowSort = true;
		dc1.Width = ColumnsDefs[aKey];
		
		aDataSource.Columns.Add(dc1);
		   
		//add a row to the data table
		var dr = new DSDataRow();
		dr["ID"] = loop;
		dr["Title"] = @"Test";
		dr["Description"] = @"Some description would go here";
		dr["Date"] = DateTime.Now.ToShortDateString();
		dr["Value"] = "10000.00";
		
		//set the value as an image
		dr["Image"] =  UIImage.FromFile("first.png")
		
		aDataSource.Rows.Add(dr); 
		
		...
	}

For large datasets you can create a sub-class of `DSDataTable` and then override some of it functions to allow for "Lazy" loading of the data.

If you want to use the `Rows` property of `DSDataTable` to store your `DSDataRow` objects then you only need to override

* GetRowCount
    * This can be ignored if you want to pre-load your data
* GetRow(int Index)
    * This is used to return the DSDataRow from your collection Just-in-Time
    

If you want to use your own collection then you will need to override the following methods, to ensure that it uses you custom collection and that the selection mechanism still works correctly.

* GetRowCount
* GetRow(int Index)
* GetRow(string RowId)
* IndexOfRow(string RowId)
* GetValue (int RowIndex, string ColumnName)
* SetValue (int RowIndex, string ColumnName, object Value)
* SortByColumn(int ColumnIndex)

This will allow the Grid to load super quick and worry about loading the data when needed.

	using DSoft.Datatypes.Grid.Data;
	
	public class MyDataTable : DSDataTable
	{
	
		public void MyDataTable(String Name) : base(Name)
		{
			//create and add a column
			var dc1 = new DSDataColumn("Title");
			dc1.Caption = "Title";
			dc1.ReadOnly = true;
			dc1.DataType = typeof(String);
			dc1.AllowSort = true;
			dc1.Width = ColumnsDefs[aKey];
			
			this.Columns.Add(dc1);
		}
		
		public override int GetRowCount ()
		{
			return 100000;
		}
		
		public override DSDataRow GetRow (int Index)
		{
			DSDataRow aRow = null;

			//check to see if we have a row for this index
			if (Index < Rows.Count)
			{
				aRow = Rows [Index];
			}
			else
			{
				// create a new one
				aRow = new DSDataRow ();
				aRow ["Title"] = @"Test";
				Rows.Add (aRow);
			}
			
			///set the values
			aRow ["Description"] = @"Some description would go here";
			aRow ["Date"] = DateTime.Now.ToShortDateString ();
			aRow ["Value"] = "10000.00";

			//see if even or odd to pick an image from the array
			var pos = Index % 2;
			aRow ["Image"] = Icons [pos];
			aRow ["Ordered"] = (pos == 0) ? true : false;

			return aRow;
		}		
		
	}


There are extension methods for converting standard .Net DataSet and DataTable objects to their DSoft equivalents, however for speed purposes it would be better to hold a reference to it in you DSDataTable sub-class and pull the data out when required inside the GetValue method.

For a complete example please check out the sample solution and look at the ExampleDataTable class.

`DSDataRow` now also includes an override-able `RowId` property that allows the Grid view to track the selected items.  Bu default this is Guid string, but you can override it to be anything that you want if need be.


### Sorting

If you have created a sub-class of the DSDataTable and implemented GetRowCount and GetRow then you will need to also override

* SortByColumn

This will allow you to sort your data into the right order, based on the specified column, for when the GetValue method is called by the Grid view


	using DSoft.Datatypes.Grid.Data;
	
	public class MyDataTable : DSDataTable
	{
	
		...
		
		public override void SortByColumn (int ColumnIndex)
		{
			base.SortByColumn (ColumnIndex);
			isUpSort = !isUpSort;
		}
		
		...	
		
	}

For a complete example please check out the sample solution and look at the ExampleDataTable class.

*Ensure you are using `Rows` to store your data in the `DSDataTable` base class other wise you will need to override IndexOfRow and GetRow(String Id)*  

### Create Grid View - iOS

Create the grid view as with any normal iOS View and then set the datasource of the DSGridView object.


	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	
	public override void ViewDidLoad ()
	{
		//create the data table object and set a name
		var aDataSource = new DSDataTable("ADT");
		
		//add a column
		var dc1 = new DSDataColumn("Title");
		dc1.Caption = "Title";
		dc1.ReadOnly = true;
		dc1.DataType = typeof(String);
		dc1.AllowSort = true;
		dc1.Width = ColumnsDefs[aKey];
		
		aDataSource.Columns.Add(dc1);
		
		//add a row to the data table
		var dr = new DSDataRow();
		dr["ID"] = loop;
		dr["Title"] = @"Test";
		dr["Description"] = @"Some description would go here";
		dr["Date"] = DateTime.Now.ToShortDateString();
		dr["Value"] = "10000.00";    	
		aDataSource.Rows.Add(dr); 
		
		//Create the grid view, assign the datasource and add the view as a subview
		var aGridView = new DSGridView(new RectangleF(0,0,1024,768));
		aGridView.DataSource = aDataSouce;
		this.View.AddSubview(aGridView);
	        
	}

**DSGridViewController**  
You can also use the `DSGridViewController` class to load the grid automatically into a View controller.  

As of version 2.6 we have added support for automatically adapting to the height of the Navigation Controller toolbar.  This will take care of resizing the `DSGridView` automatically during rotations etc.

*Note: By default this behaviour is disabled, to allow existing projects to continue to work as expected* 

We have add a new property called `DisableNavigationControllerSizing` ,which is set to `true` by default, which can be used to enable or disable the new resizing mechanism. Set to `false` to enable the new mechanism.

*Note: The default will be switched to `false` in future and the property will be deprecated*

### Create Grid View - Android

Create the `DSGridView` in the same way as any normal Android View, either in your layout file or programatically, and then set the datasource of the DSGridView object.

In your layout file setup view 

	<?xml version="1.0" encoding="utf-8"?>
	<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
	    android:orientation="vertical"
	    android:layout_width="fill_parent"
	    android:layout_height="fill_parent">
	    <dsoft.ui.grid.DSGridView
	        android:id="@+id/myDataGrid"
	        android:layout_width="fill_parent"
	        android:layout_height="fill_parent" />
	</LinearLayout>
	
In your Activity build access the view or create one programtically

	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	
		DSGridView mDataGrid;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

			mDataGrid = this.FindViewById<DSGridView>(Resource.Id.myDataGrid);

			/*
				mDataGrid = new DSGridView(this);
				mDataGrid.LayoutParameters = new ViewGroup.LayoutParams (FrameLayout.LayoutParams.FillParent, FrameLayout.LayoutParams.FillParent);

				this.SetContentView (mDataGrid);
			*/
			if (mDataGrid != null)
			{
				mDataGrid.DataSource = new ExampleDataSet (this);
				mDataGrid.TableName = "DT1";
			}


		}

**DSGridViewActivity**  

You can also use the `DSGridViewActivity` class to load the grid automatically into an Activity controller.  

### Register for events

Once the grid is created you can register for the selection events.



	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	public override void ViewDidLoad ()
	{
		//create the data table object and set a name
		var aDataSource = new DSDataTable("ADT");
		
		//add a column
		var dc1 = new DSDataColumn("Title");
		dc1.Caption = "Title";
		dc1.ReadOnly = true;
		dc1.DataType = typeof(String);
		dc1.AllowSort = true;
		dc1.Width = ColumnsDefs[aKey];
		
		aDataSource.Columns.Add(dc1);
		   
		//add a row to the data table
		var dr = new DSDataRow();
		dr["ID"] = loop;
		dr["Title"] = @"Test";
		dr["Description"] = @"Some description would go here";
		dr["Date"] = DateTime.Now.ToShortDateString();
		dr["Value"] = "10000.00";    	
		aDataSource.Rows.Add(dr); 
		
		//Create the grid view, assign the datasource and add the view as a subview
		var aGridView = new DSGridView(new RectangleF(0,0,1024,768));
		aGridView.DataSource = aDataSouce;
		this.View.AddSubview(aGridView);
		        
		// Add handlers for single and double tap on each cell
		aGridView.OnSingleCellTap += (DSGridCellView sender) => 
		{
			//do something
		};
			
		aGridView.OnDoubleCellTap += (DSGridCellView sender) => 
		{
			//do something
		};
			
		//single row selected
		aGridView.OnRowSelect += (DSGridRowView sender, DSDataRow Row) => 
		{
			//do something
		};
			
		//row double tap
		grdView.OnRowDoubleTapped += (DSGridRowView sender, DSDataRow Row) => 
		{
			//do something
		};
	        
	}

### Custom Views   

In DSGridView v2.2 we introduced support for custom views using the `DSViewFormatter<T>` formatter.  This allows you to integrate custom views in to cells for a column within in the grid.

To implement a custom view you will a view that implements the `IDSCustomView` interface.  This interface is designed to provide the ability to receive the value for the cell, the frame for the view, if the column is readonly or not and a action to call if you want to update the data.  This now allows some measure of data entry, which will be implemented fully in a later version of the grid view.

Once you have implemented your view you can create a formatter in the same way as the other formatters(see the "Cell Formatters" section below) by create a new `DSViewFormatter<T>` object with you view class name as the Generic Type.  



		else if (aKey.Equals ("Title"))
		{
			//add a custom view to allow us to update the title
			//DSTextFieldView
			dc1.DataType = typeof(String);
			dc1.AllowSort = true;
			dc1.ReadOnly = false;
			dc1.Formatter = new DSViewFormatter<DSTextFieldView> ();
		}
		

The sample application has an example of a custom view assigned to the "Title" column on the ExampleDataTable class.

You may need to override the "SetValue" method of the DSDataTable class to be able to update your data, depending on if you are lazy loading your data or not.

### Multi-Select and Deselection  

You can enable or disable multi-select at any point by setting the `EnableMultiSelect`  property on the instance of `DSGridView` too true or false.  The Default is false.

You can also enable or disable Deselection(selecting the same row again to deselect it) by setting the `EnableDeselection` property of `DSGridView`. The Default is false.

Setting the `EnableMultiSelect` also sets the `EnableDeselection` to the same boolean value, as in Enabling Multi-Select will also enable Deselection.  You can disable Deselection by setting  `EnableDeselection` to false, after setting `EnableMultiSelect`.

### DataTable Hot-Swapping

If you provide a DSDataSet object as the datasource for the grid you can use hot-swapping to quickly switch between the DSDataTables objects within it.

The DSGridView has a property called TableName for the purpose. Setting the property will switch the data to the specified DataTable and then reload the grid view with the new data.



	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	public override void ViewDidLoad ()
	{
		var aDataSet = new DSDataSet();
		aDataSet.Tables.Add(new DSDataTable("ADT1"));
		aDataSet.Tables.Add(new DSDataTable("ADT2"));
		
		aGridView.DataSource = aDataSet;
		
		//set to the first data table
		aGridView.TableName = ((DSDataSet) aDataSet).Tables[0].Name;
	}
	
	public void OnClickSwitchDataTable()
	{
		aGridView.TableName = "ADT2";
	}


### DS Types and Native Types

In order to provide cross platform support for the theming system which have switched to using our own intermediary data types for values in the theme properties.  This allows us to use the same themes regardless of platform, and should make it easier for you too.

We provide use the following datatypes in the themes

* DSBitmap
* DSColor
* DSFont

We provide global extension methods for you to convert native types into the DSSoft data types.

An example of this is converting an iOS UIColor object to a DSColor object while overriding the HeaderColor property of the theme.

	public override DSColor HeaderColor 
	{
		get 
		{
			return UIColor.LightGray().ToDSColor();
		}
	}

In the case of DSImage, or really UIImage, you may want to create a color from a pattern image.  This can be achieved using the class methods DSColor.FromPatternImage in combination with the extension method that converts a UIImage to a DSImage.  You could create the DSBitmap object from a byte-array or stream, to allow your code to be shared.


	public class ItunesTheme : DSGridDefaultTheme
	{
		public ItunesTheme () : base ()
		{
			//set default values
			HeaderBackground = DSColor.FromPatternImage (new UIImage ("header.png").ToDSBitmap ());

		}

Please read the MonoDocs for more information

### Themes and styling

Since version 2.0 there are three ways you create a custom theme, thanks to setters being added to the theme properties

* Sub-class an existing theme and override the properties
* Sub-class an existing theme and change values in the constructor
* Change the values during instantiation using object initialisation

*Sub-Classing - Overriding*

You can create a custom theme by subclassing either an existing them(such as DSGridDefaultTheme) or creating a new theme based on the the DSGridTheme abstract class.

To change the look of the grid you can simply override the property that you want to change and set it to the values you want to use.  

As an example to change the color of the default header you could simple create a new Class called AGridViewTheme, as a subclass of DSGridViewDefaultTheme, and override the HeaderColor property.


	using DSoft.Datatypes.Grid.Data;
	using DSoft.Theme.Grid;
	
	public class AGridTheme : DSGridDefaultTheme
	{
		private DSColor mHeaderColor = new DSColor(1.0f,0.5f,0.5f,1.0f);
		
		public override MonoTouch.UIKit.UIColor HeaderColor 
		{
			get 
			{
				return mHeaderColor;
			}
			set
			{
				mHeaderColor = value;
			}
		}
	}

Note: You now have to provide a setter, which if you have previously overridden a theme will result compile error(See upgrade notes for more information)

*Sub-Classing - Constructor*

You can now also change a theme by setting new values in the constructor of the sub-class, which has the benefit of not needing to override the properties or providing fields to store the values.


	using DSoft.Datatypes.Grid.Data;
	using DSoft.Theme.Grid;
	
	public class AGridTheme : DSGridDefaultTheme
	{
		public void AGridTheme() : base()
		{
			this.HeaderColor = new DSColor(1.0f,0.5f,0.5f,1.0f);
		}
	}

*Using object initialisation*

You can avoid sub-classing altogether by setting the new value using a object initialiser when you create the object


	using DSoft.Datatypes.Grid.Data;
	using DSoft.Themes.Grid;
	using DSoft.UI.Grid;
	using DSoft.UI.Grid.Views;
	using DSoft.Datatypes.Types;
	
	public class AGridViewController : DSGridViewController
	{
		public void AGridViewController() : base()
		{
			var aNewTheme = new DSGridDefaultTheme()
			{
				HeaderColor = new DSColor(1.0f,0.5f,0.5f,
			};
		}
	}

Note: DSGridTheme is an abstract class and cannot be used for this method, use DSGridDefaultTheme instead or any other DSGridTheme sub-class.

*Global and Instance themes*

As of DSGridView 2.0 you can know set themes at either a Global level that will affect all Grid views and or against an individual instance of the control.

*Global*

To set globally set the theme instance to the Current property of the DSGridTheme base class

	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	
	public override void ViewDidLoad ()
	{
		DSGridTheme.Current = new AGridTheme();
	}

**Instance**

To set on a individual instance of the DSGridView control set the theme instance against the "Theme" property of the control itself.  This will stop it using the Global theme.

To revert to using the global theme set the "Theme" value to null.


### Cell Formatters

You can now set a formatter on DSDataColumn object to allow you to control how the cell is drawn with the value object that is provided for it.

Currently there are formatters for the following types

* Custom Views(DSViewFormatter, see 'Custom Views' for more information)
* UIImage (DSImageFormatter)
* Boolean (DSBooleanFormatter)
* String (DSTextFormatter)

To set the formatter on the DSDataColumn object instantiate the required formatter, configure it and assign it to the objects formatter property.


		//add a column
		var dc1 = new DSDataColumn("Title");
		dc1.Caption = "Title";
		dc1.ReadOnly = true;
		dc1.DataType = typeof(UIImage);
		dc1.AllowSort = false;
		dc1.Width = ColumnsDefs[aKey];

		//Set an image formatter with a square of 50x50
		//apply a 5px margin to all sides
		dc1.Formatter = new DSImageFormatter (new DSSize(50,50))
		{
			Margin = new DSInset(5.0f);
		};
		
		
		//add a column
		var dc2 = new DSDataColumn("Title");
		dc2.Caption = "Title";
		dc2.ReadOnly = true;
		dc2.DataType = typeof(Boolean);
		dc2.AllowSort = false;
		dc2.Width = ColumnsDefs[aKey];

		//create a boolean formatter that shows an image
		var boolFormatter = new DSBooleanFormatter (BooleanFormatterStyle.Image);
		
		/*
			//create a bool formatter with text values for true or false
			var boolFormatter = new DSBooleanFormatter (BooleanFormatterStyle.Text, "Yes", "No");
			boolFormatter.TextAlignment = DSoft.Datatypes.Enums.TextAlignment.Middle;
		
		*/
		
		//set the size of the formatter
		boolFormatter.Size = new DSSize(20,20);
		dc2.Formatter = boolFormatter;
		
		// As DSTextFormatter can be used to set the alignment of text cells
		dc2.Formatter = new DSTextFormatter(TextAlignment.Center);
		

### Contact Us

If you have any questions, suggestions of feed back please feel free to contact us via email: gridview  at dsoftonline.com or check out our website: http://www.dsoftonline.com

## Revision History
**3.0**  

 * Added Android Support  
 * Improved support in iOS for cross platfrom datasources  
     *  Support for DSBitmap in Image cells and BooleanFormatters  
 * API Changes to use Intefaces in Event handlers
 

**2.7**

 * Added support for Xamarin.iOS Unified and 64bit
 
**2.6**  

* Added support for Auto-resizing for DSGridViewController when hosted in a UINavigationController  
    * `DisableNavigationControllerSizing` has been added to keep to the existing behaviour, which defaults to true
* Cells now update automatically when the data changes
* Retains selection when re-ordering  
* SelectItem/SelectItems now has setters
* DSDataRow now has an override-able RowId  
* Re-Fixed issue with incorrect positioning of the header columns when switching datasets
* Re-Added iOS6 support for samples
* iOS6 Bug fixes 

**2.5**  

* Fixed issue with incorrect positioning of the header columns when switching datasets 

**2.4**  

* Fixed issue with missing last row  
* Removed old DSoft.UI.Grid.dll from component


**2.3**  

* Added multi-select support  
* Added Deselection support for single and multi-select
* Added SelectedIndex, SelectedIndexes, SelectedItem and SelectedItems properties
* Fixed Crash in the Sample app

**2.2**

* Added support for custom views in cells using DSFormatter
* Added "SelectRow" to DSGridView to allow you to highlight a specific row programatically

**2.1**

* Added DSTextFormatter to allow for column text alignment to be set independently
* Added support for multi-line Column headings, just add a "/n" in between words and set your height height to an appropriate size

**2.0**

* Improved Performance which improved rendering speed and reduced memory usage and leaks
* Work on cross-platform support for future Android and Windows Phone releases
* Reworked data source mechanism
	* Can now load the data as needed rather than up front(see Getting Started for more details)
* Sorting Indicator now uses Templating on iOS7 so it can inherit the foreground colour of the header text
* Relocated shareable code to portable libraries(See upgrade notes for more detail)
* Moved theme system to portable libraries for cross-platform support(See upgrade notes for more detail)
	* Now uses shared types to provide cross-platform support
	* Extension methods provided to convert between native(UIImage, UIFont, UIColor) to DSoft types (DSImage, DSFont, DSColor)
* Updated default theme with new style
* Implemented Global and Instance themes
* Themes are automatically applied when changed, not need to call ReloadData on the grid view
* Added setters to the theme properties so values can be overridden with out the need to sub-class theme
* Added margin property to DSImageFormatter to allow for adding margins to images in the cells

**1.6**

* Fixed issue with top off setting on iOS7. No longer required to set ContentTopInset value 

**1.5**

* Fixed crash issue on iOS7 when using Fixed Header Style in the theme.
* Added helper property to DSGridViewTheme to make it easy to check for iOS 7 in your themes and applications
* Added ContentTopInset property to DSGridView to make it easier to correct iOS7 issue where the first row renders to low.  Default now adds a -44 inset when running in iOS7, which you can override with your own value using this property.

**1.4**

* Changed the datatype of the Items property of DSDataRow so that it now returns a DSDataValue object.  This is in preparation of the data entry support in a future release.
* Changed DisableColumnAutoSizing to be enabled by default.  This will improve performance out-of-the-box with large datasets.  Note: Column width calculation may be removed all together in future as it hampers performance. 

**1.3**

* Added ability to disable auto-sizing of the columns.  DSGridView now has a DisableColumnAutoSizing property to allow it to be disabled

**1.2**

* Added boolean formatter to allow for text and image styles in cells
* Fixed issue with blurry text


**1.1**

* Added support for images in cells
* Added ability to adjust the tap delay for a single tap when double tap is enabled by setting DSGridView.DoubleTapTimeout
* Fixed drawing bug which left cells from a previous dataset/table visible if the new one had less columns.
