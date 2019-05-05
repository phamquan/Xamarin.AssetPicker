using System;
using CoreGraphics;
using Foundation;
using SnapKit;
using UIKit;

namespace AssetsPicker.iOS
{
    internal class AssetsGuideView : UIView
    {
        public nfloat LineSpace { get; set; } = 10;

        protected UIFontTextStyle TitleStyle { get; set; } = UIFontTextStyle.Title1;
        UIFontTextStyle BodyStyle { get; set; } = UIFontTextStyle.Body;

        Lazy<UILabel> messageLabel = new Lazy<UILabel>(() => new UILabel
        {
            TextAlignment = UITextAlignment.Center,
            Lines = 10
        });

        UILabel MessageLabel { get => messageLabel.Value; }

        public AssetsGuideView() => CommonInit();

        protected internal AssetsGuideView(IntPtr handle) : base(handle)
        {
        }

        public AssetsGuideView(CGRect frame) : base(frame)
        {
        }

        protected virtual void CommonInit()
        {
            BackgroundColor = UIColor.White;
            AddSubview(MessageLabel);
            MessageLabel.Snap().MakeConstraints((make) =>
            {
                make.Top.EqualToSuperview();
                make.Leading.EqualToSuperview().Inset(15);
                make.Bottom.EqualToSuperview();
                make.Trailing.EqualToSuperview().Inset(15);
            });
        }

        public void Set(string title, string message)
        {

            var attributedString = new NSMutableAttributedString();

            var titleParagraphStyle = new NSMutableParagraphStyle();
            titleParagraphStyle.ParagraphSpacing = LineSpace;
            titleParagraphStyle.Alignment = UITextAlignment.Center;
            var attributedTitle = new NSMutableAttributedString(str: $"{title}\n",
                font: UIFontExtensions.GetSystemFont(TitleStyle), 
                foregroundColor: new UIColor(red: 0.60f, green: 0.60f, blue: 0.60f, alpha: 1.0f),
                paragraphStyle: titleParagraphStyle);

            var bodyParagraphStyle = new NSMutableParagraphStyle();
            bodyParagraphStyle.Alignment = UITextAlignment.Center;
            bodyParagraphStyle.FirstLineHeadIndent = 20;
            bodyParagraphStyle.HeadIndent = 20;
            bodyParagraphStyle.TailIndent = -20;
            var attributedBody = new NSMutableAttributedString(str: $"{message}\n",
                font: UIFontExtensions.GetSystemFont(BodyStyle),
                foregroundColor: new UIColor(red: 0.60f, green: 0.60f, blue: 0.60f, alpha: 1.0f),
                paragraphStyle: bodyParagraphStyle);

            attributedString.Append(attributedTitle);
            attributedString.Append(attributedBody);
            MessageLabel.AttributedText = attributedString;
        }
    }
}