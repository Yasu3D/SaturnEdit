using System;
using Avalonia;
using Avalonia.Controls;

namespace SaturnEdit.Docking;

public partial class DockSplitter : UserControl
{
    public DockSplitter()
    {
        InitializeComponent();
        Update();
    }

    public DockSplitter(UserControl destination, UserControl inserted, TargetSide targetSide, double proportion = 0.5)
    {
        if (targetSide is TargetSide.Center or TargetSide.None)
        {
            throw new ArgumentException("Target Side must be Top, Bottom, Left, or Right.");
        }
        
        InitializeComponent();
        
        if (targetSide == TargetSide.Left)
        {
            ItemA.Content = inserted;
            ItemB.Content = destination;
            Direction = GridResizeDirection.Columns;
        }
        else if (targetSide == TargetSide.Right)
        {
            ItemA.Content = destination;
            ItemB.Content = inserted;
            Direction = GridResizeDirection.Columns;
        }
        else if (targetSide == TargetSide.Top)
        {
            ItemA.Content = inserted;
            ItemB.Content = destination;
            Direction = GridResizeDirection.Rows;
        }
        else if (targetSide == TargetSide.Bottom)
        {
            ItemA.Content = destination;
            ItemB.Content = inserted;
            Direction = GridResizeDirection.Rows;
        }
        
        Update();
        Proportion = proportion;
    }

    public static readonly StyledProperty<GridResizeDirection> DirectionProperty = AvaloniaProperty.Register<DockTarget, GridResizeDirection>(nameof(Direction), defaultValue: GridResizeDirection.Columns);
    public GridResizeDirection Direction
    {
        get => GetValue(DirectionProperty);
        set
        {
            SetValue(DirectionProperty, value);
            Update();
        }
    }

    public double Proportion
    {
        get
        {
            double value;
            if (Direction == GridResizeDirection.Columns)
            {
                double totalWidth = SplitGrid.ColumnDefinitions[0].Width.Value + SplitGrid.ColumnDefinitions[2].Width.Value;
                value = SplitGrid.ColumnDefinitions[0].Width.Value / totalWidth;
            }
            else
            {
                double totalHeight = SplitGrid.RowDefinitions[0].Height.Value + SplitGrid.RowDefinitions[2].Height.Value;
                value = SplitGrid.RowDefinitions[0].Height.Value / totalHeight;
            }

            return value;
        }
        set
        {
            value = Math.Clamp(value, 0, 1);

            if (Direction == GridResizeDirection.Columns)
            {
                SplitGrid.ColumnDefinitions =
                [
                    new ColumnDefinition(value, GridUnitType.Star) { MinWidth = SplitterMinWidth },
                    new ColumnDefinition(1, GridUnitType.Pixel),
                    new ColumnDefinition(1 - value, GridUnitType.Star) { MinWidth = SplitterMinWidth },
                ];
            }
            else
            {
                SplitGrid.RowDefinitions =
                [
                    new RowDefinition(value, GridUnitType.Star) { MinHeight = SplitterMinWidth },
                    new RowDefinition(1, GridUnitType.Pixel),
                    new RowDefinition(1 - value, GridUnitType.Star) { MinHeight = SplitterMinWidth },
                ];
            }
        }
    }
    
    private const double SplitterMinWidth = 35;
    
#region Methods
    private void Update()
    {
        if (Direction == GridResizeDirection.Columns)
        {
            SplitGrid.ColumnDefinitions =
            [
                new ColumnDefinition(1, GridUnitType.Star) { MinWidth = SplitterMinWidth }, 
                new ColumnDefinition(1, GridUnitType.Pixel), 
                new ColumnDefinition(1, GridUnitType.Star) { MinWidth = SplitterMinWidth },
            ];
            SplitGrid.RowDefinitions = [];

            SplitterLine.Width = 1;
            SplitterLine.Height = double.NaN;
            
            SplitterLine.SetValue(Grid.RowProperty, 0);
            SplitterLine.SetValue(Grid.ColumnProperty, 1);
            
            Splitter.SetValue(Grid.RowProperty, 0);
            Splitter.SetValue(Grid.ColumnProperty, 1);
            
            ItemB.SetValue(Grid.RowProperty, 0);
            ItemB.SetValue(Grid.ColumnProperty, 2);

            Splitter.ResizeDirection = GridResizeDirection.Columns;
        }
        else
        {
            SplitGrid.ColumnDefinitions = [];
            SplitGrid.RowDefinitions = 
                [
                    new RowDefinition(1, GridUnitType.Star) { MinHeight = SplitterMinWidth }, 
                    new RowDefinition(1, GridUnitType.Pixel), 
                    new RowDefinition(1, GridUnitType.Star) { MinHeight = SplitterMinWidth },
                ];
            
            SplitterLine.Width = double.NaN;
            SplitterLine.Height = 1;

            SplitterLine.SetValue(Grid.RowProperty, 1);
            SplitterLine.SetValue(Grid.ColumnProperty, 0);
            
            Splitter.SetValue(Grid.RowProperty, 1);
            Splitter.SetValue(Grid.ColumnProperty, 0);
            
            ItemB.SetValue(Grid.RowProperty, 2);
            ItemB.SetValue(Grid.ColumnProperty, 0);

            Splitter.ResizeDirection = GridResizeDirection.Rows;
        }
    }
#endregion Methods
}