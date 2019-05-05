using System;
using UIKit;
using Foundation;
using System.Collections.Generic;
using Photos;
using PHImageRequestID = System.Int32;
using System.Linq;
using System.Diagnostics;
using SnapKit;
using CoreGraphics;

namespace AssetsPicker.iOS
{
    #region AssetsPhotoViewController
    public partial class AssetsPhotoViewController : UIViewController
    {

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return AssetsPickerConfig.StatusBarStyle;
        }

        // MARK: Properties
        internal AssetsPickerConfig PickerConfig { get; set; }
        private UIViewControllerPreviewingDelegate Previewing { get; set; }

        private readonly string cellReuseIdentifier = new NSUuid().AsString();
        private string CellReuseIdentifier => cellReuseIdentifier;

        private readonly string footerReuseIdentifier = new NSUuid().AsString();
        private string FooterReuseIdentifier => footerReuseIdentifier;

        private Dictionary<NSIndexPath, PHImageRequestID> RequestIdMap = new Dictionary<NSIndexPath, PHImageRequestID>();
        readonly Lazy<UIBarButtonItem> CancelButtonItem;
        readonly Lazy<UIBarButtonItem> DoneButtonItem;

        private Lazy<AssetsEmptyView> emptyView;
        private AssetsEmptyView EmptyView => emptyView.Value;

        private Lazy<AssetsNoPermissionView> noPermissionView;
        private AssetsNoPermissionView NoPermissionView => noPermissionView.Value;

        private IAssetsPickerViewControllerDelegate Delegate
        {
            get
            {
                IAssetsPickerViewControllerDelegate value = null;
                if ((this.NavigationController as AssetsPickerViewController).PickerDelegate.TryGetTarget(out value))
                {

                }
                return value;
            }
        }

        private AssetsPickerViewController Picker
        {
            get
            {
                return this.NavigationController as AssetsPickerViewController;
            }
        }

        private UITapGestureRecognizer TapGesture { get; set; }
        private nfloat SyncOffsetRatio { get; set; } = -1;

        private IList<PHAsset> SelectedArray { get; set; } = new List<PHAsset>();
        private IDictionary<string, PHAsset> SelectedMap { get; set; } = new Dictionary<string, PHAsset>();

        private bool DidSetInitialPosition { get; set; } = false;

        private bool IsPortrait { get; set; } = true;

        LayoutConstraint LeadingConstraint { get; set; }
        LayoutConstraint TrailingConstraint { get; set; }

        private Lazy<UICollectionView> collectionView;

        private UICollectionView CollectionView => collectionView.Value;

        public IList<PHAsset> SelectedAssets
        {
            get
            {
                return SelectedArray;
            }
        }

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

