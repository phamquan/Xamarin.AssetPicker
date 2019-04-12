using System;
using CoreGraphics;
using UIKit;

namespace AssetsPicker.iOS
{
    public static class UIScreenExtensions
    {
        public static CGSize GetPortraitSize(this UIScreen screen)
        {
            var size = UIScreen.MainScreen.Bounds.Size;
            return new CGSize(width: NMath.Min(size.Width, size.Height), height: NMath.Max(size.Width, size.Height));
        }

        public static CGSize GetLandscapeSize(this UIScreen screen)
        {
            var size = UIScreen.MainScreen.Bounds.Size;
            return new CGSize(width: NMath.Max(size.Width, size.Height), height: NMath.Min(size.Width, size.Height));
        }
    }
}
