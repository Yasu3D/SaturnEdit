using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentIcons.Common;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit.Docking;

public partial class DockArea : UserControl
{
    public DockArea()
    {
        InitializeComponent();
        Instance = this;

        DockTabGroup groupA = new();
        groupA.ListBoxTabs.Items.Add(new DockTab(new CursorView(), Icon.Agents, "ChartEditor.Cursor"));

        DockTabGroup groupB = new();
        groupB.ListBoxTabs.Items.Add(new DockTab(new ChartPropertiesView(), Icon.Agents, "ChartEditor.ChartProperties"));
        groupB.ListBoxTabs.Items.Add(new DockTab(new EventListView(), Icon.Agents, "ChartEditor.EventList"));

        DockSplitter splitterA = new(groupA, groupB, TargetSide.Top, 0.75);
        
        DockTabGroup groupC = new();
        groupC.ListBoxTabs.Items.Add(new DockTab(new ChartView3D(), Icon.Agents, "ChartEditor.ChartView3D"));
        groupC.ListBoxTabs.Items.Add(new DockTab(new ChartViewTxt(), Icon.Agents, "ChartEditor.ChartViewTxt"));
        
        DockSplitter splitterB = new(splitterA, groupC, TargetSide.Left, 0.66);
        
        Root.Content = splitterB;

        DockTabGroup groupD = new();
        groupD.ListBoxTabs.Items.Add(new DockTab(new ChartStatisticsView(), Icon.Status, "ChartEditor.ChartStatistics"));
        
        DockWindow window = new()
        {
            Content = groupD,
        };
        
        window.Show();
    }

    public static DockArea? Instance { get; private set; } = null;

#region Methods
    public void DockIntoRoot(DockTabGroup insertedGroup, TargetSide targetSide)
    {
        if (insertedGroup.GetVisualRoot() is not DockWindow insertedGroupRootWindow) return;
        
        insertedGroupRootWindow.Content = null;
        insertedGroupRootWindow.Close();
        
        if (targetSide == TargetSide.None) return;
        // There are two valid scenarios:
        
        // 1. Center Target, empty root.
        // - add item to root.
        if (targetSide == TargetSide.Center && Root.Content == null)
        {
            Root.Content = insertedGroup;
            return;
        }

        // 2. Side Target, occupied root.
        // - create splitter
        // - move existing root to one side of splitter
        // - move item to other side of splitter
        if (targetSide != TargetSide.Center && Root.Content is UserControl existingRoot)
        {
            DockSplitter splitter = new(existingRoot, insertedGroup, targetSide);
            Root.Content = splitter;
        }
    }
    
    public void DockIntoTabGroup(DockTabGroup destinationGroup, DockTabGroup insertedGroup, TargetSide targetSide)
    {
        if (targetSide == TargetSide.None) return;
        // There are two valid scenarios:
        
        if (insertedGroup.GetVisualRoot() is not DockWindow insertedGroupRootWindow) return;
        
        insertedGroupRootWindow.Content = null;
        insertedGroupRootWindow.Close();
        
        // 1. Center target
        // - clear tab content container
        // - remove tabs from insertedGroup
        // - add tabs to destinationGroup
        if (targetSide == TargetSide.Center && insertedGroup.ListBoxTabs.Items.Count != 0)
        {
            DockTab? selectedTab = insertedGroup.SelectedTab;
            insertedGroup.TabContentContainer.Content = null;

            List<DockTab> tabsToAdd = [];
            for (int i = insertedGroup.ListBoxTabs.Items.Count - 1; i >= 0; i--)
            {
                DockTab? tab = (DockTab?)insertedGroup.ListBoxTabs.Items[i];
                
                if (tab != null)
                {
                    insertedGroup.ListBoxTabs.Items.Remove(tab);
                    tabsToAdd.Add(tab);
                }
            }

            for (int i = tabsToAdd.Count - 1; i >= 0; i--)
            {
                destinationGroup.ListBoxTabs.Items.Add(tabsToAdd[i]);
            }
            
            destinationGroup.SelectedTab = selectedTab;
        }
        
        // 2. Side target
        // - find destinationRoot
        // - remove destinationGroup from destinationRoot
        // - remove insertedGroup from root window
        // - create splitter from destinationGroup and insertedGroup
        // - insert splitter into destinationRoot
        if (targetSide != TargetSide.Center)
        {
            if (destinationGroup.Parent is not UserControl destinationRoot) return;

            destinationRoot.Content = null;
            
            DockSplitter splitter = new(destinationGroup, insertedGroup, targetSide);
            destinationRoot.Content = splitter;
        }
    }
#endregion Methods
}