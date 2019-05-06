using System;
using CoreGraphics;
using Foundation;
using Photos;
using SnapKit;
using UIKit;

namespace AssetsPicker.iOS
{
    public class AssetsAlbumCell : UICollectionViewCell, IAssetsAlbumCellProtocol
    {
        private string titleText;
        private int count = 0;

        public AssetsAlbumCell(NSCoder coder) : base(coder)
        {
            CommonInit();
        }

        public AssetsAlbumCell(CGRect frame) : base(frame)
        {
            CommonInit();
        }

        protected internal AssetsAlbumCell(IntPtr handle) : base(handle)
        {
            CommonInit();
        }

        public PHAssetCollection Album { get; set; }
        public string TitleText
        {
            get => titleText; set
            {
                titleText = value;
                TitleLabel.Text = TitleText;
            }
        }
        public int Count
        {
            get => count; set
            {
                count = value;
                CountLabel.Text = $"{Count}";
            }
        }

        public UIImageView ImageView { get; } = new UIImageView
        {
            BackgroundColor = new UIColor(red: 0.94f, green: 0.94f, blue: 0.94f, alpha: 1.0f),
            ContentMode = UIViewContentMode.ScaleAspectFill,
            ClipsToBounds = true
        };

        private UILabel TitleLabel { get; } = new UILabel
        {
            TextColor = UIColor.Black,
            Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Subheadline)
        };

        private UILabel CountLabel { get; } = new UILabel
        {
            TextColor = new UIColor(red: 0.55f, green: 0.55f, blue: 0.57f, alpha: 1.0f),
            Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Subheadline)
        };



        private void CommonInit()
        {
            ImageView.Layer.CornerRadius = 5;
            ContentView.AddSubview(ImageView);
            ContentView.AddSubview(TitleLabel);
            ContentView.AddSubview(CountLabel);


            ImageView.Snap().MakeConstraints((make) =>
            {
                make.Height.EqualTo(ImageView.Snap().Width);
                make.Top.EqualToSuperview();
                make.Leading.EqualToSuperview();
                make.Trailing.EqualToSuperview();
            });

            TitleLabel.Snap().MakeConstraints((make) =>
            {
                make.Top.EqualTo(ImageView.Snap().Bottom).Offset(8);
                make.Leading.EqualToSuperview();
                make.Trailing.EqualToSuperview();
                make.Height.EqualTo(TitleLabel.Font.PointSize + 2);
            });

            CountLabel.Snap().MakeConstraints((make) =>
            {
                make.Top.EqualTo(TitleLabel.Snap().Bottom).Offset(2);
                make.Leading.EqualToSuperview();
                make.Trailing.EqualToSuperview();
                make.Height.EqualTo(CountLabel.Font.PointSize + 2);
            });
        }
    }
}