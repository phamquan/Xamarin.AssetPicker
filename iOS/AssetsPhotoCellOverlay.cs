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

        public UILabel CountLabel { get; } = new UILabel
        {
            TextAlignment = UITextAlignment.Center,
            TextColor = UIColor.White,
            AdjustsFontSizeToFitWidth = true,
            Font = UIFontExtensions.GetSystemFont(UIFontTextStyle.Subheadline),
            Hidden = false
        };

        SSCheckMark CheckMark
        {
            get;
        } = new SSCheckMark();

        public AssetsPhotoCellOverlay() : base()
        {
            CommonInt();
        }

        public AssetsPhotoCellOverlay(CGRect frame) : base(frame)
        {
            CommonInt();
        }

        private void CommonInt()
        {
            //dim(animated: false, color: .white, alpha: 0.25)
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