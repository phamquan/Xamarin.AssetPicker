using System;
using UIKit;

namespace AssetsPicker.iOS
{
    public static class DeviceExtensions
    {
        public static UIEdgeInsets GetSafeAreaInsets(bool isPotrailt)
        {
            //TODO: Device
            return new UIEdgeInsets(88, 0, 34, 0);
        }
    }
}
