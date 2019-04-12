using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace AssetsPicker.iOS
{
    public class PanoramaIconView : UIView
    {

        private CAShapeLayer IconLayer { get; set; } = null;

        public UIColor IconColor { get; set; } = UIColor.White;

        public PanoramaIconView() : base()
        {
        }

        public PanoramaIconView(CGRect frame) : base(frame)
        {
        }

        public PanoramaIconView(CGRect frame, UIColor color) : base(frame)
        {
            IconColor = color;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (this.IconLayer != null)
            {
                IconLayer.RemoveFromSuperLayer();
                IconLayer = null;
            }

            var iconLayer = ShapeLayer(path: ShapePath(size: Bounds.Size));
            Layer.AddSublayer(iconLayer);
            IconLayer = iconLayer;

        }

        private UIBezierPath ShapePath(CGSize size)
        {
            var intensity = 0.44f;
            var controlRatio = 0.25f;


            var padding = 0;
            CGPoint leftTop = new CGPoint(x: padding, y: padding);
            CGPoint rightTop = new CGPoint(x: size.Width - padding, y: padding);
            CGPoint leftBottom = new CGPoint(x: padding, y: size.Height - padding);
            CGPoint rightBottom = new CGPoint(x: size.Width - padding, y: size.Height - padding);


            var path = new UIBezierPath();
            path.MoveTo(leftTop);
            path.AddQuadCurveToPoint(new CGPoint(x: size.Width / 2, y: size.Height / 2 * intensity),
            controlPoint: new CGPoint(x: size.Width / 2 * controlRatio, y: size.Height / 2 * intensity));
            path.AddQuadCurveToPoint(rightTop,
                controlPoint: new CGPoint(x: size.Width - (size.Width / 2 * controlRatio), y: size.Height / 2 * intensity));
            path.AddLineTo(rightBottom);
            path.AddQuadCurveToPoint(new CGPoint(x: size.Width / 2, y: size.Height - (size.Height / 2 * intensity)),
                controlPoint: new CGPoint(x: size.Width - (size.Width / 2 * controlRatio), y: size.Height - (size.Height / 2 * intensity)));
            path.AddQuadCurveToPoint(leftBottom,
                controlPoint: new CGPoint(x: size.Width / 2 * controlRatio, y: size.Height - (size.Height / 2 * intensity)));
            path.AddLineTo(leftTop);
            path.ClosePath();
            return path;
        }

        private CAShapeLayer ShapeLayer(UIBezierPath path)
        {
            var shapeLayer = new CAShapeLayer();
            shapeLayer.Path = path.CGPath;
            shapeLayer.StrokeColor = IconColor.CGColor;
            shapeLayer.FillColor = IconColor.CGColor;
            return shapeLayer;
        }
    }
}