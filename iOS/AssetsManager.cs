using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;
using PHImageRequestID = System.Int32;

namespace AssetsPicker.iOS
{

    public interface IAssetsManagerDelegate
    {
        void AuthorizationStatusChanged(AssetsManager manager, PHAuthorizationStatus oldStatus, PHAuthorizationStatus newStatus);
        void ReloadedAlbumsInSection(AssetsManager manager, int section);


        void InsertedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths);
        void RemovedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths);
        void UpdatedAlbums(AssetsManager manager, PHAssetCollection[] albums, NSIndexPath[] indexPaths);


        void ReloadedAlbum(AssetsManager manager, PHAssetCollection album, NSIndexPath indexPath);
        void InsertedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths);
        void RemovedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths);
        void UpdatedAssets(AssetsManager manager, PHAsset[] assets, NSIndexPath[] indexPaths);
    }

    public partial class AssetsManager : NSObject
    {

        public static AssetsManager Shared => shared;
        private readonly static AssetsManager shared = new AssetsManager();

        private AssetsPickerConfig pickerConfig;

        public AssetsPickerConfig PickerConfig
        {
            get => pickerConfig;
            set
            {
                pickerConfig = value;
                IsFetchedAlbums = false;
            }
        }

        public bool IsFetchedAlbums { get; set; } = false;

        public PHCachingImageManager ImageManager => imageManager;
        public PHAuthorizationStatus AuthorizationStatus { get; set; } = PHPhotoLibrary.AuthorizationStatus;
        public IList<IAssetsManagerDelegate> Subscribers { get => subscribers; set => subscribers = value; }
        public IDictionary<string, PHAssetCollection> AlbumMap { get => albumMap; set => albumMap = value; }
        public IList<PHFetchResult> AlbumsFetchArray { get => albumsFetchArray; set => albumsFetchArray = value; }
        public IDictionary<string, PHFetchResult> FetchMap { get => fetchMap; set => fetchMap = value; }
        public IList<IList<PHAssetCollection>> FetchedAlbumsArray { get => fetchedAlbumsArray; set => fetchedAlbumsArray = value; }
        public IList<IList<PHAssetCollection>> SortedAlbumsArray { get => sortedAlbumsArray; set => sortedAlbumsArray = value; }
        public List<PHAsset> AssetArray { get => assetArray; private set => assetArray = value; }

        public PHAssetCollection DefaultAlbum { get; private set; }
        public PHAssetCollection CameraRollAlbum { get; private set; }
        public PHAssetCollection SelectedAlbum { get; private set; }

        private readonly PHCachingImageManager imageManager = new PHCachingImageManager();
        IList<IAssetsManagerDelegate> subscribers = new List<IAssetsManagerDelegate>();
        IDictionary<string, PHAssetCollection> albumMap = new Dictionary<string, PHAssetCollection>();
        IList<PHFetchResult> albumsFetchArray = new List<PHFetchResult>();
        IDictionary<string, PHFetchResult> fetchMap = new Dictionary<string, PHFetchResult>();
        IList<IList<PHAssetCollection>> fetchedAlbumsArray = new List<IList<PHAssetCollection>>();
        IList<IList<PHAssetCollection>> sortedAlbumsArray = new List<IList<PHAssetCollection>>();
        List<PHAsset> assetArray = new List<PHAsset>();

        private AssetsManager()
        {
        }

        public void Clear()
        {
            // clear observer & subscriber
            UnregisterObserver();
            UnsubscribeAll();

            // clear cache
            if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
            {
                imageManager.StopCaching();
            }

            // clear albums
            AlbumMap.Clear();
            FetchedAlbumsArray.Clear();
            SortedAlbumsArray.Clear();

            // clear assets
            AssetArray.Clear();

            // clear fetch results
            AlbumsFetchArray.Clear();
            FetchMap.Clear();

            // clear flags
            SelectedAlbum = null;
            IsFetchedAlbums = false;
        }

    }

    // MARK: - Subscriber
    public partial class AssetsManager
    {

        public void Subscribe(IAssetsManagerDelegate subscriber)
        {
            Subscribers.Add(subscriber);
        }

        public void Unsubscribe(IAssetsManagerDelegate subscriber)
        {
            Subscribers.Remove(subscriber);
        }

        public void UnsubscribeAll()
        {
            subscribers.Clear();
        }

        public void NotifySubscribers(Action<IAssetsManagerDelegate> action, bool condition = true)
        {
            if (condition)
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    foreach (var subscriber in this.Subscribers)
                    {
                        action?.Invoke(subscriber);
                    }
                });
            }
        }
    }

    // MARK: - Observer
    public partial class AssetsManager
    {
        public void RegisterObserver()
        {
            PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);
        }

        public void UnregisterObserver()
        {
            PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
        }
    }

    #region Permission
    public partial class AssetsManager
    {
        public void Authorize(Action<bool> completion)
        {
            if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
            {
                completion?.Invoke(true);
            }
            else
            {
                PHPhotoLibrary.RequestAuthorization((status) =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        switch (status)
                        {
                            case PHAuthorizationStatus.Authorized:
                                completion?.Invoke(true);
                                break;
                            default:
                                completion?.Invoke(false);
                                break;

                        }
                    });
                });
            }

        }
    }
    #endregion

    #region Cache

    public partial class AssetsManager
    {
        public void Cache(PHAsset asset, CGSize size)
        {
            Cache(new PHAsset[] { asset }, size);
        }

        public void Cache(PHAsset[] assets, CGSize size)
        {
            ImageManager.StartCaching(assets, size, PHImageContentMode.AspectFill, null);
        }

        public void StopCache(PHAsset asset, CGSize size)
        {
            StopCache(new PHAsset[] { asset }, size);
        }

        public void StopCache(PHAsset[] assets, CGSize size)
        {
            ImageManager.StopCaching(assets, size, PHImageContentMode.AspectFill, null);
        }

    }

    #endregion

    #region Sources

    public partial class AssetsManager
    {
        public int NumberOfSections => SortedAlbumsArray.Count;

        public int NumberOfAlbumsInSection(int section)
        {
            return SortedAlbumsArray[section].Count;
        }

        public int NumberOfAssets(NSIndexPath indexPath)
        {
            return (int)FetchMap[SortedAlbumsArray[indexPath.Section][indexPath.Row].LocalIdentifier].Count;
        }

        public NSIndexPath IndexPathForAlbumInAlbumsArray(PHAssetCollection target, IList<IList<PHAssetCollection>> albumArray)
        {
            var section = AlbumSectionFor(target.AssetCollectionType);
            var row = albumArray[section].IndexOf(target);
            if (row > -1)
            {
                return NSIndexPath.FromRowSection(row, section);
            }
            else
            {
                return null;
            }
        }

        public string TitleAt(NSIndexPath indexPath)
        {
            return SortedAlbumsArray[indexPath.Section][indexPath.Row].LocalizedTitle;
        }

        public void ImageOfAlbumAt(NSIndexPath indexPath, CGSize size, bool isNeedDegraded = true, Action<UIImage> completion = null)
        {
            var fetchResult = FetchMap[SortedAlbumsArray[indexPath.Section][indexPath.Row].LocalIdentifier];
            if (fetchResult != null)
            {
                if ((PickerConfig.AssetsIsScrollToBottom ? fetchResult.LastObject : fetchResult.firstObject) is PHAsset asset)
                {
                    ImageManager.RequestImageForAsset(asset, size, PHImageContentMode.AspectFill, null, (image, info) =>
                    {
                        var isDegraded = NSObjectConverter.ToBool(info[PHImageKeys.ResultIsDegraded]);
                        if (!isNeedDegraded && isDegraded)
                        {
                            return;
                        }
                        DispatchQueue.MainQueue.DispatchAsync(() => completion(image));
                    });
                }
                else
                {
                    completion?.Invoke(null);
                }
            }
            else
            {
                completion?.Invoke(null);
            }
        }

        public PHImageRequestID ImageAt(int index, CGSize size, bool isNeedDegraded = true, Action<UIImage, bool> completion = null)
        {
            return imageManager.RequestImageForAsset(assetArray[index], size, PHImageContentMode.AspectFill, null, (image, info) =>
            {
                var isDegraded = NSObjectConverter.ToBool(info[PHImageKeys.ResultIsDegraded]);
                if (!isNeedDegraded && isDegraded)
                {
                    return;
                }
                DispatchQueue.MainQueue.DispatchAsync(() => completion(image, isDegraded));
            });
        }

        public void CancelRequest(PHImageRequestID requestId)
        {
            imageManager.CancelImageRequest(requestId);
        }

        public PHFetchResult FetchResultFor(PHAssetCollection album)
        {
            return FetchMap[album.LocalIdentifier];
        }

        public PHAssetCollection AlbumAt(NSIndexPath indexPath)
        {
            return SortedAlbumsArray[indexPath.Section][indexPath.Row];
        }

        public int AlbumSectionFor(PHAssetCollectionType type)
        {
            switch (type)
            {
                case PHAssetCollectionType.SmartAlbum:
                    return 0;
                case PHAssetCollectionType.Album:
                    return 1;
                case PHAssetCollectionType.Moment:
                    return 2;
            }

            return -1;
        }

        public PHAssetCollectionType AlbumTypeForSection(int section)
        {
            switch (section)
            {
                case 0:
                    return PHAssetCollectionType.SmartAlbum;
                case 1:
                    return PHAssetCollectionType.Album;
                case 2:
                    return PHAssetCollectionType.Moment;
                default:
                    return PHAssetCollectionType.Album;
            }
        }

        public int CountOfType(PHAssetMediaType type)
        {
            if (SelectedAlbum != null && fetchMap.TryGetValue(SelectedAlbum.LocalIdentifier, out PHFetchResult fetchResult))
            {
                return (int)fetchResult.CountOfAssetsWithMediaType(type);
            }
            else
            {
                nuint count = 0;
                foreach (var albums in SortedAlbumsArray)
                {
                    foreach (var album in albums)
                    {
                        if (fetchMap.TryGetValue(album.LocalIdentifier, out PHFetchResult fetchResult2) && album.AssetCollectionSubtype != PHAssetCollectionSubtype.SmartAlbumRecentlyAdded)
                        {
                            count += fetchResult2.CountOfAssetsWithMediaType(type);
                        }
                    }

                }
                return (int)count;
            }
        }

        public bool IsExist(PHAsset asset)
        {
            return PHAsset.FetchAssetsUsingLocalIdentifiers(new string[] { asset.LocalIdentifier }, null).Count > 0;
        }

        public bool Select(PHAssetCollection newAlbum)
        {
            if (SelectedAlbum != null && SelectedAlbum.LocalIdentifier == newAlbum.LocalIdentifier)
            {
                Debug.WriteLine("Selected same album.");
                return false;
            }
            SelectedAlbum = newAlbum;
            if (fetchMap.TryGetValue(newAlbum.LocalIdentifier, out PHFetchResult fetchResult))
            {
                var indexSet = NSIndexSet.FromNSRange(new NSRange(0, fetchResult.Count));
                AssetArray = fetchResult.ObjectsAt<PHAsset>(indexSet).ToList();
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    #endregion

    #region Model Manipulation

    public partial class AssetsManager
    {

        bool IsQualified(PHAssetCollection album)
        {

            return true;
        }

        void RemoveAt(PHAssetCollection album = null, NSIndexPath indexPath = null)
        {
            if (indexPath != null)
            {
                FetchedAlbumsArray[indexPath.Section].RemoveAt(indexPath.Row);
            }
            else
            {
                var albumToRemove = album;
                if (albumToRemove != null)
                {
                    foreach (var i in FetchedAlbumsArray.Select((value, index) => KeyValuePair.Create(index, value)))
                    {
                        var section = i.Key;
                        var fetchedAlbums = i.Value;

                        var row = fetchedAlbums.IndexOf(albumToRemove);
                        if (row > -1)
                        {
                            FetchedAlbumsArray[section].RemoveAt(row);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Empty parameters.");
                }
            }
        }

        PHAssetCollection[] SortedAlbumFromAlbums(PHAssetCollection[] albums)
        {
            var filtered = albums.Where(x => this.IsQualified(x)).ToArray();

            return filtered;
        }
    }

    #endregion

    #region Check
    public partial class AssetsManager
    {
        private bool NotifyIfAuthorizationzStatusChanged()
        {
            var newStatus = PHPhotoLibrary.AuthorizationStatus;
            if (AuthorizationStatus != newStatus)
            {
                var oldStatus = AuthorizationStatus;
                AuthorizationStatus = newStatus;
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    foreach (var subscriber in this.Subscribers)
                    {
                        subscriber.AuthorizationStatusChanged(this, oldStatus, newStatus);
                    }
                });
            }
            return AuthorizationStatus == PHAuthorizationStatus.Authorized;
        }

        bool IsCountChanged(PHFetchResultChangeDetails changeDetails)
        {
            return changeDetails.FetchResultBeforeChanges.Count != changeDetails.FetchResultAfterChanges.Count;
        }

        bool IsThumbnailChanged(PHFetchResultChangeDetails changeDetails)
        {
            var isChanged = false;
            var lastBeforeChange = changeDetails.FetchResultBeforeChanges.LastObject as PHObject;
            var lastAfterChange = changeDetails.FetchResultAfterChanges.LastObject as PHObject;

            if (lastBeforeChange != null)
            {
                if (lastAfterChange != null)
                {
                    if (lastBeforeChange.LocalIdentifier == lastAfterChange.LocalIdentifier)
                    {
                        if (Array.IndexOf(changeDetails.ChangedObjects, lastAfterChange) > -1)
                        {
                            isChanged = true;
                        }
                    }
                    else
                    {
                        isChanged = true;
                    }
                }
                else
                {
                    isChanged = true;
                }
            }
            else
            {
                if (changeDetails.FetchResultAfterChanges.LastObject != null)
                    isChanged = true;
            }

            return isChanged;
        }
    }
    #endregion

    #region Fetch
    public partial class AssetsManager
    {
        public void FetchAlbums(bool isRefetch = false, Action<IList<IList<PHAssetCollection>>> completion = null)
        {
            if (isRefetch)
            {
                SelectedAlbum = null;
                IsFetchedAlbums = false;
                FetchedAlbumsArray.Clear();
                SortedAlbumsArray.Clear();
                AlbumsFetchArray.Clear();
                FetchMap.Clear();
                AlbumMap.Clear();
            }

            if (!IsFetchedAlbums)
            {
                var smartAlbumEntry = FetchAlbumsForAlbumType(PHAssetCollectionType.SmartAlbum);
                FetchedAlbumsArray.Add(smartAlbumEntry.fetchedAlbums);
                SortedAlbumsArray.Add(smartAlbumEntry.sortedAlumbs);
                AlbumsFetchArray.Add(smartAlbumEntry.fetchResult);

                var albumEntry = FetchAlbumsForAlbumType(PHAssetCollectionType.Album);
                FetchedAlbumsArray.Add(albumEntry.fetchedAlbums);
                SortedAlbumsArray.Add(albumEntry.sortedAlumbs);
                AlbumsFetchArray.Add(albumEntry.fetchResult);

                if (PickerConfig.AlbumIsShowMomentAlbums)
                {
                    var momentEntry = FetchAlbumsForAlbumType(PHAssetCollectionType.Album);
                    FetchedAlbumsArray.Add(momentEntry.fetchedAlbums);
                    SortedAlbumsArray.Add(momentEntry.sortedAlumbs);
                    AlbumsFetchArray.Add(momentEntry.fetchResult);
                }
                IsFetchedAlbums = true;
            }
            completion?.Invoke(SortedAlbumsArray);
        }

        public void FetchAssets(bool isRefetch = false, Action<IList<PHAsset>> completion = null)
        {
            FetchAlbums(isRefetch);

            if (isRefetch)
            {
                AssetArray.Clear();
            }
            Select(DefaultAlbum ?? CameraRollAlbum);
            completion(AssetArray);
        }

        public (PHAssetCollection[] fetchedAlbums, PHAssetCollection[] sortedAlumbs, PHFetchResult fetchResult) FetchAlbumsForAlbumType(PHAssetCollectionType type)
        {
            PHFetchOptions fetchOption = null;
            PickerConfig.AlbumFetchOptions?.TryGetValue(type, out fetchOption);
            var albumFetchResult = PHAssetCollection.FetchAssetCollections(type, PHAssetCollectionSubtype.Any, fetchOption);
            var fetchedAlbums = new List<PHAssetCollection>();

            foreach (var obj in albumFetchResult.AsEnumerable())
            {
                if (obj is PHAssetCollection album)
                {
                    this.FetchAlbum(album);
                    if (album.AssetCollectionSubtype == this.PickerConfig.AlbumDefaultType)
                    {
                        DefaultAlbum = album;
                    }
                    if (album.AssetCollectionSubtype == PHAssetCollectionSubtype.SmartAlbumUserLibrary)
                    {
                        this.CameraRollAlbum = album;
                    }
                    fetchedAlbums.Add(album);
                }
            }
            var sortedAlbums = this.SortedAlbumFromAlbums(fetchedAlbums.ToArray());

            // append album fetch result
            return (fetchedAlbums.ToArray(), sortedAlbums, albumFetchResult);
        }

        PHFetchResult FetchAlbum(PHAssetCollection album)
        {
            this.pickerConfig.AssetFetchOptions.TryGetValue(album.AssetCollectionType, out PHFetchOptions option);
            var fetchResult = PHAsset.FetchAssets(album, option);

            // cache fetch result
            this.FetchMap[album.LocalIdentifier] = fetchResult;

            // cache album
            this.AlbumMap[album.LocalIdentifier] = album;

            return fetchResult;
        }
    }
    #endregion

    #region  IndexSet Utility

    public static partial class IndexSetExtensions
    {
        public static NSIndexPath[] AsArray(this NSIndexSet indexSet, int? section = null)
        {
            var indexPaths = new List<NSIndexPath>();

            if (indexSet.Count > 0)
            {
                indexSet.EnumerateIndexes((nuint idx, ref bool stop) =>
                {
                    indexPaths.Add(NSIndexPath.FromRowSection((nint)idx, section ?? 0));
                });
            }

            return indexPaths.ToArray();
        }
    }

    #endregion

}