using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using Photos;
using SnapKit;
using UIKit;

namespace AssetsPicker.iOS
{

    public interface IAssetsAlbumViewControllerDelegate
    {
        void DidCancel(AssetsAlbumViewController controller);
        void DidSelectAlbum(AssetsAlbumViewController controller, PHAssetCollection album);
    }

    public partial class AssetsAlbumViewController : UIViewController
    {

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return AssetsPickerConfig.StatusBarStyle;
        }

        public IAssetsAlbumViewControllerDelegate Delegate { get; internal set; }

        public AssetsPickerConfig PickerConfig { get; }
        private string CellReuseIdentifier { get; } = new NSUuid().AsString();
        private string HeaderReuseIdentifier { get; } = new NSUuid().AsString();

        readonly Lazy<UIBarButtonItem> cancelButtonItem;
        UIBarButtonItem CancelButtonItem => cancelButtonItem.Value;

        readonly Lazy<UIBarButtonItem> searchButtonItem;
        UIBarButtonItem SearchButtonItem => searchButtonItem.Value;

        readonly Lazy<UICollectionView> collectionView;
        public UICollectionView CollectionView => collectionView.Value;

        public AssetsAlbumViewController(AssetsPickerConfig pickerConfig) : base(null, null)
        {
            PickerConfig = pickerConfig;

            cancelButtonItem = new Lazy<UIBarButtonItem>(() =>
            {
                var buttonItem = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Plain, PressedCancel);
                return buttonItem;
            });

            searchButtonItem = new Lazy<UIBarButtonItem>(() =>
            {
                var buttonItem = new UIBarButtonItem(UIBarButtonSystemItem.Search, PressedSearch);
                return buttonItem;
            });

