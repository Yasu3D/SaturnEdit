using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace SaturnEdit.Docking;

// TODO: Set Floating Window position via PixelPoint p = group.PointToScreen(new(0, 0));
public partial class DockArea : UserControl
{
    public DockArea()
    {
        Instance = this;
        InitializeComponent();

        WindowDragStarted += OnWindowDragStarted;
        WindowDragEnded += OnWindowDragEnded;
        WindowDragged += OnWindowDragged;
        
        //DockTabGroup groupA = new();
        //groupA.TabList.Items.Add(new DockTab(new CursorView(), Icon.Agents, "ChartEditor.Cursor"));
//
        //DockTabGroup groupB = new();
        //groupB.TabList.Items.Add(new DockTab(new ChartPropertiesView(), Icon.Agents, "ChartEditor.ChartProperties"));
        //groupB.TabList.Items.Add(new DockTab(new EventListView(), Icon.Agents, "ChartEditor.EventList"));
//
        //DockSplitter splitterA = new(groupA, groupB, TargetSide.Top, 0.75);
        //
        //DockTabGroup groupC = new();
        //groupC.TabList.Items.Add(new DockTab(new ChartView3D(), Icon.Agents, "ChartEditor.ChartView3D"));
        //groupC.TabList.Items.Add(new DockTab(new ChartViewTxt(), Icon.Agents, "ChartEditor.ChartViewTxt"));
        //
        //DockSplitter splitterB = new(splitterA, groupC, TargetSide.Left, 0.66);
        //
        //Root.Content = splitterB;
        //
        //DockTabGroup groupD = new();
        //groupD.TabList.Items.Add(new DockTab(new InspectorView(), Icon.Agents, "ChartEditor.Inspector"));
    }

    public static DockArea? Instance { get; private set; } = null;
    
    public event EventHandler? WindowDragStarted;
    public event EventHandler? WindowDragEnded;
    public event EventHandler? WindowDragged;
    
    public Point WindowOffset;
    public DockTabGroup? DraggedGroup = null;

    public PixelPoint StartPosition;
    
    public PixelPoint PointerPosition
    {
        get => pointerPosition;
        set
        {
            pointerPosition = value;
            WindowDragged?.Invoke(null, EventArgs.Empty);
        }
    }
    private PixelPoint pointerPosition;
    
    public bool WindowDragActive
    {
        get => windowDragActive;
        set
        {
            windowDragActive = value;
            
            if (value == true)
            {
                WindowDragStarted?.Invoke(null, EventArgs.Empty);
            }

            if (value == false)
            {
                WindowDragEnded?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private bool windowDragActive = false;
    
#region Methods
    public bool HitTest(Rect rect)
    {
        double x = PointerPosition.X;
        double y = PointerPosition.Y;
        
        bool insideX = x > rect.Left && x < rect.Right;
        bool insideY = y > rect.Top && y < rect.Bottom;
        
        return insideX && insideY;
    }

    public static Rect ScreenBounds(Visual visual)
    {
        PixelPoint topLeft = visual.PointToScreen(new(0, 0));
        return new(topLeft.X, topLeft.Y, visual.Bounds.Width, visual.Bounds.Height);
    }
    
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
                Root.Content = null;
                
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

    public void Float(UserControl item)
    {
        // TODO: merge FloatTabGroup and FloatTab
    }

    public void Popup(DockTab tab)
    {
        if (MainWindow.Instance == null) return;

        DockTabGroup group = new();
        group.TabList.Items.Add(tab);
        
        DockWindow window = new()
        {
            WindowContent = { Content = group },
            Width = 500,
            Height = 500,
        };
        
        window.Show(MainWindow.Instance);
    }
    
    public void FloatTabGroup(DockTabGroup group)
    {
        if (MainWindow.Instance == null) return;

        List<object?> tabs = group.TabList.Items.ToList();
        
        PixelPoint p = group.PointToScreen(new(0, 0));
        
        RemoveTabGroup(group);

        DockTabGroup newGroup = new();
        foreach (object? tab in tabs)
        {
            newGroup.TabList.Items.Add(tab);
        }
        
        DockWindow window = new()
        {
            WindowContent = { Content = newGroup },
            Width = group.Bounds.Width,
            Height = group.Bounds.Height,
            Position = p,
        };

        window.Show(MainWindow.Instance);
    }
    
    public void FloatTab(DockTabGroup group, DockTab tab)
    {
        if (MainWindow.Instance == null) return;
        
        RemoveTab(group, tab);

        DockTabGroup newGroup = new();
        newGroup.TabList.Items.Add(tab);
        
        DockWindow window = new()
        {
            WindowContent = { Content = newGroup },
            Width = group.Bounds.Width,
            Height = group.Bounds.Height,
        };
        
        window.Show(MainWindow.Instance);
    }
    
    public void RemoveWindow(DockWindow dockWindow)
    {
        dockWindow.WindowContent.Content = null;
        dockWindow.Close();
    }
    
    public void RemoveSplitter(DockSplitter splitter, UserControl preservedItem)
    {
        if (splitter.Parent is not UserControl parent) return;
        
        // un-parent splitter.
        parent.Content = null;
        
        // un-parent contents of splitter.
        splitter.ItemA.Content = null;
        splitter.ItemB.Content = null;
        
        // insert preserved item in splitter's place.
        parent.Content = preservedItem;
    }

    public void RemoveTabGroup(DockTabGroup group)
    {
        group.TabList.Items.Clear();
        group.TabContentContainer.Content = null;
        
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
            UserControl? preservedItem = Equals(splitter.ItemA.Content, group) 
                ? (UserControl?)splitter.ItemB.Content 
                : (UserControl?)splitter.ItemA.Content;
            
            if (preservedItem == null) return;
            
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
    
    private void UpdateTarget()
    {
        if (VisualRoot == null) return;
        
        if (!WindowDragActive)
        {
            Target.IsVisible = false;
            return;
        }

        Target.ShowSideTargets = Root.Content != null;
        Target.ShowCenterTarget = Root.Content == null;
            
        Rect bounds = ScreenBounds(this);
        Target.IsVisible = HitTest(bounds);
    }
#endregion Methods

#region System Event Delegates
    private void OnWindowDragStarted(object? sender, EventArgs e) => UpdateTarget();
    
    private void OnWindowDragEnded(object? sender, EventArgs e)
    {
        Target.IsVisible = false;
        
        if (Target.TargetSide != TargetSide.None && DraggedGroup != null)
        {
            Dock(Target, DraggedGroup);
        }

        Target.TargetSide = TargetSide.None;
    }

    private void OnWindowDragged(object? sender, EventArgs e) => UpdateTarget();
#endregion System Event Delegates
    
#region UI Event Delegates
    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (!WindowDragActive) return;
        Target.IsVisible = true;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (!WindowDragActive) return;
        Target.IsVisible = false;
    }
#endregion UI Event Delegates
}