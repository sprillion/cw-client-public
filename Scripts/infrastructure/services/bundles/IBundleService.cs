using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace infrastructure.services.bundles
{
    public interface IBundleService
    {
        UniTask<byte[]> LoadMapAsync();
        UniTask<Material> LoadMaterialAsync(string materialName);
        UniTask<List<T>> LoadAssetsByLabel<T>(string labelName);
        UniTask<T> LoadAssetByName<T>(string name);
    }
}