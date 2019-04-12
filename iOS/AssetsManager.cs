using System;
using System.Collections.Generic;
using Photos;

namespace AssetsPicker.iOS
{

    public interface IAssetsManagerDelegate
    {

    }

    public class AssetsManager
    {

        public static AssetsManager Shared => shared;
        private readonly static AssetsManager shared = new AssetsManager();

        private AssetsPickerConfig pickerConfig;

        public AssetsPickerConfig PickerConfig
        {
            get => pickerConfig;
            private set
            {
                pickerConfig = value;
                IsFetchedAlbums = false;
            }
        }

        public bool IsFetchedAlbums { get; set; } = false;

        public PHCachingImageManager ImageManager => imageManager;
        public PHAuthorizationStatus AuthorizationStatus => authorizationStatus;
        public IList<IAssetsManagerDelegate> Subscribers { get => subscribers; set => subscribers = value; }
        public IDictionary<string, PHAssetCollection> AlbumMap { get => albumMap; set => albumMap = value; }
        public IList<PHFetchResult> AlbumsFetchArray { get => albumsFetchArray; set => albumsFetchArray = value; }
        public IDictionary<string, PHFetchResult> FetchMap { get => fetchMap; set => fetchMap = value; }
        public IList<IList<PHAssetCollection>> FetchedAlbumsArray { get => fetchedAlbumsArray; set => fetchedAlbumsArray = value; }
        public IList<IList<PHAssetCollection>> SortedAlbumsArray { get => sortedAlbumsArray; set => sortedAlbumsArray = value; }
        public IList<PHAsset> AssetArray { get => assetArray; private set => assetArray = value; }

        public PHAssetCollection DefaultAlbum { get; private set; }
        public PHAssetCollection CameraRollAlbum { get; private set; }
        public PHAssetCollection SelectedAlbum { get; private set; }

        private readonly PHCachingImageManager imageManager = new PHCachingImageManager();
        private readonly PHAuthorizationStatus authorizationStatus = PHPhotoLibrary.AuthorizationStatus;
        IList<IAssetsManagerDelegate> subscribers = new List<IAssetsManagerDelegate>();
        IDictionary<string, PHAssetCollection> albumMap = new Dictionary<string, PHAssetCollection>();
        IList<PHFetchResult> albumsFetchArray = new List<PHFetchResult>();
        IDictionary<string, PHFetchResult> fetchMap = new Dictionary<string, PHFetchResult>();
        IList<IList<PHAssetCollection>> fetchedAlbumsArray = new List<IList<PHAssetCollection>>();
        IList<IList<PHAssetCollection>> sortedAlbumsArray = new List<IList<PHAssetCollection>>();
        IList<PHAsset> assetArray = new List<PHAsset>();

        private AssetsManager()
        {
        }
    }
}
