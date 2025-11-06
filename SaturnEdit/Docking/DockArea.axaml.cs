using System;
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
        groupA.TabList.Items.Add(new DockTab(new CursorView(), Icon.Agents, "ChartEditor.Cursor"));

        DockTabGroup groupB = new();
        groupB.TabList.Items.Add(new DockTab(new ChartPropertiesView(), Icon.Agents, "ChartEditor.ChartProperties"));
        groupB.TabList.Items.Add(new DockTab(new EventListView(), Icon.Agents, "ChartEditor.EventList"));

        DockSplitter splitterA = new(groupA, groupB, TargetSide.Top, 0.75);
        
        DockTabGroup groupC = new();
        groupC.TabList.Items.Add(new DockTab(new ChartView3D(), Icon.Agents, "ChartEditor.ChartView3D"));
        groupC.TabList.Items.Add(new DockTab(new ChartViewTxt(), Icon.Agents, "ChartEditor.ChartViewTxt"));
        
        DockSplitter splitterB = new(splitterA, groupC, TargetSide.Left, 0.66);
        
        Root.Content = splitterB;
    }

    public static DockArea? Instance { get; private set; } = null;

#region Methods
    public void Dock(DockTarget destination, DockTabGroup insertedGroup)
    {
        if (destination.TargetSide == TargetSide.None) return;
        
        UserControl? parent = destination.Parent?.Parent as UserControl;
        
        // remove floating window.
        if (insertedGroup.GetVisualRoot() is not DockWindow window) return;
        RemoveWindow(window);

        // A. Docking into Root
        if (parent == this)
        {
            // 1. Center Target, empty root.
            if (destination.TargetSide == TargetSide.Center && Root.Content == null)
            {
                // place inserted group in root.
                Root.Content = insertedGroup;
                return;
            }

            // 2. Side Target, occupied root.
            if (destination.TargetSide != TargetSide.Center && Root.Content is UserControl existingRoot)
            {
                // create splitter
                DockSplitter splitter = new(existingRoot, insertedGroup, destination.TargetSide);
                
                // place splitter in root.
                Root.Content = splitter;
            }
        }

        // B. Docking into Tab Group
        if (parent is DockTabGroup destinationGroup)
        {
            // 1. Center target
            if (destination.TargetSide == TargetSide.Center && insertedGroup.TabList.Items.Count != 0)
            {
                // clear tab content container
                DockTab? selectedTab = insertedGroup.SelectedTab;
                insertedGroup.TabContentContainer.Content = null;

                // remove tabs from insertedGroup
                List<DockTab> tabsToAdd = [];
                for (int i = insertedGroup.TabList.Items.Count - 1; i >= 0; i--)
                {
                    DockTab? tab = (DockTab?)insertedGroup.TabList.Items[i];

                    if (tab != null)
                    {
                        insertedGroup.TabList.Items.Remove(tab);
                        tabsToAdd.Add(tab);
                    }
                }

                // add tabs to destinationGroup
                for (int i = tabsToAdd.Count - 1; i >= 0; i--)
                {
                    destinationGroup.TabList.Items.Add(tabsToAdd[i]);
                }

                destinationGroup.SelectedTab = selectedTab;
            }

            // 2. Side target
            if (destination.TargetSide != TargetSide.Center)
            {
                // remove destinationGroup from destinationRoot
                if (destinationGroup.Parent is not UserControl destinationRoot) return;
                destinationRoot.Content = null;

                // create splitter from destinationGroup and insertedGroup
                DockSplitter splitter = new(destinationGroup, insertedGroup, destination.TargetSide);

                // insert splitter into destinationRoot
                destinationRoot.Content = splitter;
            }
        }
    }

    public void FloatTabGroup(DockTabGroup group)
    {
        RemoveTabGroup(group);

        DockWindow window = new()
        {
            Content = group,
        };

        window.Show();
    }
    
    public void FloatTab(DockTabGroup group, DockTab tab)
    {
        RemoveTab(group, tab);

        DockTabGroup newGroup = new();
        newGroup.TabList.Items.Add(tab);
        
        DockWindow window = new()
        {
            Content = group,
        };

        window.Show();
    }
    
    public void RemoveWindow(DockWindow dockWindow)
    {
        dockWindow.Content = null;
        dockWindow.Close();
    }
    
    public void RemoveSplitter(DockSplitter splitter, UserControl preservedItem)
    {
        if (splitter.Parent is not UserControl parent) return;

        // un-parent contents of splitter
        splitter.ItemA = null;
        splitter.ItemB = null;
        
        // replace splitter with preserved item.
        parent.Content = preservedItem;
    }

    public void RemoveTabGroup(DockTabGroup group)
    {
        // 1. Group is child of Window
        if (group.GetVisualRoot() is DockWindow window)
        {
            // remove window
            RemoveWindow(window);
            return;
        }
        
        // 2. Group is child of Root
        if (group.Parent == Root)
        {
            // remove group
            Root.Content = null;
            return;
        }
        
        // 3. Group is child of DockSplitter
        if (group.Parent?.Parent?.Parent is DockSplitter splitter)
        {
            // find preserved item
            UserControl preservedItem = splitter.ItemA == group 
                ? splitter.ItemB 
                : splitter.ItemA;
            
            // remove splitter
            RemoveSplitter(splitter, preservedItem);
        }
    }

    public void RemoveTab(DockTabGroup group, DockTab tab)
    {
        if (!group.TabList.Items.Contains(tab)) return;

        // 1. Group contains no other tabs
        if (group.TabList.Items.Count < 2)
        {
            // remove group
            RemoveTabGroup(group);
        }
        
        // 2. Group contains other tabs.
        else
        {
            // find next selected tab index
            int index = group.TabList.Items.IndexOf(tab);
            index = Math.Max(0, index - 1);
            
            // remove tab
            group.TabList.Items.Remove(tab);
            
            // select tab at index
            group.SelectedTab = group.TabList.Items[index] as DockTab;
        }
    }
#endregion Methods
}