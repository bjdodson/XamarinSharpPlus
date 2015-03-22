DSGridView is a fast lightweight and customisable data grid component for all your data presentation needs.

# DSGridView Features

* Customisable themes  
    * Can be set globally or for each instance of DSGridView   
    * Cross-platform for use across supported platforms  
    * Easy to implement your own themes or override elements of the default ones   
* Works on iOS (Classic and Unified) and Android (Beta)  
    * Windows Phone support in development  
* Cross-plafrom support for Datasources    
* High performance loading and rendering - 100,000 records as quick as 20
* Low memory footprint
* Support for standard .Net DataSet and DataTable objects
* Lightweight and simple to use
* Support for Hot-Swapping between DataTables in a DataSet
* Column sorting with themeable sort indicator
* Events for Row and Cell selection - Including Double Tap
* Works from an XIB, as a view or a view controller
* Support Text,Image, Boolean Cells and Custom views  
    * Formatters provided for Image and Boolean Cells to configure appearance  
    * Custom views can also be used in cells, including entry of data  

# Android Beta #

Android support is currently considered beta.  There may be elements that do not function as expected and performance on large datasets is, currently, not as good as it is on iOS.  

If you find any issues then please get in contact so we can get them fixed, details are in the "Contact Us" section below.

## Upgrade Notes

Version 2.0 received a number of changes that could stop your application from compiling. This is in order to provide cross-platform support for future Android and Windows Phone releases.  Please check the Getting Started section "Upgrading from 1.x to 2.x" for details of how to upgrade your existing project to 2.0.


### Upgrading from v2.x to 2.6

To fix an issue with retaining selection after sorting a column some changes have been implemented.  Please read the upgrade notes in the Getting Started guide for more information.

## Usage

Both a view and view controller are provided to get you started in as few steps as possible.

### Create DataSource

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
    

If you want to use your own collection then you will need to override the following methods, to ensure that it uses you custom collection.

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

### Create Grid View - iOS

Create the grid view as with any normal iOS View and then set the datasource of the DSGridView object.


	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	
	public override void ViewDidLoad ()
	{
		...
		
		//Create the grid view, assign the datasource and add the view as a subview
		var aGridView = new DSGridView(new RectangleF(0,0,1024,768));
		
		//assign the datasource
		aGridView.DataSource = aDataSouce;
		
		//add to view
		this.View.AddSubview(aGridView);
	        
	}

### Create Grid View - Android

Create the grid view as with any normal Android View, either in you layout file or programatically, and then set the datasource of the DSGridView object.

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
	
### Register for events

Once the grid is created you can register for the selection events


	using DSoft.Datatypes.Grid.Data;
	using DSoft.UI.Grid;
	public override void ViewDidLoad ()
	{
		...
		        
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
		aGridView.OnRowDoubleTapped += (DSGridRowView sender, DSDataRow Row) => 
		{
			//do something
		};
	        
	}


### Windows Phone

We have just completed a major piece of work to transform the existing solution in to more portable and cross-platform codebase.  This has benefits for not just us, but for you as well by making as much of your code reusable across platforms as possible.

Work on windows phone will start shortly, once Android exists beta

### Contact Us  
If you like DSGridView then please leave feedback on the Xamarin component store.

If you have any questions, suggestions or feedback please feel free to contact us via email: gridview at dsoftonline.com or check out our website: http://www.dsoftonline.com

### Trial

The trial is fully functionally however a popup message is shown on the first touch of a row and then every five subsequent touches.  The full version has this restriction removed.

Screenshots created with [PlaceIt](http://placeit.breezi.com/) and may contain simulated functionality not included in the DSGridView.
