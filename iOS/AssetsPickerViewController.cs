using System;
using System.Collections.Generic;
using System.Diagnostics;
using Foundation;
using Photos;
using UIKit;

namespace AssetsPicker.iOS
{

    public interface IAssetsPickerViewControllerDelegate
    {
        void DidCancel(AssetsPickerViewController controller);
        void CannotAccessPhotoLibrary(AssetsPickerViewController controller);
        void SelectedAssets(AssetsPickerViewController controller, PHAsset[] assets);
        bool ShouldSelectAsset(AssetsPickerViewController controller, PHAsset asset, NSIndexPath atIndexPath);
        void DidSelectAsset(AssetsPickerViewController controller, PHAsset asset, NSIndexPath atIndexPath);
        bool ShouldDeselectAssets(AssetsPickerViewController controller, PHAsset asset, NSIndexPath atIndexPath);
        void DidDeselect(AssetsPickerViewController controller, PHAsset asset, NSIndexPath atIndexPath);
        void DidDismissByCancelling(AssetsPickerViewController controller, bool byCancel);
    }

    public class AssetsPickerViewController : UINavigationController
    {

        public WeakReference<IAssetsPickerViewControllerDelegate> PickerDelegate;
        private AssetsPickerConfig pickerConfig;

        public IList<PHAsset> SelectedAssets
        {
            get
            {
                return PhotoViewController.SelectedAssets;
            }
        }

        public bool IsShowLog { get; set; } = false;
        public AssetsPickerConfig PickerConfig
        {
            get => pickerConfig; set
            {
                pickerConfig = value;
                if (this.PickerConfig != null && PickerConfig.Prepare() is AssetsPickerConfig config )
                {
                    AssetsManager.Shared.PickerConfig = config;
                    PhotoViewController.PickerConfig = config;
                }
            }
        }

        public AssetsPhotoViewController PhotoViewController { get; private set; }

        public AssetsPickerViewController(NSCoder coder) : base(coder)
        {
            CommonInit();
        }

        public AssetsPickerViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
            CommonInit();
        }

        public AssetsPickerViewController() : this(null, null)
        {
        }

        void CommonInit()
        {
            var config = new AssetsPickerConfig().Prepare();
            this.pickerConfig = config;
            AssetsManager.Shared.PickerConfig = config;
            var controller = new AssetsPhotoViewController();
            controller.PickerConfig = config;
            this.PhotoViewController = controller;


            //TinyLog.isShowInfoLog = isShowLog
            //TinyLog.isShowErrorLog = isShowLog
            AssetsManager.Shared.RegisterObserver();
            ViewControllers = new UIViewController[] { PhotoViewController };
            }

        ~AssetsPickerViewController()
        {
            AssetsManager.Shared.Clear();
            Debug.WriteLine("Release");
        }
    }
}
