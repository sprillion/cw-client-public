using ui.admin;
using ui.confirm;
using ui.interaction;
using ui.inventory;
using ui.quest;

namespace infrastructure.services.ui
{
    public interface IUiService
    {
        Interaction Interaction { get; }
        Inventory Inventory { get; }
        QuestsInfoPopup QuestsInfoPopup { get; }
        ConfirmView ConfirmView { get; }
        AdminPanelView AdminPanel { get; }
    }
}