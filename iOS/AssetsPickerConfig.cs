using System;
using System.Collections;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace AssetsPicker.iOS
{
    public class AssetsPickerConfig
    {
        public static UIStatusBarStyle StatusBarStyle { get; set; } = UIStatusBarStyle.Default;

        public static UIColor DefaultCheckmarkColor { get; set; } = new UIColor(red: 0.078f, green: 0.435f, blue: 0.875f, alpha: 1.0f);

        //    /// Set selected album at initial load.
        public PHAssetCollectionSubtype albumDefaultType { get; set; } = PHAssetCollectionSubtype.SmartAlbumUserLibrary;
        //    /// true: shows empty albums, false: hides empty albums
        public bool AlbumIsShowEmptyAlbum { get; set; } = true;
        //    /// true: shows "Hidden" album, false: hides "Hidden" album
        public bool AlbumIsShowHiddenAlbum { get; set; } = false;
        //    /// Customize your own album list by providing filter block below.
        public IDictionary<PHAssetCollectionType, Func<PHAssetCollection, PHFetchResult, bool>> AlbumFilter { get; set; }

        //    /// Not yet fully implemeted, do not set this true until it's completed.
        public bool AlbumIsShowMomentAlbums { get; set; } = false;

        //    // MARK: Fetch
        public IDictionary<PHAssetCollectionType, PHFetchOptions> AlbumFetchOptions { get; set; }

        public Func<PHAssetCollectionType, (PHAssetCollection, PHFetchResult), (PHAssetCollection, PHFetchResult), bool> AlbumComparator { get; set; }

        //    // MARK: Order
        //    /// by giving this comparator, albumFetchOptions going to be useless
        //    open var albumComparator: ((PHAssetCollectionType, (album: PHAssetCollection, result: PHFetchResult<PHAsset>), (album: PHAssetCollection, result: PHFetchResult<PHAsset>)) -> Bool)?

        //    // MARK: Cache
        private CGSize _albumCacheSize = CGSize.Empty;


        public CGSize? AlbumForcedCacheSize { get; set; }
        public CGSize AlbumCacheSize
        {
            get
            {
                if (this.AlbumForcedCacheSize is CGSize forcedCacheSize)
                {
                    return forcedCacheSize;
                }
                else return _albumCacheSize;
            }
        }


        //    // MARK: Custom Layout
        public Type AlbumCellType { get; set; } = typeof(AssetsAlbumCell);
        public nfloat AlbumDefaultSpace { get; set; } = 20;
        public nfloat AlbumLineSpace { get; set; } = -1;

        private readonly int albumPortraitDefaultColumnCount = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 3 : 2;
        public int AlbumPortraitDefaultColumnCount { get => albumPortraitDefaultColumnCount; }
        public int? AlbumPortraitColumnCount { get; set; }
        public nfloat? AlbumPortraitForcedCellWidth { get; set; }
        public nfloat? AlbumPortraitForcedCellHeight { get; set; }
        public CGSize AlbumPortraitCellSize { get; set; } = CGSize.Empty;

        private readonly int albumLandscapeDefaultColumnCount = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 4 : 3;
        public int AlbumLandscapeDefaultColumnCount { get => albumLandscapeDefaultColumnCount; }
        public int? AlbumLandscapeColumnCount { get; set; }
        public nfloat? AlbumLandscapeForcedCellWidth { get; set; }
        public nfloat? AlbumLandscapeForcedCellHeight { get; set; }
        public CGSize AlbumLandscapeCellSize { get; set; } = CGSize.Empty;

        nfloat AlbumItemSpace(bool isPortrait)
        {
            var size = isPortrait ? UIScreen.MainScreen.GetPortraitSize() : UIScreen.MainScreen.GetLandscapeSize();
            var count = isPortrait ? (AlbumPortraitColumnCount ?? albumPortraitDefaultColumnCount) : AlbumLandscapeColumnCount ?? albumLandscapeDefaultColumnCount;
            var albumCellSize = isPortrait ? AlbumPortraitCellSize : AlbumLandscapeCellSize;
            var space = (size.Width - count * albumCellSize.Width) / (count + 1);
            return space;
        }

        //    // MARK: - Asset Config

        //    // MARK: Asset
        public IList<PHAsset> selectedAssets;
        public int AssetsMinimumSelectionCount { get; set; } = 1;
        public bool AssetsIsScrollToBottom { get; set; } = true;

        //    // MARK: Fetch
        public IDictionary<PHAssetCollectionType, PHFetchOptions> AssetFetchOptions { get; set; }

        //    // MARK: Custom Layout
        public Type AssetCellType { get; set; } = typeof(AssetsPhotoCell);
        private CGSize _assetCacheSize = CGSize.Empty;
        public CGSize? assetForcedCacheSize;
        public CGSize AssetCacheSize
        {
            get
            {
                if (assetForcedCacheSize is CGSize forcedCacheSize)
                {
                    return forcedCacheSize;
                }
                else
                {
                    return _assetCacheSize;
                }
            }
        }
        public int AssetPortraitColumnCount { get; set; } = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 5 : 4;
        public nfloat AssetPortraitInteritemSpace { get; set; } = 1;
        public nfloat AssetPortraitLineSpace { get; set; } = 1;

        CGSize AssetPortraitCellSize(CGSize size)
        {
            var count = this.AssetPortraitColumnCount;
            var edge = (size.Width - (count - 1) * this.AssetPortraitInteritemSpace) / count;
            return new CGSize(width: edge, height: edge);
        }

        public int AssetLandscapeColumnCount { get; set; } = 7;
        public nfloat AssetLandscapeInteritemSpace { get; set; } = 1.5f;
        public nfloat AssetLandscapeLineSpace { get; set; } = 1.5f;

        CGSize AssetLandscapeCellSize(CGSize size)
        {
            var count = this.AssetLandscapeColumnCount;
            var edge = (size.Width - (count - 1) * this.AssetLandscapeInteritemSpace) / count;
            return new CGSize(width: edge, height: edge);
        }

        AssetsPickerConfig()
        {

        }

        //@discardableResult
        AssetsPickerConfig Prepare()
        {

            var scale = UIScreen.MainScreen.Scale;

            /* initialize album attributes */

            // album line space
            if (AlbumLineSpace < 0)
            {
                AlbumLineSpace = AlbumDefaultSpace;
            }

            //        // initialize album cell size
            var albumPortraitCount = AlbumPortraitColumnCount ?? albumPortraitDefaultColumnCount;
            var albumPortraitWidth = (UIScreen.MainScreen.GetPortraitSize().Width - AlbumDefaultSpace * (albumPortraitCount + 1)) / albumPortraitCount;
            AlbumPortraitCellSize = new CGSize(
                width: AlbumPortraitForcedCellWidth ?? albumPortraitWidth,
                height: AlbumPortraitForcedCellHeight ?? albumPortraitWidth * 1.25
            );


            var albumLandscapeCount = AlbumLandscapeColumnCount ?? albumLandscapeDefaultColumnCount;
            nfloat albumLandscapeWidth = 0;
            if (AlbumPortraitColumnCount != null) {
                albumLandscapeWidth = (UIScreen.MainScreen.GetLandscapeSize().Width - AlbumDefaultSpace * (albumLandscapeCount + 1)) / albumLandscapeCount;
            } else {
                albumLandscapeWidth = albumPortraitWidth;
            }
            AlbumLandscapeCellSize = new CGSize(
                width: AlbumLandscapeForcedCellWidth ?? albumLandscapeWidth,
                height: AlbumLandscapeForcedCellHeight ?? albumLandscapeWidth * 1.25
            );

            // initialize cache size for album thumbnail
            _albumCacheSize = new CGSize(width: albumPortraitWidth * scale, height: albumPortraitWidth * scale);


            //        /* initialize asset attributes */

            //// initialize cache size for asset thumbnail
            var assetPivotCount = AssetPortraitColumnCount;
            var assetWidth = (UIScreen.MainScreen.GetPortraitSize().Width - (assetPivotCount - 1) * AssetPortraitInteritemSpace) / assetPivotCount;


            _assetCacheSize = new CGSize(width: assetWidth * scale, height: assetWidth * scale);

            // asset fetch options by default
            if (AssetFetchOptions == null) {
                var options = new PHFetchOptions();
                options.IncludeHiddenAssets = AlbumIsShowHiddenAlbum;
                options.SortDescriptors = new NSSortDescriptor[] {
                    new NSSortDescriptor(key: "creationDate", ascending: true),
                    new NSSortDescriptor(key: "modificationDate", ascending: true)
                };
                options.Predicate = NSPredicate.FromFormat($"mediaType = {PHAssetMediaType.Image} OR mediaType = {PHAssetMediaType.Video}");
                AssetFetchOptions = new Dictionary<PHAssetCollectionType, PHFetchOptions>() {
                    { PHAssetCollectionType.SmartAlbum, options },
                    { PHAssetCollectionType.Album, options },
                    { PHAssetCollectionType.Moment, options }
                };
            }

            return this;
        }
    }
}
