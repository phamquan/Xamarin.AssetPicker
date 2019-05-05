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
        bool Selected { get; set; }
        bool IsVideo { get; set; }
        UIImageView ImageView { get; }
        int Count { get; set; }
        TimeInterval Duration { get; set; }
    }

    public class AssetsPhotoCell : UICollectionViewCell, IAssetsPhotoCellProtocol
    {
        private PHAsset asset;
        private bool isVideo;
        private int count = 0;
        private double duration = 0;

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

        public override bool Selected
        {
            get => base.Selected; set
            {
                base.Selected = value;
                Overlay.Hidden = !Selected;
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
        public UIImageView ImageView { get; } = new UIImageView
        {
            BackgroundColor = new UIColor(red: 0.94f, green: 0.94f, blue: 0.94f, alpha: 1.0f),
            ContentMode = UIViewContentMode.ScaleAspectFill,
            ClipsToBounds = true
        };

        public int Count
        {
            get => count; set
            {
                count = value;
                Overlay.Count = value;
            }
        }
        public TimeInterval Duration
        {
            get => duration; set
            {
                duration = value;
                DurationLabel.Text = $"{duration}";
            }
        }
        private UILabel DurationLabel { get; } = new UILabel
        {
            TextColor = UIColor.White,
            TextAlignment = UITextAlignment.Right,
            Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Caption1)
        };

        private PanoramaIconView PanoramaIconView { get; } = new PanoramaIconView
        {
            Hidden = true
        };

        private AssetsPhotoCellOverlay Overlay { get; } = new AssetsPhotoCellOverlay
        {
            Hidden = true
        };

        public AssetsPhotoCell()
        {
            CommonInit();
        }

        public AssetsPhotoCell(CGRect frame) : base(frame)
        {
            CommonInit();
        }

        protected internal AssetsPhotoCell(IntPtr handle) : base(handle)
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