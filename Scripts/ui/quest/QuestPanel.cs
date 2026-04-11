using System;
using I2.Loc;
using infrastructure.services.quests;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.quest
{
    public class QuestPanel : PooledObject
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _panelImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Image _statusIcon;
        
        private IQuestsService _questsService;
        private ColorCatalog _colors;
        
        public Quest CurrentQuest { get; private set; }
        public event Action<QuestPanel> OnSelected; 

        [Inject]
        public void Construct(IQuestsService questsService)
        {
            _questsService = questsService;
            _button.onClick.AddListener(ShowInfo);
            
            _colors = GameResources.Data.Catalogs.quest_panel_colors<ColorCatalog>();
        }

        public override void Release()
        {
            OnSelected = null;
            base.Release();
        }

        public void SetQuest(Quest quest)
        {
            CurrentQuest = quest;
            _nameText.text = LocalizationManager.GetTranslation($"QuestNames/{quest.Id}");
            Unselect();
        }

        public void ShowInfo()
        {
            _questsService.ShowQuestsInfo(CurrentQuest);
            SetColor(QuestPanelType.Selected);
            OnSelected?.Invoke(this);
        }

        public void Unselect()
        {
            SetColor(CurrentQuest.IsCurrent ? QuestPanelType.Current : QuestPanelType.Available);
        }

        private void SetColor(QuestPanelType questPanelType)
        {
            _panelImage.color = _colors.GetColor(questPanelType);
        }
    }
}