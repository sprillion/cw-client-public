using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.services.transport;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.shop.transport
{
    public class TransportsView : Popup
    {
        [SerializeField] private Transform _elementsParent;

        private readonly List<TransportElement> _elements = new();
        private bool _initialized;

        private ITransportService _transportService;

        [Inject]
        public void Construct(ITransportService transportService)
        {
            _transportService = transportService;
        }

        public override void Initialize()
        {
            Pool.CreatePool<TransportElement>(8);
        }

        public override void Show()
        {
            if (!_initialized)
            {
                _initialized = true;
                _transportService.OnOwnedTransportsChanged += Refresh;
                _transportService.OnLocalTransportSpawned  += OnSpawned;
                _transportService.OnLocalTransportDespawned += OnDespawned;
                CreateElements();
            }
            Refresh();
            base.Show();
        }

        private void CreateElements()
        {
            foreach (var data in _transportService.TransportDatas.Values)
            {
                var element = Pool.Get<TransportElement>();
                element.transform.SetParent(_elementsParent, false);
                element.SetData(data);
                _elements.Add(element);
                LoadElementAsset(element, data.Id).Forget();
            }
        }

        private async UniTaskVoid LoadElementAsset(TransportElement element, int transportTypeId)
        {
            var assetData = await _transportService.GetTransportAssetDataAsync(transportTypeId);
            if (assetData != null)
                element.SetAssetData(assetData);
        }

        private void Refresh()
        {
            var datas = new List<TransportData>(_transportService.TransportDatas.Values);
            for (int i = 0; i < _elements.Count; i++)
            {
                var data = datas[i];
                _elements[i].SetData(data);
                LoadElementAsset(_elements[i], data.Id).Forget();
                if (_transportService.OwnedTransportIds.Contains(data.Id))
                {
                    _elements[i].SetOwned();
                    if (_transportService.LocalSpawnedTransportTypeId == data.Id)
                        _elements[i].SetSpawned();
                }
            }
        }

        private void OnSpawned(int transportTypeId)
        {
            var datas = new List<TransportData>(_transportService.TransportDatas.Values);
            for (int i = 0; i < _elements.Count; i++)
            {
                if (datas[i].Id == transportTypeId)
                {
                    _elements[i].SetSpawned();
                    return;
                }
            }
        }

        private void OnDespawned()
        {
            Refresh();
        }
    }
}
