using Content.Client._MC.ASRS.UI.Buttons;
using Content.Client._MC.ASRS.UI.Views;
using Content.Shared._MC.ASRS.Components;

namespace Content.Client._MC.ASRS.UI;

public sealed partial class MCASRSBui
{
    public MCASRSCategory Category = null!;

    private MCASRSCategoryView CategoryView => _window.CategoryView;

    private void InitializeViewCategory(MCASRSConsoleComponent component)
    {
        LoadCategories(component.Categories);

        CategoryView.PendingOrderButton.OnPressed += _ => OpenView(_window.PendingOrdersView);
        CategoryView.RequestsButton.OnPressed += _ => OpenView(_window.RequestsView);
        CategoryView.AwaitingDeliveryButton.OnPressed += _ => OpenView(_window.RequestsAwaitingDeliveryView);
        CategoryView.ApprovedRequestsButton.OnPressed += _ => OpenView(_window.RequestsApprovedHistoryView);
        CategoryView.DeniedRequestsButton.OnPressed += _ => OpenView(_window.RequestsDeniedHistoryView);

        // Settings
        CategoryView.ShowIconsButton.OnToggled += args => SetShowIcons(args.Pressed);

        StoreRefreshed += CategoryRefresh;
        StateRefreshed += CategoryRefresh;
    }

    private void CategoryRefresh()
    {
        CategoryView.Refresh(this);
    }

    private void LoadCategories(List<MCASRSCategory> categories)
    {
        var container = CategoryView.CategoriesContainer;

        container.Children.Clear();
        foreach (var category in categories)
        {
            container.AddChild(new MCASRSCategoryButton(category, OnOpenCategory));
        }
    }

    private void OnOpenCategory(MCASRSCategory category)
    {
        Category = category;
        OpenView(OrdersView);
    }

    private void SetShowIcons(bool pressed)
    {
        SettingShowIcons = pressed;

        if (_openedView is not null)
            OpenView(_openedView);
    }
}
