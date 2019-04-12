using System;
using System.Collections;
using System.Collections.Generic;
using CoreGraphics;
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


        public CGSize AlbumForcedCacheSize { get; set; }
        public CGSize AlbumCacheSize
        {
            get
            {
                if (this.AlbumForcedCacheSize != null) return AlbumForcedCacheSize;
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

        nfloat AlbumItemSpace(bool isPortrait) {
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
        public Type assetCellType { get; set; } = typeof(AssetsPhotoCell);
        //    private var _assetCacheSize: CGSize = .zero
        //    open var assetForcedCacheSize: CGSize?
        //    open var assetCacheSize: CGSize {
        //        if let forcedCacheSize = self.assetForcedCacheSize {
        //            return forcedCacheSize
        //        } else {
        //            return _assetCacheSize
        //        }
        //    }
        //    open var assetPortraitColumnCount: Int = UI_USER_INTERFACE_IDIOM() == .pad? 5 : 4
        //    open var assetPortraitInteritemSpace: CGFloat = 1
        //    open var assetPortraitLineSpace: CGFloat = 1

        //    func assetPortraitCellSize(forViewSize size: CGSize) -> CGSize {
        //        let count = CGFloat(self.assetPortraitColumnCount)
        //        let edge = (size.width - (count - 1) * self.assetPortraitInteritemSpace) / count
        //        return CGSize(width: edge, height: edge)
        //    }

        //    open var assetLandscapeColumnCount: Int = 7
        //    open var assetLandscapeInteritemSpace: CGFloat = 1.5
        //    open var assetLandscapeLineSpace: CGFloat = 1.5

        //    func assetLandscapeCellSize(forViewSize size: CGSize) -> CGSize {
        //        let count = CGFloat(self.assetLandscapeColumnCount)
        //        let edge = (size.width - (count - 1) * self.assetLandscapeInteritemSpace) / count
        //        return CGSize(width: edge, height: edge)
        //    }

        AssetsPickerConfig()
        {

        }

        //@discardableResult
        //open func prepare() -> Self {

        //        let scale = UIScreen.main.scale

        //        /* initialize album attributes */

        //        // album line space
        //        if albumLineSpace< 0 {
        //            albumLineSpace = albumDefaultSpace
        //        }

        //        // initialize album cell size
        //        let albumPortraitCount = CGFloat(albumPortraitColumnCount ?? albumPortraitDefaultColumnCount)
        //        let albumPortraitWidth = (UIScreen.main.portraitSize.width - albumDefaultSpace * (albumPortraitCount + 1)) / albumPortraitCount
        //        albumPortraitCellSize = CGSize(
        //            width: albumPortraitForcedCellWidth ?? albumPortraitWidth,
        //            height: albumPortraitForcedCellHeight ?? albumPortraitWidth* 1.25
        //        )


        //        let albumLandscapeCount = CGFloat(albumLandscapeColumnCount ?? albumLandscapeDefaultColumnCount)
        //        var albumLandscapeWidth: CGFloat = 0
        //        if let _ = albumPortraitColumnCount {
        //            albumLandscapeWidth = (UIScreen.main.landscapeSize.width - albumDefaultSpace* (albumLandscapeCount + 1)) / albumLandscapeCount
        //        } else {
        //            albumLandscapeWidth = albumPortraitWidth
        //        }
        //        albumLandscapeCellSize = CGSize(
        //            width: albumLandscapeForcedCellWidth ?? albumLandscapeWidth,
        //            height: albumLandscapeForcedCellHeight ?? albumLandscapeWidth* 1.25
        //        )

        //        // initialize cache size for album thumbnail
        //_albumCacheSize = CGSize(width: albumPortraitWidth* scale, height: albumPortraitWidth* scale)


        //        /* initialize asset attributes */

        //// initialize cache size for asset thumbnail
        //let assetPivotCount = CGFloat(assetPortraitColumnCount)
        //    let assetWidth = (UIScreen.main.portraitSize.width - (assetPivotCount - 1) * assetPortraitInteritemSpace) / assetPivotCount


        //    _assetCacheSize = CGSize(width: assetWidth* scale, height: assetWidth* scale)

        //    // asset fetch options by default
        //    if assetFetchOptions == nil {
        //        let options = PHFetchOptions()
        //        options.includeHiddenAssets = albumIsShowHiddenAlbum
        //        options.sortDescriptors = [
        //            NSSortDescriptor(key: "creationDate", ascending: true),
        //            NSSortDescriptor(key: "modificationDate", ascending: true)
        //        ]
        //        options.predicate = NSPredicate(format: "mediaType = %d OR mediaType = %d", PHAssetMediaType.image.rawValue, PHAssetMediaType.video.rawValue)
        //        assetFetchOptions = [
        //            .smartAlbum: options,
        //            .album: options,
        //            .moment: options
        //        ]
        //    }

        //    return self
        //}
    }
}
