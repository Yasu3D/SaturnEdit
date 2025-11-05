using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SaturnEdit.Docking;

public partial class DockSplitter : UserControl
{
    public DockSplitter()
    {
        InitializeComponent();
        Update();
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
                double width = Bounds.Width;
                double itemWidth = ItemContainerA.Bounds.Width;
                value = itemWidth / width;
            }
            else
            {
                double height = Bounds.Height;
                double itemHeight = ItemContainerA.Bounds.Height;
                value = itemHeight / height;
            }

            return value;
        }
        set
        {
            value = Math.Clamp(value, 0, 1);

            if (Direction == GridResizeDirection.Columns)
            {
                double a = Bounds.Width * value;
                double b = Bounds.Width - a - 1;

                SplitGrid.ColumnDefinitions =
                [
                    new ColumnDefinition(a, GridUnitType.Star) { MinWidth = SplitterMinWidth },
                    new ColumnDefinition(1, GridUnitType.Pixel),
                    new ColumnDefinition(b, GridUnitType.Star) { MinWidth = SplitterMinWidth },
                ];
            }
            else
            {
                double a = Bounds.Height * value;
                double b = Bounds.Height - a - 1;

                SplitGrid.RowDefinitions =
                [
                    new RowDefinition(a, GridUnitType.Star) { MinHeight = SplitterMinWidth },
                    new RowDefinition(1, GridUnitType.Pixel),
                    new RowDefinition(b, GridUnitType.Star) { MinHeight = SplitterMinWidth },
                ];
            }
        }
    }
    
    private const double SplitterMinWidth = 100;
    
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