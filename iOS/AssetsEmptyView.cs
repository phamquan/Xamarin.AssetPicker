using UIKit;

namespace AssetsPicker.iOS
{
    internal class AssetsEmptyView : AssetsGuideView
    {
        protected override void CommonInit()
        {
            var messageKey = AppResources.Message_No_Items;
            if (!UIImagePickerController.IsCameraDeviceAvailable(UIImagePickerControllerCameraDevice.Rear))
            {
                messageKey = AppResources.Message_No_Items_Camera;
            }
            Set(title: AppResources.Title_No_Items,
                 message: string.Format(messageKey, UIDevice.CurrentDevice.Model));
            TitleStyle = UIFontTextStyle.Title2;

            base.CommonInit();
        }
    }
}