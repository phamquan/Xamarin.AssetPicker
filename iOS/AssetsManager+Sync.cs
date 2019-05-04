using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Foundation;
using Photos;

namespace AssetsPicker.iOS
{
    public partial class AssetsManager : IPHPhotoLibraryChangeObserver
    {

        NSIndexSet[] SynchronizeAlbums(PHChange changeInstance)
        {
            // updated index set
            var updatedIndexSets = new List<NSIndexSet>();

            // notify changes of albums
            foreach (var i in AlbumsFetchArray.Select((value, index) => KeyValuePair.Create(index, value)))
            {
                var section = i.Key;
                var albumsFetchResult = i.Value;
                var updatedIndexSet = new NSMutableIndexSet();
                updatedIndexSets.Append(updatedIndexSet);

                var albumsChangeDetail = changeInstance.GetFetchResultChangeDetails(albumsFetchResult);
                if (albumsChangeDetail == null) continue;

                // update albumsFetchArray
                AlbumsFetchArray[section] = albumsChangeDetail.FetchResultAfterChanges;

                if (!albumsChangeDetail.HasIncrementalChanges)
                {
                    NotifySubscribers((_) => _.ReloadedAlbumsInSection(this, section));
                    continue;
                }
                // sync removed albums
                var removedIndexes = albumsChangeDetail.RemovedIndexes;
                removedIndexes.EnumerateIndexes((nuint insertedIndex, ref bool stop) =>
                {
                    RemoveAt(indexPath: NSIndexPath.FromRowSection((nint)insertedIndex, section));
                });
                // sync inserted albums
                var insertedIndexes = albumsChangeDetail.InsertedIndexes;
                insertedIndexes.EnumerateIndexes((nuint insertedIndex, ref bool stop) =>
                {
                    var insertedAlbum = albumsChangeDetail.FetchResultAfterChanges[(nint)insertedIndex] as PHAssetCollection;
                    FetchAlbum(insertedAlbum);
                    FetchedAlbumsArray[section].Insert((int)insertedIndex, insertedAlbum);
                    updatedIndexSet.Add(insertedIndex);
                });
                // sync updated albums
                var updatedIndexes = albumsChangeDetail.ChangedIndexes;
                updatedIndexes.EnumerateIndexes((nuint updatedIndex, ref bool stop) =>
                {
                    var updatedAlbum = albumsChangeDetail.FetchResultAfterChanges[(nint)updatedIndex] as PHAssetCollection;
                    FetchAlbum(updatedAlbum);
                    updatedIndexSet.Add(updatedIndex);
                });
            }

            return updatedIndexSets.ToArray();
        }