            collectionView = new Lazy<UICollectionView>(() =>
            {
                var layout = new AssetsPhotoLayout(pickerConfig: PickerConfig);
                this.UpdateLayout(layout: layout, isPortrait: UIApplication.SharedApplication.StatusBarOrientation.IsPortrait());
                layout.ScrollDirection = UICollectionViewScrollDirection.Vertical;


                var view = new UICollectionView(frame: CGRect.Empty, layout: layout);
                view.AllowsMultipleSelection = true;
                view.AlwaysBounceVertical = true;
                view.RegisterClassForCell(this.PickerConfig.AssetCellType, this.CellReuseIdentifier);
                view.RegisterClassForSupplementaryView(typeof(AssetsPhotoFooterView), UICollectionElementKindSection.Footer, this.FooterReuseIdentifier);
                view.ContentInset = new UIEdgeInsets(top: 1, left: 0, bottom: 0, right: 0);
                view.BackgroundColor = UIColor.Clear;
                view.DataSource = this;
                view.Delegate = this;
                view.RemembersLastFocusedIndexPath = true;
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    view.PrefetchDataSource = this;
                }
                view.ShowsHorizontalScrollIndicator = false;
                view.ShowsVerticalScrollIndicator = true;

                return view;
            });

            emptyView = new Lazy<AssetsEmptyView>(() => new AssetsEmptyView());

            noPermissionView = new Lazy<AssetsNoPermissionView>(() => new AssetsNoPermissionView());
        }

        public override void LoadView()
        {
            base.LoadView();
            View = new UIView();
            View.BackgroundColor = UIColor.White;
            View.AddSubview(CollectionView);
            View.AddSubview(EmptyView);
            View.AddSubview(NoPermissionView);
            View.SetNeedsUpdateConstraints();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupCommon();
            SetupBarButtonItems();
            SetupCollectionView();

            UpdateEmptyView(0);
            UpdateNoPermissionView();

            AssetsManager.Shared.Authorize((isGranted) =>
            {
                this.UpdateNoPermissionView();
                if (isGranted)
                {
                    this.SetupAssets();
                }
                else
                {
                    this.Delegate?.CannotAccessPhotoLibrary(Picker);
                }
            });

        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (!DidSetInitialPosition)
            {
                if (PickerConfig.AssetsIsScrollToBottom)
                {
                    var count = AssetsManager.Shared.AssetArray.Count;
                    if (count > 0)
                    {
                        if (this.CollectionView.CollectionViewLayout.CollectionViewContentSize.Height > 0)
                        {
                            var lastRow = this.CollectionView.NumberOfItemsInSection(0) - 1;
                            this.CollectionView.ScrollToItem(NSIndexPath.FromRowSection(lastRow, 0), UICollectionViewScrollPosition.Bottom, false);
                        }
                    }
                }

                DidSetInitialPosition = true;
            }
        }

        public void DeselectAll()
        {
            if (CollectionView.GetIndexPathsForSelectedItems() is NSIndexPath[] indexPaths)
            {
                foreach (var indexPath in indexPaths.AsEnumerable())
                {
                    var asset = AssetsManager.Shared.AssetArray[indexPath.Row];
                    this.Deselect(asset, indexPath);
                    this.Delegate?.DidDeselect(Picker, asset, indexPath);
                }
                UpdateNavigationStatus();
                CollectionView.ReloadItems(indexPaths);
            }
        }

        public override void ViewWillTransitionToSize(CGSize size, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(size, coordinator);
            var isPortrait = size.Height > size.Width;
            var contentSize = new CGSize(size.Width, size.Height);
            if (CollectionView.CollectionViewLayout is AssetsPhotoLayout photoLayout)
            {
                if (photoLayout.TranslateOffsetForChangingSize(contentSize, CollectionView.ContentOffset) is CGPoint offset)
                {
                    photoLayout.TranslatedOffset = offset;
                    Debug.WriteLine($"translated offset: {offset}");
                }
                coordinator.AnimateAlongsideTransition((_) =>
                {

                }, (_) =>
                 {
                     photoLayout.TranslatedOffset = null;
                 });
            }
            UpdateLayout(CollectionView.CollectionViewLayout, isPortrait);
        }

    }
    #endregion

    #region Initial Setups

    partial class AssetsPhotoViewController
    {
        private void SetupCommon()
        {
            View.BackgroundColor = UIColor.White;
        }

        private void SetupBarButtonItems()
        {
            NavigationItem.LeftBarButtonItem = CancelButtonItem.Value;
            NavigationItem.RightBarButtonItem = DoneButtonItem.Value;
            DoneButtonItem.Value.Enabled = false;
        }

        private void SetupCollectionView()
        {
            CollectionView.Snap().MakeConstraints((make) =>
            {
                make.Top.EqualToSuperview();

                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    LeadingConstraint = make.Leading.EqualToSuperview().Inset(View.SafeAreaInsets.Left).Constraint.LayoutConstraints.First();
                    TrailingConstraint = make.Trailing.EqualToSuperview().Inset(View.SafeAreaInsets.Right).Constraint.LayoutConstraints.First();
                }
                else
                {
                    LeadingConstraint = make.Leading.EqualToSuperview().Constraint.LayoutConstraints.First();
                    TrailingConstraint = make.Trailing.EqualToSuperview().Constraint.LayoutConstraints.First();
                }

                make.Bottom.EqualToSuperview();
            });

            EmptyView.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });

            NoPermissionView.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });
        }

        private void SetupAssets()
        {
            var manager = AssetsManager.Shared;
            manager.Subscribe(this);
            manager.FetchAlbums();
            manager.FetchAssets(completion: (photos) =>
            {
                this.UpdateEmptyView(photos.Count);
                this.Title = this.TitleForAlbum(manager.SelectedAlbum);

                if (this.SelectedArray.Count > 0)
                {
                    this.CollectionView.PerformBatchUpdates(() =>
                    {
                        this.CollectionView.ReloadData();
                    },
                    (finished) =>
                    {
                        foreach (var asset in SelectedArray)
                        {
                            var row = photos.IndexOf(asset);
                            if (row > -1)
                            {
                                var indexPathToSelect = NSIndexPath.FromRowSection(row, 0);
                                this.CollectionView.SelectItem(indexPathToSelect, false, UICollectionViewScrollPosition.None);
                            }
                        }
                        this.UpdateSelectionCount();
                    });
                }
            });

        }
    }

    #endregion

    #region Internal APIs for UI
    partial class AssetsPhotoViewController
    {

        private void UpdateEmptyView(int count)
        {
            if (EmptyView.Hidden)
            {
                if (count == 0)
                {
                    EmptyView.Hidden = false;
                }
            }
            else
            {
                if (count > 0)
                {
                    EmptyView.Hidden = true;
                }
            }
        }

        private void UpdateNoPermissionView()
        {
            NoPermissionView.Hidden = PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized;
        }

        private void UpdateLayout(UICollectionViewLayout layout, bool? isPortrait = null)
        {
            var flowLayout = layout as UICollectionViewFlowLayout;
            if (flowLayout == null) return;

            if (isPortrait != null)
            {
                this.IsPortrait = isPortrait.Value;
            }

            flowLayout.ItemSize = this.IsPortrait ?
                PickerConfig.AssetPortraitCellSize(size: UIScreen.MainScreen.GetPortraitSize())
                : PickerConfig.AssetLandscapeCellSize(size: UIScreen.MainScreen.GetLandscapeContentSize());
            flowLayout.MinimumLineSpacing = this.IsPortrait ?
                PickerConfig.AssetPortraitLineSpace :
                PickerConfig.AssetLandscapeLineSpace;
            flowLayout.MinimumInteritemSpacing = this.IsPortrait ?
                PickerConfig.AssetPortraitInteritemSpace :
                PickerConfig.AssetLandscapeInteritemSpace;
        }

        void Select(PHAssetCollection album)
        {
            if (AssetsManager.Shared.Select(album))
            {
                if (SelectedArray.Count > 0)
                {
                    UpdateNavigationStatus();
                }
                else
                {
                    Title = TitleForAlbum(album);
                }

                CollectionView.ReloadData();

                foreach (var asset in SelectedArray)
                {
                    var index = AssetsManager.Shared.AssetArray.IndexOf(asset);
                    if (index > -1)
                    {
                        CollectionView.SelectItem(NSIndexPath.FromRowSection(index, 0), false, UICollectionViewScrollPosition.None);
                    }
                }

                if (AssetsManager.Shared.AssetArray.Count > 0)
                {
                    if (PickerConfig.AssetsIsScrollToBottom)
                    {
                        CollectionView.ScrollToItem(NSIndexPath.FromRowSection(AssetsManager.Shared.AssetArray.Count - 1, 0), UICollectionViewScrollPosition.Bottom, false);
                    }
                    else
                    {
                        CollectionView.ScrollToItem(NSIndexPath.FromRowSection(0, 0), UICollectionViewScrollPosition.Bottom, false);
                    }
                }
            }
        }

        private void Select(PHAsset asset, NSIndexPath indexPath)
        {
            if (SelectedMap.ContainsKey(asset.LocalIdentifier)) return;
            SelectedArray.Add(asset);
            SelectedMap[asset.LocalIdentifier] = asset;

            // update selected UI
            if (CollectionView.CellForItem(indexPath) is IAssetsPhotoCellProtocol photoCell)
            {
                photoCell.Count = SelectedArray.Count;
            }
        }

        private void Deselect(PHAsset asset, NSIndexPath indexPath)
        {
            if (!SelectedMap.TryGetValue(asset.LocalIdentifier, out PHAsset targetAsset))
            {
                Debug.WriteLine("Invalid status.");
                return;
            }

            var targetIndex = SelectedArray.IndexOf(targetAsset);
            if (targetIndex < 0)
            {
                Debug.WriteLine("Invalid status.");
                return;
            }
            SelectedArray.RemoveAt(targetIndex);
            SelectedMap.Remove(targetAsset.LocalIdentifier);

            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            var visibleIndexPaths = CollectionView.IndexPathsForVisibleItems;
            foreach (var visibleIndexPath in visibleIndexPaths)
            {
                if (!(AssetsManager.Shared.AssetArray.Count > visibleIndexPath.Row))
                {
                    Debug.WriteLine("Referred wrong index");
                    break;
                }
                if (SelectedMap.TryGetValue(AssetsManager.Shared.AssetArray[visibleIndexPath.Row].LocalIdentifier, out PHAsset selectedAsset) &&
                    CollectionView.CellForItem(visibleIndexPath) is IAssetsPhotoCellProtocol photoCell)
                {
                    var selectedIndex = SelectedArray.IndexOf(selectedAsset);
                    if (selectedIndex > -1)
                    {
                        photoCell.Count = selectedIndex + 1;
                    }
                }
            }
        }

        private void UpdateNavigationStatus()
        {
            DoneButtonItem.Value.Enabled = SelectedArray.Count >=
                (PickerConfig.AssetsMinimumSelectionCount > 0 ?
                PickerConfig.AssetsMinimumSelectionCount : 1);

            var counts = SelectedArray.Aggregate((0, 0), (result, asset) =>
            {
                var i = asset.MediaType == PHAssetMediaType.Image ? 1 : 0;
                var v = asset.MediaType == PHAssetMediaType.Video ? 1 : 0;
                return (result.Item1 + i, result.Item2 + v);
            });

            var imageCount = counts.Item1;
            var videoCount = counts.Item2;

            //TODO: Localize
            Title = $"{imageCount} image, {videoCount} video";
        }

        void UpdateFooter()
        {
            if (CollectionView.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Footer).Last() is AssetsPhotoFooterView footerView)
            {
                footerView.Set(AssetsManager.Shared.CountOfType(PHAssetMediaType.Image), AssetsManager.Shared.CountOfType(PHAssetMediaType.Video));
            }
            else
            {
                return;
            }
        }

        void PresentAlbumController(bool animated = true)
        {
            if (PHPhotoLibrary.AuthorizationStatus != PHAuthorizationStatus.Authorized)
            {
                return;
            }
            var navigationController = new UINavigationController();
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                navigationController.NavigationBar.PrefersLargeTitles = true;
            }
            //TODO: For Album
            var controller = new AssetsAlbumViewController(this.PickerConfig);
            controller.Delegate = this;
            navigationController.ViewControllers = new UIViewController[] { controller };

            this.NavigationController.PresentViewController(navigationController, animated, null);
        }

        string TitleForAlbum(PHAssetCollection album)
        {
            String titleString = "";
            if (album != null && album.LocalizedTitle is string albumTitle)
            {
                titleString = $"{albumTitle} ▾";
            }

            return titleString;
        }

    }
    #endregion

    #region UI Event Handlers
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

        void PressedTitle(object sender)
        {
            PresentAlbumController();
        }
    }
    #endregion

    #region UIScrollViewDelegate
    partial class AssetsPhotoViewController : IUIScrollViewDelegate
    {
        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            Debug.WriteLine($"contentOffset: {scrollView.ContentOffset}");
        }
    }
    #endregion

    #region UICollectionViewDelegate
    partial class AssetsPhotoViewController : IUICollectionViewDelegate
    {
        [Export("collectionView:shouldSelectItemAtIndexPath:")]
        public bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (this.Delegate != null)
            {
                return Delegate.ShouldSelectAsset(Picker, AssetsManager.Shared.AssetArray[indexPath.Row], indexPath);
            }
            else
            {
                return true;
            }
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var asset = AssetsManager.Shared.AssetArray[indexPath.Row];
            Select(asset: asset, indexPath: indexPath);
            UpdateNavigationStatus();
            Delegate?.DidSelectAsset(Picker, asset, indexPath);
        }

        [Export("collectionView:shouldDeselectItemAtIndexPath:")]
        public bool ShouldDeselectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (Delegate != null)
            {
                return Delegate.ShouldDeselectAssets(Picker, AssetsManager.Shared.AssetArray[indexPath.Row], indexPath);
            }
            else
            {
                return true;
            }
        }

        [Export("collectionView:didDeselectItemAtIndexPath:")]
        public void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var asset = AssetsManager.Shared.AssetArray[indexPath.Row];
            Deselect(asset, indexPath);
            UpdateNavigationStatus();
            Delegate?.DidDeselect(Picker, asset, indexPath);
        }
    }
    #endregion

    #region UICollectionViewDataSource
    partial class AssetsPhotoViewController : IUICollectionViewDataSource
    {

        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var count = AssetsManager.Shared.AssetArray.Count;
            UpdateEmptyView(count);
            return count;
        }



        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(CellReuseIdentifier, indexPath) as UICollectionViewCell;
            try
            {

                if (cell is IAssetsPhotoCellProtocol photoCell)
                {
                    photoCell.IsVideo = AssetsManager.Shared.AssetArray[indexPath.Row].MediaType == PHAssetMediaType.Video;
                    cell.NeedsUpdateConstraints();
                    cell.UpdateConstraintsIfNeeded();
                }
                return cell;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
            return cell;
        }

        [Export("collectionView:willDisplayCell:forItemAtIndexPath:")]
        public void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            try
            {
                var photoCell = cell as IAssetsPhotoCellProtocol;
                if (cell == null) return;

                var asset = AssetsManager.Shared.AssetArray[indexPath.Row];

                photoCell.Asset = asset;
                photoCell.IsVideo = asset.MediaType == PHAssetMediaType.Video;
                if (photoCell.IsVideo)
                {
                    photoCell.Duration = asset.Duration;
                }

                if (SelectedMap.TryGetValue(asset.LocalIdentifier, out PHAsset selectedAsset))
                {
                    // update cell UI as selected
                    var targetIndex = SelectedArray.IndexOf(selectedAsset);
                    if (targetIndex > -1)
                    {
                        photoCell.Count = targetIndex + 1;
                    }
                }


                CancelFetching(indexPath);

                var requestId = AssetsManager.Shared.ImageAt(indexPath.Row, PickerConfig.AssetCacheSize, false, (image, isDegraded) =>
                {
                    if (this.IsFetching(indexPath))
                    {
                        if (!isDegraded)
                        {
                            this.RemoveFetching(indexPath);
                        }

                        UIView.Transition(photoCell.ImageView,
                                          0.125,
                                          UIViewAnimationOptions.TransitionCrossDissolve,
                                          () => photoCell.ImageView.Image = image,
                                          null);

                    }
                });

                RegisterFetching(requestId, indexPath);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [Export("collectionView:didEndDisplayingCell:forItemAtIndexPath:")]
        public void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            CancelFetching(indexPath);
        }
    }

    //[Export("collectionView:viewForSupplementaryElementOfKind:atIndexPath:")]
    //public UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
    //{
    //    //throw new System.NotImplementedException();
    //}

    #endregion

    #region Image Fetch Utility

    partial class AssetsPhotoViewController
    {

        void CancelFetching(NSIndexPath indexPath)
        {
            if (RequestIdMap.TryGetValue(indexPath, out PHImageRequestID requestId))
            {
                RequestIdMap.Remove(indexPath);
                AssetsManager.Shared.CancelRequest(requestId);
            }
        }

        void RegisterFetching(PHImageRequestID requestId, NSIndexPath indexPath)
        {
            RequestIdMap[indexPath] = requestId;
        }

        void RemoveFetching(NSIndexPath indexPath)
        {
            if (RequestIdMap.ContainsKey(indexPath))
            {
                RequestIdMap.Remove(indexPath);
            }
        }

        bool IsFetching(NSIndexPath indexPath)
        {
            if (RequestIdMap.TryGetValue(indexPath, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    #endregion

    #region UICollectionViewDelegateFlowLayout

    partial class AssetsPhotoViewController : IUICollectionViewDelegateFlowLayout
    {
        [Export("collectionView:layout:referenceSizeForFooterInSection:")]
        public CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            if (collectionView.NumberOfSections() - 1 == section)
            {
                if (collectionView.Bounds.Width > collectionView.Bounds.Height)
                {
                    return new CGSize(collectionView.Bounds.Width, PickerConfig.AssetLandscapeCellSize(collectionView.Bounds.Size).Width * 2 / 3);
                }
                else
                {
                    return new CGSize(collectionView.Bounds.Width, PickerConfig.AssetPortraitCellSize(collectionView.Bounds.Size).Width * 2 / 3);
                }
            }
            else
            {
                return CGSize.Empty;
            }
        }
    }

    #endregion

    #region UICollectionViewDataSourcePrefetching

    partial class AssetsPhotoViewController : IUICollectionViewDataSourcePrefetching
    {
        public void PrefetchItems(UICollectionView collectionView, NSIndexPath[] indexPaths)
        {
            var assets = new List<PHAsset>();
            foreach (var indexPath in indexPaths)
            {
                assets.Add(AssetsManager.Shared.AssetArray[indexPath.Row]);
            }
            AssetsManager.Shared.Cache(assets.ToArray(), PickerConfig.AssetCacheSize);
        }
    }

    #endregion

    #region AssetsAlbumViewControllerDelegate

    partial class AssetsPhotoViewController : IAssetsAlbumViewControllerDelegate
    {
        public void DidCancel(AssetsAlbumViewController controller)
        {
            Debug.WriteLine("Cancelled.");
        }

        public void DidSelectAlbum(AssetsAlbumViewController controller, PHAssetCollection album)
        {
            Select(album);
        }
    }

    #endregion

    #region AssetsManagerDelegate

    partial class AssetsPhotoViewController : IAssetsManagerDelegate
    {
        public void AuthorizationStatusChanged(AssetsManager manager, PHAuthorizationStatus oldStatus, PHAuthorizationStatus newStatus)
        {
            if (oldStatus != PHAuthorizationStatus.Authorized)
            {
                if (newStatus == PHAuthorizationStatus.Authorized)
                {
                    UpdateNoPermissionView();
                    AssetsManager.Shared.FetchAssets(true, (_) =>
                    {
                        this.CollectionView.ReloadData();
                    });
                }
            }
            else
            {
                UpdateNoPermissionView();
            }
        }

        public void InsertedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths) { }

        public void InsertedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths)
        {
            var indexPathDescription = String.Join(",", indexPaths.Select(_ => $"[{_.Section}, {_.Row}]"));
            Debug.WriteLine($"insertedAssets at: {indexPathDescription}");
            CollectionView.InsertItems(indexPaths);
            UpdateFooter();
        }

        public void ReloadedAlbum(AssetsManager manager, PHAssetCollection album, NSIndexPath indexPath) { }

        public void ReloadedAlbumsInSection(AssetsManager manager, int section) { }

        public void RemovedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths)
        {
            throw new NotImplementedException();
        }

        public void RemovedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths)
        {
            throw new NotImplementedException();
        }

        public void UpdatedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths)
        {
            throw new NotImplementedException();
        }

        public void UpdatedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

}
