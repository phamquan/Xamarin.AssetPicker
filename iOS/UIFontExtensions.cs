using System;
using UIKit;

namespace AssetsPicker.iOS
{
    public static class UIFontExtensions
    {
       public static UIFont GetSystemFont(UIFontTextStyle style, UIFontWeight weight = UIFontWeight.Regular)
        {
            var font = UIFont.GetPreferredFontForTextStyle(style);
            return UIFont.SystemFontOfSize(size: font.PointSize, weight: weight);
        }
    }
}
