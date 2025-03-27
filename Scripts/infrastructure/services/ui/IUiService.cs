using ui.interaction;
using ui.inventory;

namespace infrastructure.services.ui
{
    public interface IUiService
    {
        Interaction Interaction { get; }
        Inventory Inventory { get; }
    }
}