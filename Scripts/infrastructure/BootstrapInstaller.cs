using infrastructure.services.bundles;
using infrastructure.services.saveLoad;
using network;
using UnityEngine;
using Zenject;

namespace infrastructure
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private NetworkManager _networkManager;
        
        public override void InstallBindings()
        {
            BindBundleService();
            BindSaveLoadService();
            BindNetworkManager();
        }

        private void BindBundleService()
        {
            Container
                .Bind<IBundleService>()
                .To<BundlesService>()
                .AsSingle()
                .NonLazy();
        }
                
        private void BindSaveLoadService()
        {
            Container
                .Bind<ISaveLoadService>()
                .To<SaveLoadService>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindNetworkManager()
        {
            Container
                .Bind<INetworkManager>()
                .To<NetworkManager>()
                .FromComponentInNewPrefab(_networkManager)
                .AsSingle()
                .NonLazy();
        }
                
        
    }
}