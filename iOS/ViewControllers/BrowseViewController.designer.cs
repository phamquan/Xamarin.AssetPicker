// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AssetsPicker.iOS
{
	[Register ("ItemsViewController")]
	partial class BrowseViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton btnAddItem { get; set; }

		[Action ("AddClick:")]
		partial void AddClick (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnAddItem != null) {
				btnAddItem.Dispose ();
				btnAddItem = null;
			}
		}
	}
}
