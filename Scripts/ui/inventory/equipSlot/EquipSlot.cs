namespace ui.inventory.equipSlot
{
    public class EquipSlot : Slot
    {
        public EquipSlotType EquipSlotType;
        
        public override bool DropCondition(UiItem uiItem)
        {
            return uiItem.CurrentItem.IsEquipeItem && EquipSlotType == uiItem.CurrentItem.EquipSlotType;
        }
    }
}