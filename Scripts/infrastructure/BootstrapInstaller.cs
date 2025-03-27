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
            Application.targetFrameRate = -1;
            
            BindSaveLoadService();
            BindNetworkManager();
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