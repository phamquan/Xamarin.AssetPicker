using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace AssetsPicker.iOS
{

    public enum SSCheckMarkStyle
    {
        OpenCircle, GrayedOut
    }

    public class SSCheckMark : UIView
    {
        private bool isChecked = true;
        private SSCheckMarkStyle checkMarkStyle = SSCheckMarkStyle.GrayedOut;

        public bool IsChecked
        {
            get => isChecked; set
            {
                isChecked = value;
                SetNeedsDisplay();
            }
        }

        public SSCheckMarkStyle CheckMarkStyle
        {
            get => checkMarkStyle; set
            {
                checkMarkStyle = value;
                SetNeedsDisplay();
            }
        }

        public SSCheckMark()
        {
            CommonInit();
        }

        public SSCheckMark(CGRect frame) : base(frame)
        {
            CommonInit();
        }

        public SSCheckMark(NSCoder coder) : base(coder)
        {
            CommonInit();
        }

        protected internal SSCheckMark(IntPtr handle) : base(handle)
        {
            CommonInit();
        }

        private void CommonInit()
        {
            BackgroundColor = UIColor.Clear;
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            if (IsChecked)
            {
                DrawRectChecked(rect: rect);
            }
            else
            {
                switch (CheckMarkStyle)
                {
                    case SSCheckMarkStyle.OpenCircle:
                        DrawRectOpenCircle(rect: rect);
                        break;
                    case SSCheckMarkStyle.GrayedOut:
                        DrawRectGrayedOut(rect: rect);
                        break;
                }
            }
        }

        void DrawRectChecked(CGRect rect)
        {
            var context = UIGraphics.GetCurrentContext();
            if (context == null)
            {
                return;
            }

            var checkmarkColor = AssetsPickerConfig.DefaultCheckmarkColor;
            var shadow2 = UIColor.Black;


            var shadow2Offset = new CGSize(width: 0.1, height: -0.1);
            var shadow2BlurRadius = 2.5f;
            var frame = this.Bounds;
            var group = new CGRect(x: frame.GetMinX() + 3, y: frame.GetMinY() + 3, width: frame.Width - 6, height: frame.Height - 6);


            var checkedOvalPath = UIBezierPath.FromOval(new CGRect(x: group.GetMinX() + NMath.Floor(group.Width * 0.00000f + 0.5f), y: group.GetMinY() + NMath.Floor(group.Height * 0.00000f + 0.5f), width: NMath.Floor(group.Width * 1.00000f + 0.5f) - NMath.Floor(group.Width * 0.00000f + 0.5f), height: NMath.Floor(group.Height * 1.00000f + 0.5f) - NMath.Floor(group.Height * 0.00000f + 0.5f)));


            context.SaveState();
            context.SetShadow(offset: shadow2Offset, blur: shadow2BlurRadius, color: shadow2.CGColor);
            checkmarkColor.SetFill();
            checkedOvalPath.Fill();
            context.RestoreState();
            UIColor.White.SetStroke();
            checkedOvalPath.LineWidth = 1;
            checkedOvalPath.Stroke();
            var bezierPath = new UIBezierPath();
            bezierPath.MoveTo(new CGPoint(x: group.GetMinX() + 0.27083 * group.Width, y: group.GetMinY() + 0.54167f * group.Height));
            bezierPath.AddLineTo(new CGPoint(x: group.GetMinX() + 0.41667 * group.Width, y: group.GetMinY() + 0.68750f * group.Height));
            bezierPath.AddLineTo(new CGPoint(x: group.GetMinX() + 0.75000 * group.Width, y: group.GetMinY() + 0.35417 * group.Height));
            bezierPath.LineCapStyle = CGLineCap.Square;
            UIColor.White.SetStroke();
            bezierPath.LineWidth = 1.3f;
            bezierPath.Stroke();
        }

        void DrawRectGrayedOut(CGRect rect)
        {
            var context = UIGraphics.GetCurrentContext();
            if (context == null)
            {
                return;
            }

            var grayTranslucent = new UIColor(red: 1, green: 1, blue: 1, alpha: 0.6f);
            var shadow2 = UIColor.Black;
            var shadow2Offset = new CGSize(width: 0.1, height: -0.1);
            var shadow2BlurRadius = 2.5f;
            var frame = this.Bounds;
            var group = new CGRect(x: frame.GetMinX() + 3, y: frame.GetMinY() + 3, width: frame.Width - 6, height: frame.Height - 6);
            var uncheckedOvalPath = UIBezierPath.FromOval(inRect: new CGRect(x: group.GetMinX() + NMath.Floor(group.Width * 0.00000f + 0.5f), y: group.GetMinY() + NMath.Floor(group.Height * 0.00000f + 0.5f), width: NMath.Floor(group.Width * 1.00000f + 0.5f) - NMath.Floor(group.Width * 0.00000f + 0.5f), height: NMath.Floor(group.Height * 1.00000f + 0.5f) - NMath.Floor(group.Height * 0.00000f + 0.5f)));


            context.SaveState();
            context.SetShadow(offset: shadow2Offset, blur: shadow2BlurRadius, color: shadow2.CGColor);
            grayTranslucent.SetFill();
            uncheckedOvalPath.Fill();
            context.RestoreState();
            UIColor.White.SetStroke();
            uncheckedOvalPath.LineWidth = 1;
            uncheckedOvalPath.Stroke();
            var bezierPath = new UIBezierPath();


            bezierPath.MoveTo(new CGPoint(x: group.GetMinX() + 0.27083f * group.Width, y: group.GetMinY() + 0.54167f * group.Height));
            bezierPath.AddLineTo(new CGPoint(x: group.GetMinX() + 0.41667 * group.Width, y: group.GetMinY() + 0.68750 * group.Height));
            bezierPath.AddLineTo(new CGPoint(x: group.GetMinX() + 0.75000 * group.Width, y: group.GetMinY() + 0.35417 * group.Height));
            bezierPath.LineCapStyle = CGLineCap.Square;
            UIColor.White.SetStroke();
            bezierPath.LineWidth = 1.3f;
            bezierPath.Stroke();
        }

        void DrawRectOpenCircle(CGRect rect)
        {
        }
    }
}