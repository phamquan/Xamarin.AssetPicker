using Photos;
using UIKit;
using TimeInterval = System.Double;

namespace AssetsPicker.iOS
{

    public interface IAssetsPhotoCellProtocol
    {
        PHAsset Asset { get; set; }
        bool IsSelected { get; set; }
        bool IsVideo { get; set; }
        UIImageView ImageView { get; }
        int Count { get; set; }
        TimeInterval Duration { get; set; }
    }

    public class AssetsPhotoCell : UICollectionViewCell, IAssetsPhotoCellProtocol
    {
        private PHAsset asset;
        private bool isSelected;
        private bool isVideo;

        public PHAsset Asset
        {
            get => asset; set
            {
                asset = value;
                //if let asset = asset {
                //    panoramaIconView.isHidden = asset.mediaSubtypes != .photoPanorama
                //}
            }
        }
        public bool IsSelected
        {
            get => isSelected; set
            {
                isSelected = value;
            }
        }
        public bool IsVideo
        {
            get => isVideo; set
            {
                isVideo = value;
                //            durationLabel.isHidden = !isVideo
                //            if !isVideo {
                //                imageView.removeGradient()
                //            }

            }
        }
        public UIImageView ImageView { get; }
        public int Count { get; set; }
        public TimeInterval Duration { get; set; }

        private UILabel DurationLabel
        {
            get
            {
                var label = new UILabel();
                label.TextColor = UIColor.White;
                label.TextAlignment = UITextAlignment.Right;
                label.Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Caption1);
                return label;
            }
        }

        private PanoramaIconView PanoramaIconView
        {
            get
            {
                var view = new PanoramaIconView();
                view.Hidden = true;
                return view;
            }
        }

        private AssetsPhotoCellOverlay Overlay
        {
            get
            {
                var overlay = new AssetsPhotoCellOverlay();
                overlay.Hidden = true;
                return overlay;
            }
        }
    }
}