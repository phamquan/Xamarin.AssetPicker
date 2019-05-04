using System;
using CoreGraphics;
using Foundation;
using UIKit;
using SnapKit;

namespace AssetsPicker.iOS
{
    internal class AssetsPhotoFooterView : UICollectionReusableView
    {
        private UILabel countLabel = new UILabel()
        {
            TextAlignment = UITextAlignment.Center,
            Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Subheadline, UIFontWeight.Semibold),
            TextColor = UIColor.DarkTextColor
        };

        public UILabel CountLabel { get => countLabel; }

        public AssetsPhotoFooterView(NSCoder coder) : base(coder)
        {
            CommonInit();
        }

        public AssetsPhotoFooterView(CGRect frame) : base(frame)
        {
            CommonInit();
        }

        private void CommonInit()
        {
            AddSubview(CountLabel);
            CountLabel.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });
        }

        public void Set(int imageCount, int videoCount)
        {
            //TODO: Localize
            string countText = $"{imageCount} Photos, {videoCount} Videos";

            CountLabel.Text = countText;
        }
    }
}