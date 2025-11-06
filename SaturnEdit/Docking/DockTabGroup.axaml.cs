using Avalonia.Controls;

namespace SaturnEdit.Docking;

public partial class DockTabGroup : UserControl
{
    public DockTabGroup()
    {
        InitializeComponent();
    }

    public DockTab? SelectedTab
    {
        get
        {
            if (TabList.Items.Count == 0) return null;
            if (TabList.SelectedItem is not DockTab tab) return null;

            return tab;
        }
        set
        {
            if (value == null || !TabList.Items.Contains(value))
            {
                TabList.SelectedItem = null;
                return;
            }

            TabList.SelectedItem = value;
        }
    }

    public bool IsFloating => VisualRoot is DockWindow;

#region UI Event Delegates
    private void ListBoxTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TabList?.SelectedItem is not DockTab tab) return;
        
        TabContentContainer.Content = tab.TabContent;
    }
#endregion UI Event Delegates
}