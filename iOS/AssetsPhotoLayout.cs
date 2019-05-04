using System;
using CoreGraphics;
using UIKit;

namespace AssetsPicker.iOS
{
    public partial class AssetsPhotoLayout : UICollectionViewFlowLayout
    {

        public CGPoint? TranslatedOffset { get; set; }
        private AssetsPickerConfig PickerConfig { get; set; }

        public AssetsPhotoLayout(AssetsPickerConfig pickerConfig) : base()
        {
            PickerConfig = pickerConfig;
        }

        public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
        {
            return TargetContentOffsetForProposedContentOffset(proposedContentOffset);
        }

        public override CGPoint TargetContentOffsetForProposedContentOffset(CGPoint proposedContentOffset)
        {
            if (this.TranslatedOffset is CGPoint TranslatedOffset)
            {
                return TranslatedOffset;
            }
            else
            {
                return proposedContentOffset;
            }
        }
    }

    public partial class AssetsPhotoLayout
    {
        public nfloat ExpectedContentHeightForViewSize(CGSize size, bool isPortrait)
        {
            var rows = AssetsManager.Shared.AssetArray.Count / (isPortrait ? PickerConfig.AssetPortraitColumnCount : PickerConfig.AssetLandscapeColumnCount);
            var remainder = AssetsManager.Shared.AssetArray.Count % (isPortrait ? PickerConfig.AssetPortraitColumnCount : PickerConfig.AssetLandscapeColumnCount);
            rows += remainder > 0 ? 1 : 0;

            var cellSize = isPortrait ? PickerConfig.AssetPortraitCellSize(UIScreen.MainScreen.GetPortraitContentSize()) :
                PickerConfig.AssetLandscapeCellSize(UIScreen.MainScreen.GetLandscapeContentSize());
            var lineSpace = isPortrait ? PickerConfig.AssetPortraitLineSpace : PickerConfig.AssetLandscapeLineSpace;
            var contentHeight = rows * cellSize.Height + (NMath.Max(rows - 1, 0) * lineSpace);
            var bottomHeight = cellSize.Height * 2 / 3 + DeviceExtensions.GetSafeAreaInsets(isPortrait).Bottom;

            return contentHeight + bottomHeight;
        }

        public nfloat OffsetRatio(UICollectionView collectionView, CGPoint offset, CGSize contentSize, bool isPotrait)
        {
            return (offset.Y > 0 ? offset.Y : 0) / ((contentSize.Height + DeviceExtensions.GetSafeAreaInsets(isPotrait).Bottom) - CollectionView.Bounds.Height);
        }

        public CGPoint? TranslateOffsetForChangingSize(CGSize size, CGPoint currentOffset)
        {
            if (CollectionView == null) return null;

            var isPotraitFuture = size.Height > size.Width;
            var isPotraitCurrent = CollectionView.Bounds.Size.Height > CollectionView.Bounds.Size.Width;
            var contentHeight = ExpectedContentHeightForViewSize(size, isPotraitFuture);
            var currentRatio = OffsetRatio(CollectionView, currentOffset, CollectionView.ContentSize, isPotraitCurrent);

            var futureOffsetY = (contentHeight - size.Height) * currentRatio;

            if (currentOffset.Y < 0)
            {
                var insetRatio = (-currentOffset.Y) / DeviceExtensions.GetSafeAreaInsets(isPotraitCurrent).Top;
                var insetDiff = DeviceExtensions.GetSafeAreaInsets(isPotraitFuture).Top * insetRatio;
                futureOffsetY -= insetDiff;
            }

            return new CGPoint(0, futureOffsetY);
        }

    }
}