        void SynchronizeAssets(NSIndexSet[] updatedAlbumIndexSets, IDictionary<string, PHFetchResult> fetchMapBeforeChanges, PHChange changeInstance)
        {
            var updatedIndexSets = updatedAlbumIndexSets;

            foreach (var i in FetchedAlbumsArray.Select((value, index) => KeyValuePair.Create(index, value)))
            {
                var section = i.Key;
                var albums = i.Value;

                var updatedIndexSet = updatedIndexSets[section] as NSMutableIndexSet;

                foreach (var album in albums)
                {
                    Debug.WriteLine($"Looping album: {album.LocalIdentifier}");

                    var fetchResult = fetchMapBeforeChanges[album.LocalIdentifier];
                    var assetsChangeDetails = changeInstance.GetFetchResultChangeDetails(fetchResult);
                    if (fetchResult != null || assetsChangeDetails != null)
                    {
                        continue;
                    }

                    // check thumbnail
                    if (IsThumbnailChanged(assetsChangeDetails) || IsCountChanged(assetsChangeDetails))
                    {
                        var updateRow = fetchedAlbumsArray[section].IndexOf(album);

                        if (updateRow > -1)
                        {
                            updatedIndexSet.Add((uint)updateRow);
                        }
                    }

                    // update fetch result for each album
                    fetchMap[album.LocalIdentifier] = assetsChangeDetails.FetchResultAfterChanges;

                    if (!assetsChangeDetails.HasIncrementalChanges)
                    {
                        NotifySubscribers((subscriber) =>
                        {
                            var indexPathForAlbum = this.IndexPathForAlbumInAlbumsArray(album, this.SortedAlbumsArray);
                            if (indexPathForAlbum != null)
                            {
                                subscriber.ReloadedAlbum(this, album, indexPathForAlbum);
                            }
                        });

                        continue;
                    }
                    var selectedAlbum = this.SelectedAlbum;
                    if (selectedAlbum == null || selectedAlbum.LocalIdentifier != album.LocalIdentifier)
                    {
                        continue;
                    }

                    // sync removed assets
                    var removedIndexesSet = assetsChangeDetails.RemovedIndexes;
                    if (removedIndexesSet != null)
                    {
                        var removedIndexes = removedIndexesSet.AsArray().OrderBy(x => x.Row);
                        var removeAssets = new List<PHAsset>();
                        foreach (var removedIndex in removedIndexes.Reverse())
                        {
                            removeAssets.Insert(0, assetArray.RemoveAndGet(removedIndex.Row));
                        }
                        // stop caching for removed assets
                        StopCache(removeAssets.ToArray(), PickerConfig.AssetCacheSize);
                        NotifySubscribers((_) => _.RemovedAssets(this, removeAssets.ToArray(), removedIndexes.ToArray()), removeAssets.Count > 0);
                    }

                    // sync inserted assets
                    var insertedIndexesSet = assetsChangeDetails.InsertedIndexes;
                    if (insertedIndexesSet != null)
                    {
                        var insertedIndexes = insertedIndexesSet.AsArray().OrderBy(x => x.Row);
                        var insertedAssets = new List<PHAsset>();
                        foreach (var insertedIndex in insertedIndexes)
                        {
                            var insertedAsset = assetsChangeDetails.FetchResultAfterChanges[insertedIndex.Row] as PHAsset;
                            insertedAssets.Append(insertedAsset);
                            assetArray.Insert(insertedIndex.Row, insertedAsset);
                        }
                        // stop caching for removed assets
                        Cache(insertedAssets.ToArray(), PickerConfig.AssetCacheSize);
                        NotifySubscribers((_) => _.InsertedAssets(this, insertedAssets.ToArray(), insertedIndexes.ToArray()), insertedAssets.Count > 0);
                    }

                    // sync updated assets
                    var updatedIndexes = assetsChangeDetails.ChangedIndexes.AsArray();
                    if (updatedIndexes != null)
                    {
                        var updatedAssets = new List<PHAsset>();
                        foreach (var updatedIndex in updatedIndexes)
                        {
                            var updatedAsset = assetsChangeDetails.FetchResultAfterChanges[updatedIndex.Row] as PHAsset;
                            updatedAssets.Append(updatedAsset);
                        }
                        // stop caching for removed assets
                        Cache(updatedAssets.ToArray(), PickerConfig.AssetCacheSize);
                        StopCache(updatedAssets.ToArray(), PickerConfig.AssetCacheSize);
                        NotifySubscribers((_) => _.UpdatedAssets(this, updatedAssets.ToArray(), updatedIndexes.ToArray()), updatedAssets.Count > 0);
                    }
                }

                // update final changes in albums
                var oldSortedAlbums = SortedAlbumsArray[section];
                var newSortedAlbums = SortedAlbumFromAlbums(fetchedAlbumsArray[section].ToArray());

                /* 1. find & notify removed albums. */
                var removedInfo = RemovedIndexPaths(newSortedAlbums, oldSortedAlbums.ToArray(), section);
                foreach (var item in removedInfo.Item1.Select((value, index) => new { Value = value, Index = index }))
                {
                    var fetchedIndexPath = IndexPathForAlbumInAlbumsArray(removedInfo.Item2[item.Index], fetchedAlbumsArray);
                    if (fetchedIndexPath != null)
                    {
                        updatedIndexSet.Remove((uint)fetchedIndexPath.Row);
                    }
                }
                SortedAlbumsArray[section] = oldSortedAlbums;
                NotifySubscribers((_) => _.InsertedAlbums(this, removedInfo.Item2, removedInfo.Item1), removedInfo.Item1.Length > 0);

                /* 2. find & notify inserted albums. */

            }
        }

        public void PhotoLibraryDidChange(PHChange changeInstance)
        {
            if (!this.NotifyIfAuthorizationzStatusChanged())
            {
                Debug.WriteLine("Does not have access to photo library.");
                return;
            }
            var fetchMapBeforeChanges = fetchMap;
            var updatedAlbumIndexSets = SynchronizeAlbums(changeInstance: changeInstance);
            SynchronizeAssets(
                updatedAlbumIndexSets: updatedAlbumIndexSets,
                fetchMapBeforeChanges: fetchMapBeforeChanges,
                changeInstance: changeInstance
            );
        }


        public (NSIndexPath[] indexPaths, PHAssetCollection[] albums) RemovedIndexPaths(PHAssetCollection[] newAlbums, PHAssetCollection[] oldAlbums, int section)
        {
            var removedIndexPaths = new List<NSIndexPath>();
            var removedAlbums = new List<PHAssetCollection>();

            foreach (var item in oldAlbums.Reverse().Select((value, index) => new { Value = value, Index = index }))
            {
                if (!newAlbums.Contains(item.Value))
                {
                    removedAlbums.Append(item.Value);
                    removedIndexPaths.Append(NSIndexPath.FromRowSection(item.Index, section));
                    continue;
                }
            }

            return (removedIndexPaths.ToArray(), removedAlbums.ToArray());
        }

        public Tuple<NSIndexPath[], PHAssetCollection[]> InsertedIndexPaths(PHAssetCollection[] newAlbums, PHAssetCollection[] oldAlbums, int section)
        {
            var insertedIndexPaths = new List<NSIndexPath>();
            var insertedAlbums = new List<PHAssetCollection>();

            foreach (var item in newAlbums.Select((value, index) => new { Value = value, Index = index }))
            {
                if (!newAlbums.Contains(item.Value))
                {
                    insertedAlbums.Append(item.Value);
                    insertedIndexPaths.Append(NSIndexPath.FromRowSection(item.Index, section));
                    continue;
                }
            }

            return Tuple.Create(insertedIndexPaths.ToArray(), insertedAlbums.ToArray());
        }

    }
}
