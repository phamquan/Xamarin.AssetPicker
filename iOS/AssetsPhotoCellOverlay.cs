using CoreGraphics;
using SnapKit;
using UIKit;

namespace AssetsPicker.iOS
{
    public class AssetsPhotoCellOverlay : UIView
    {
        private int count;

        public int Count
        {
            get => count; set
            {
                count = value;
                CountLabel.Text = $"{Count}";
            }
        }

        UILabel CountLabel
        {
            get
            {
                var label = new UILabel();
                label.TextAlignment = UITextAlignment.Center;
                label.TextColor = UIColor.White;
                label.AdjustsFontSizeToFitWidth = true;
                label.Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Subheadline);
                label.Hidden = true;
                return label;
            }
        }

        SSCheckMark CheckMark
        {
            get
            {
                var view = new SSCheckMark();
                return view;
            }
        }

        private void CommonInt()
        {
            AddSubview(CountLabel);
            AddSubview(CheckMark);

            CountLabel.Snap().MakeConstraints((make) =>
            {
                make.Edges.EqualToSuperview();
            });

            CheckMark.Snap().MakeConstraints((make) =>
            {
                make.Size.EqualTo(new CGSize(30, 30));
                make.Bottom.EqualToSuperview().Inset(1);
                make.Trailing.EqualToSuperview().Inset(1);
            });
        }

    }
}