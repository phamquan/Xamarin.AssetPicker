namespace AssetsPicker.iOS
{
    internal class AssetsNoPermissionView : AssetsGuideView
    {
        protected override void CommonInit()
        {
            Set(title: AppResources.Title_No_Permission, message: AppResources.Message_No_Permission);
            base.CommonInit();
        }
    }
}