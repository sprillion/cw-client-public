using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace infrastructure.services.bundles
{
    public class BundlesService : IBundleService
    {
        private readonly string MapPath = "MainMap";

        private bool _initialized;
        private readonly List<AsyncOperationHandle> _heldHandles = new List<AsyncOperationHandle>();
        
        private async UniTask InitializeAsync()
        {
            if (_initialized) return;

            Debug.Log("[Bundles] Initializing Addressables...");
            await Addressables.InitializeAsync().Task;
            _initialized = true;
            Debug.Log("[Bundles] Addressables initialized!");
        }

        public async UniTask<byte[]> LoadMapAsync()
        {
            await InitializeAsync();

            Debug.Log($"[Bundles] Loading map asset '{MapPath}'...");
            var handle = Addressables.LoadAssetAsync<TextAsset>(MapPath);

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var bytes = handle.Result.bytes;
                Debug.Log($"[Bundles] Map loaded successfully. Size: {bytes.Length} bytes.");
                Addressables.Release(handle);
                return bytes;
            }

            Debug.LogError($"[Bundles] Map load FAILED. Status: {handle.Status}, Error: {handle.OperationException?.Message}");
            Addressables.Release(handle);
            return Array.Empty<byte>();
        }
        
        public async UniTask<Material> LoadMaterialAsync(string materialName)
        {
            await InitializeAsync();
            
            var handle = Addressables.LoadAssetAsync<Material>(materialName);
            
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }
            
            Addressables.Release(handle);
            
            return null;
        }
        
        public async UniTask<List<T>> LoadAssetsByLabel<T>(string labelName)
        {
            var handle = Addressables.LoadAssetsAsync<T>(
                new List<string> { labelName },
                null,
                Addressables.MergeMode.Union
            );
        
            await handle.Task;
            
            List<T> allAssets = new List<T>();
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                allAssets = handle.Result.ToList();
                _heldHandles.Add(handle);
            }
            else
            {
                Addressables.Release(handle);
            }

            return allAssets;
        }

        public async UniTask<T> LoadAssetByName<T>(string name)
        {
            var handle = Addressables.LoadAssetAsync<T>(name);
        
            await handle.Task;

            T result = default;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                result = handle.Result;
            } 
            
            Addressables.Release(handle);
            
            return result;
        }
    }
}