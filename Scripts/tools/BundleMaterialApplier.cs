using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.bundles;
using UnityEngine;
using Zenject;

namespace tools
{
    public class BundleMaterialApplier : MonoBehaviour
    {
        [SerializeField] private string[] _materialNames;
        [SerializeField] private Renderer[] _renderers;

        public event Action<int, Renderer, Material> MaterialApplied;

        private IBundleService _bundleService;

        [Inject]
        private void Construct(IBundleService bundleService)
        {
            _bundleService = bundleService;
        }

        private void Start()
        {
            ApplyMaterialsAsync().Forget();
        }

        private async UniTaskVoid ApplyMaterialsAsync()
        {
            for (int n = 0; n < _materialNames.Length; n++)
            {
                var material = await _bundleService.LoadMaterialAsync(_materialNames[n]);

                if (material == null)
                {
                    Debug.LogWarning($"[BundleMaterialApplier] Material '{_materialNames[n]}' not found.", this);
                    continue;
                }

                foreach (var renderer in _renderers)
                {
                    if (_materialNames.Length == 1)
                    {
                        renderer.sharedMaterial = material;
                    }
                    else
                    {
                        var materials = renderer.sharedMaterials;
                        if (n >= materials.Length)
                        {
                            Debug.LogWarning($"[BundleMaterialApplier] Renderer '{renderer.name}' has no slot {n}.", this);
                            continue;
                        }
                        materials[n] = material;
                        renderer.sharedMaterials = materials;
                    }

                    MaterialApplied?.Invoke(n, renderer, material);
                }
            }
        }
    }
}