            collectionView = new Lazy<UICollectionView>(() =>
            {

                var isPortrait = UIApplication.SharedApplication.StatusBarOrientation.IsPortrait();

                var layout = new AssetsAlbumLayout();
                this.UpdateLayout(layout: layout, isPortrait: UIApplication.SharedApplication.StatusBarOrientation.IsPortrait());
                layout.ScrollDirection = UICollectionViewScrollDirection.Vertical;

                var defaultSpace = this.PickerConfig.AlbumDefaultSpace;
                var itemSpace = this.PickerConfig.AlbumItemSpace(isPortrait: isPortrait);

                var view = new UICollectionView(frame: this.View.Bounds, layout: layout);
                view.RegisterClassForCell(this.PickerConfig.AlbumCellType, this.CellReuseIdentifier);
                view.RegisterClassForSupplementaryView(typeof(AssetsAlbumHeaderView), UICollectionElementKindSection.Header, this.HeaderReuseIdentifier);
                view.ContentInset = new UIEdgeInsets(top: defaultSpace, left: itemSpace, bottom: defaultSpace, right: itemSpace);
                view.BackgroundColor = UIColor.Clear;
                view.DataSource = this;
                view.Delegate = this;
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    view.PrefetchDataSource = this;
                }
                view.ShowsHorizontalScrollIndicator = false;
                view.ShowsVerticalScrollIndicator = true;

                return view;
            });
        }


        public override void LoadView()
        {
            base.LoadView();
            View = new UIView();
            View.BackgroundColor = UIColor.White;


            View.AddSubview(CollectionView);
            View.SetNeedsUpdateConstraints();


            AssetsManager.Shared.Subscribe(subscriber: this);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetupCommon();
            SetupBarButtonItems();


            CollectionView.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });

            AssetsManager.Shared.Authorize((isAuthorized) =>
            {
                if (isAuthorized)
                {
                    AssetsManager.Shared.FetchAlbums(completion: (_) =>
                    {
                        this.CollectionView.ReloadData();
                    });
                }
                else
                {
                    this.DismissViewController(true, null);
                }
            });
        }

        public override void ViewWillTransitionToSize(CGSize size, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(size, coordinator);

            coordinator.AnimateAlongsideTransitionInView(CollectionView, (context) =>
            {
                var isPortrait = size.Height > size.Width;
                var space = this.PickerConfig.AlbumItemSpace(isPortrait: isPortrait);
                var insets = this.CollectionView.ContentInset;
                this.CollectionView.ContentInset = new UIEdgeInsets(top: insets.Top, left: space, bottom: insets.Bottom, right: space);
                this.UpdateLayout(layout: this.CollectionView.CollectionViewLayout, isPortrait: isPortrait);
            }, (_) => { });
        }

    }

    #region Internal APIs for UI

    public partial class AssetsAlbumViewController
    {
        void SetupCommon()
        {
            Title = AppResources.Title_Albums;
            View.BackgroundColor = UIColor.White;
        }

        void SetupBarButtonItems()
        {
            NavigationItem.LeftBarButtonItem = CancelButtonItem;
            //navigationItem.rightBarButtonItem = searchButtonItem
        }

        private void UpdateLayout(UICollectionViewLayout layout, bool isPortrait)
        {
            var flowLayout = layout as UICollectionViewFlowLayout;
            if (flowLayout == null) return;

            flowLayout.ItemSize = isPortrait ?
                PickerConfig.AlbumPortraitCellSize
                : PickerConfig.AlbumLandscapeCellSize;
            flowLayout.MinimumLineSpacing = PickerConfig.AlbumLineSpace;
            flowLayout.MinimumInteritemSpacing = PickerConfig.AlbumItemSpace(isPortrait);
        }
    }

    #endregion

    #region UICollectionViewDelegate

    partial class AssetsAlbumViewController : IUICollectionViewDelegate
    {
        [Export("collectionView:didSelectItemAtIndexPath:")]
        public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            DismissViewController(true, () =>
            {
                AssetsManager.Shared.Unsubscribe(this);
            });

            Delegate?.DidSelectAlbum(this, AssetsManager.Shared.AlbumAt(indexPath));
        }
    }

    #endregion

    #region UICollectionViewDataSource

    partial class AssetsAlbumViewController : IUICollectionViewDataSource
    {

        [Export("numberOfSectionsInCollectionView:")]
        public nint NumberOfSections(UICollectionView collectionView)
        {
            var count = AssetsManager.Shared.NumberOfSections;
            Debug.WriteLine("${count}");
            return count;
        }

        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var count = AssetsManager.Shared.NumberOfAlbumsInSection((int)section);
            Debug.WriteLine($"numberOfItemsInSection[${section}]: ${count}");
            return count;
        }

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(CellReuseIdentifier, indexPath) as UICollectionViewCell;
            try
            {

                if (cell is IAssetsAlbumCellProtocol)
                {
                    cell.NeedsUpdateConstraints();
                    cell.UpdateConstraintsIfNeeded();
                }
                return cell;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
            return cell;
        }

        //[Export("collectionView:viewForSupplementaryElementOfKind:atIndexPath:")]
        //public UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        //{
        //    throw new System.NotImplementedException();
        //}

        [Export("collectionView:willDisplayCell:forItemAtIndexPath:")]
        public void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            //logi("willDisplay[\(indexPath.section)][\(indexPath.row)]")
            if (!(cell is IAssetsAlbumCellProtocol albumCell))
            {
                Debug.WriteLine("Failed to cast UICollectionViewCell.");
                return;
            }
            albumCell.Album = AssetsManager.Shared.AlbumAt(indexPath);
            albumCell.TitleText = AssetsManager.Shared.TitleAt(indexPath);
            albumCell.Count = AssetsManager.Shared.NumberOfAssets(indexPath);

            AssetsManager.Shared.ImageOfAlbumAt(indexPath: indexPath, size: PickerConfig.AlbumCacheSize, isNeedDegraded: true,
            completion: (image) =>
            {
                if (image != null)
                {
                    //logi("imageSize[\(indexPath.section)][\(indexPath.row)]: \(image.size)")
                    if (albumCell.ImageView.Image != null)
                    {
                        UIView.Transition(
                            withView: albumCell.ImageView,
                            duration: 0.20f,
                            options: UIViewAnimationOptions.TransitionCrossDissolve,
                            animation: () =>
                            {
                                albumCell.ImageView.Image = image;
                            }, completion: null);
                    }
                    else
                    {
                        albumCell.ImageView.Image = image;
                    }
                }
                else
                {
                    albumCell.ImageView.Image = null;
                }
            });
        }
    }

    public partial class AssetsAlbumViewController : IUICollectionViewDataSourcePrefetching
    {
        public void PrefetchItems(UICollectionView collectionView, NSIndexPath[] indexPaths)
        {
            var assets = new List<PHAsset>();
            foreach (var albumIndexPath in indexPaths)
            {
                var album = AssetsManager.Shared.AlbumAt(albumIndexPath);
                if (AssetsManager.Shared.FetchResultFor(album)?.LastObject is PHAsset asset)
                {
                    assets.Add(asset);
                }
            }
            if (assets.Count > 0)
            {
                AssetsManager.Shared.Cache(assets: assets.ToArray(), size: PickerConfig.AlbumCacheSize);
                //logi("Caching album images at \(indexPaths)")
            }
        }
    }

    #endregion

    #region UI Event Handlers

    public partial class AssetsAlbumViewController
    {
        void PressedCancel(object sender, EventArgs e)
        {
            this.NavigationController.DismissViewController(animated: true, completionHandler: () =>
            {
                AssetsManager.Shared.Unsubscribe(this);
            });

            this.Delegate.DidCancel(controller: this);
        }

        void PressedSearch(object sender, EventArgs e)
        {

        }
    }

    #endregion

    #region sss

    partial class AssetsAlbumViewController : IAssetsManagerDelegate
    {
        public void AuthorizationStatusChanged(AssetsManager manager, PHAuthorizationStatus oldStatus, PHAuthorizationStatus newStatus) { }

        public void InsertedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths)
        {
            CollectionView.InsertItems(indexPaths);
        }

        public void InsertedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths) { }

        public void ReloadedAlbum(AssetsManager manager, PHAssetCollection album, NSIndexPath indexPath)
        {
            CollectionView.ReloadSections(NSIndexSet.FromIndex(indexPath.Section));
        }

        public void ReloadedAlbumsInSection(AssetsManager manager, int section)
        {
            CollectionView.ReloadSections(NSIndexSet.FromIndex(section));
        }

        public void RemovedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths)
        {
            CollectionView.DeleteItems(indexPaths);
        }

        public void RemovedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths) { }

        public void UpdatedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths)
        {
            CollectionView.ReloadItems(indexPaths);
        }

        public void UpdatedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths) { }
    }

    #endregion

}
