using Photos;
using UIKit;

namespace AssetsPicker.iOS
{
    public interface IAssetsAlbumCellProtocol
    {
        PHAssetCollection Album { get; set; }
        bool Selected { get; set; }
        UIImageView ImageView { get; }
        string TitleText { get; set; }
        int Count { get; set; }
    }
}