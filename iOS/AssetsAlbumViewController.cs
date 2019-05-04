using System;
using Photos;
using UIKit;

namespace AssetsPicker.iOS
{

    public interface IAssetsAlbumViewControllerDelegate
    {
        void DidCancel(AssetsAlbumViewController controller);
        void DidSelectAlbum(AssetsAlbumViewController controller, PHAssetCollection album);
    }

    public class AssetsAlbumViewController : UIViewController
    {

        public AssetsPickerConfig PickerConfig { get; }
        public IAssetsAlbumViewControllerDelegate Delegate { get; internal set; }

        public AssetsAlbumViewController(AssetsPickerConfig pickerConfig) : base(null, null)
        {
            PickerConfig = pickerConfig;
        }

    }
}
