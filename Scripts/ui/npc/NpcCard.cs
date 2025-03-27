using I2.Loc;
using infrastructure.services.npc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.npc
{
    public class NpcCard : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        
        [SerializeField] private TMP_Text _npcName;
        
        [SerializeField] private TMP_Text _attitudeLevel;
        [SerializeField] private TMP_Text _attitudeProgress;
        [SerializeField] private TMP_Text _moneySpent;

        [SerializeField] private LoadingPanel _loadingPanel;

        private INpcService _npcService;
        
        [Inject]
        public void Construct(INpcService npcService)
        {
            _npcService = npcService;
            _npcService.OnAttitudeLoaded += SetValues;
        }

        public void Show()
        {
            _loadingPanel?.Show();
        }

        public void Hide()
        {
            _loadingPanel?.Hide();
        }

        private void SetValues()
        {
            _loadingPanel?.Hide();
            
            _iconImage.sprite = _npcService.CurrentNpcData.AvatarIcon;

            _npcName.text = LocalizationManager.GetTranslation($"Npc/Name/{_npcService.CurrentNpcData.NpcType}");
            
            _attitudeLevel.text = _npcService.CurrentAttitudeLevel.ToString();
            _attitudeProgress.text = _npcService.CurrentAttitudeProgress.ToString();
            _moneySpent.text = _npcService.CurrentMoneySpent.ToString();
        }
    }
}