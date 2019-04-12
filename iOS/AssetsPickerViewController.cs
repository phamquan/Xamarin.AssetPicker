using System;
using Foundation;
using Photos;
using UIKit;

namespace AssetsPicker.iOS
{
    
    public interface IAssetsPickerViewControllerDelegate {
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
        //            open var selectedAssets: [PHAsset] {
        //        return photoViewController.selectedAssets
        //    }

        //    open var isShowLog: Bool = false
        //    public var pickerConfig: AssetsPickerConfig! {
        //        didSet {
        //            if let config = self.pickerConfig?.prepare() {
        //                AssetsManager.shared.pickerConfig = config
        //                photoViewController?.pickerConfig = config
        //}
        //        }
        //    }

        //    public private (set) var photoViewController: AssetsPhotoViewController!

        //    required public init? (coder aDecoder: NSCoder) {
        //        super.init(coder: aDecoder)
        //        commonInit()
        //    }

        //    override public init(nibName nibNameOrNil: String?, bundle nibBundleOrNil: Bundle?)
        //{
        //    super.init(nibName: nibNameOrNil, bundle: nibBundleOrNil)
        //        commonInit()
        //    }

        //public convenience init()
        //{
        //    self.init(nibName: nil, bundle: nil)
        //    }

        //func commonInit()
        //{
        //    let config = AssetsPickerConfig().prepare()
        //        self.pickerConfig = config
        //        AssetsManager.shared.pickerConfig = config
        //        let controller = AssetsPhotoViewController()
        //        controller.pickerConfig = config
        //        self.photoViewController = controller


        //        TinyLog.isShowInfoLog = isShowLog
        //        TinyLog.isShowErrorLog = isShowLog
        //        AssetsManager.shared.registerObserver()
        //        viewControllers = [photoViewController]
        //    }

        //deinit {
        //    AssetsManager.shared.clear()
        //    logd("Released \(type(of: self))")
        //}
    }
}
