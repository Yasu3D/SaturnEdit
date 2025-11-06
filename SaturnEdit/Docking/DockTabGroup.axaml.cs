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
            if (ListBoxTabs.Items.Count == 0) return null;
            if (ListBoxTabs.SelectedItem is not DockTab tab) return null;

            return tab;
        }
        set
        {
            if (value == null || !ListBoxTabs.Items.Contains(value))
            {
                ListBoxTabs.SelectedItem = null;
                return;
            }

            ListBoxTabs.SelectedItem = value;
        }
    }

    public bool IsFloating => VisualRoot is DockWindow;

#region UI Event Delegates
    private void ListBoxTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ListBoxTabs?.SelectedItem is not DockTab tab) return;
        
        TabContentContainer.Content = tab.TabContent;
    }
#endregion UI Event Delegates
}