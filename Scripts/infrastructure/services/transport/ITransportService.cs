using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using network;
using UnityEngine;

namespace infrastructure.services.transport
{
    public interface ITransportService : IReceiver
    {
        IReadOnlyCollection<int> OwnedTransportIds { get; }
        IReadOnlyDictionary<int, TransportData> TransportDatas { get; }
        int LocalSpawnedTransportTypeId { get; }
        bool IsLocalMounted { get; }

        event Action OnOwnedTransportsChanged;
        event Action<CurrencyType> OnNotEnoughCurrency;
        event Action<int> OnLocalTransportSpawned;
        event Action OnLocalTransportDespawned;
        event Action OnLocalMounted;
        event Action OnLocalDismounted;

        void LoadData();
        UniTask<TransportAssetData> GetTransportAssetDataAsync(int transportTypeId);

        void GetTransports();
        bool BuyTransport(int transportTypeId);
        void SpawnTransport(int transportTypeId);
        void DespawnTransport();
        void MountTransport();
        void DismountTransport();
        void SetAutopilotDirection(Vector3 direction);
        void StopAutopilot();
    }
}
