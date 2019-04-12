using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using Photos;
using PHImageRequestID = System.Int32;
using System.Linq;
using System.Diagnostics;

namespace AssetsPicker.iOS
{
    public partial class AssetsPhotoViewController : UIViewController
    {

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return AssetsPickerConfig.StatusBarStyle;
        }

        // MARK: Properties
        AssetsPickerConfig PickerConfig { get; set; }
        private UIViewControllerPreviewingDelegate Previewing { get; set; }

        private string CellReuseIdentifier => new NSUuid().AsString();
        private string FooterReuseIdentifier => new NSUuid().AsString();

        private Dictionary<NSIndexPath, PHImageRequestID> RequestIdMap;
        readonly Lazy<UIBarButtonItem> CancelButtonItem;
        readonly Lazy<UIBarButtonItem> DoneButtonItem;

        public AssetsPhotoViewController()
        {
            CancelButtonItem = new Lazy<UIBarButtonItem>(() =>
            {
                var buttonItem = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Plain, PressedCancel);
                return buttonItem;
            });

            DoneButtonItem = new Lazy<UIBarButtonItem>(() =>
            {
                var buttonItem = new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, PressedDone);
                return buttonItem;
            });
        }

        private IAssetsPickerViewControllerDelegate Delegate
        {
            get
            {
                IAssetsPickerViewControllerDelegate value = null;
                if ((this.NavigationController as AssetsPickerViewController).PickerDelegate.TryGetTarget(out value)) {

                }
                return value;
            }
        }

        private AssetsPickerViewController Picker
        {
            get
            {
                IAssetsPickerViewControllerDelegate value = null;
                return this.NavigationController as AssetsPickerViewController;
            }
        }

        private UITapGestureRecognizer TapGesture { get; set; }
        private nfloat SyncOffsetRatio { get; set; } = -1;

        private IList<PHAsset> SelectedArray { get; set; } = new List<PHAsset>();



    }

    partial class AssetsPhotoViewController
    {
        void PressedCancel(object sender, EventArgs e)
        {
            this.NavigationController.DismissViewController(animated: true, completionHandler: () =>
            {
                this.Delegate.DidDismissByCancelling(controller: this.Picker, byCancel: true);
            });

            this.Delegate.DidCancel(controller: Picker);
        }

        void PressedDone(object sender, EventArgs e)
        {
            this.NavigationController.DismissViewController(animated: true, completionHandler: () =>
            {
                this.Delegate.DidDismissByCancelling(controller: this.Picker, byCancel: false);
            });

            this.Delegate.SelectedAssets(this.Picker, this.SelectedArray.ToArray());
        }
    }

    partial class AssetsPhotoViewController : IUIScrollViewDelegate
    {
        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            Debug.WriteLine($"contentOffset: {scrollView.ContentOffset}");
        }
    }

    partial class AssetsPhotoViewController : IUICollectionViewDelegate
    {
        [Export("collectionView:shouldSelectItemAtIndexPath:")]
        public bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            throw new System.NotImplementedException();
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            throw new System.NotImplementedException();
        }

        [Export("collectionView:shouldDeselectItemAtIndexPath:")]
        public bool ShouldDeselectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            throw new System.NotImplementedException();
        }

        [Export("collectionView:didDeselectItemAtIndexPath:")]
        public void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            throw new System.NotImplementedException();
        }
    }

    partial class AssetsPhotoViewController : IUICollectionViewDataSource
    {

        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            throw new NotImplementedException();
        }

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            throw new NotImplementedException();
        }


    }


}
