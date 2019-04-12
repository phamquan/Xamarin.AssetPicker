using System;
using CoreGraphics;
using Photos;
using SnapKit;
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

        public AssetsPhotoCell()
        {
            CommonInit();
        }

        public AssetsPhotoCell(CGRect frame) : base(frame)
        {
            CommonInit();
        }

        private void CommonInit()
        {
            ContentView.AddSubview(ImageView);
            ContentView.AddSubview(DurationLabel);
            ContentView.AddSubview(PanoramaIconView);
            ContentView.AddSubview(Overlay);

            ImageView.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });

            DurationLabel.Snap().MakeConstraints((make) =>
            {
                make.Height.EqualTo(DurationLabel.Font.PointSize + 10);
                make.Leading.EqualToSuperview().Offset(8);
                make.Trailing.EqualToSuperview().Inset(8);
                make.Bottom.EqualToSuperview();
            });

            PanoramaIconView.Snap().MakeConstraints((make) =>
            {
                make.Size.EqualTo(new CGSize(width: 14, height: 7));
                make.Trailing.EqualToSuperview().Inset(6.5);
                make.Bottom.EqualToSuperview().Inset(10);
            });

            Overlay.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (isVideo)
            {
                //imageView.setGradient(.fromBottom, start: 0, end: 0.2, startAlpha: 0.75, color: .black)
            }
        }
    }
}