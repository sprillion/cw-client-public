using System;
using Cysharp.Threading.Tasks;
using infrastructure.services.npc;
using infrastructure.services.shop;
using TMPro;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.shop
{
    public class InsufficientFundsView : Popup
    {
        [SerializeField] private float _hideDelay = 1f;

        [SerializeField] private TMP_Text _text;
        
        [Inject]
        public void Construct(IShopService shopService)
        {
            shopService.OnNotEnoughCurrency += (currency) => Show(currency);
        }

        public override void Show(params object[] args)
        {
            var currency = (CurrencyType)args[0];
            _text.text = $"Shop/NotEnough{currency}".Loc();
            base.Show(args);
        }

        public override void Show()
        {
            HideWithDelay().Forget();
            base.Show();
        }

        private async UniTaskVoid HideWithDelay()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_hideDelay));
            Hide();
        }
    }
}