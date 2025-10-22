using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentIcons.Common;
using SaturnData.Notation.Core;

namespace SaturnEdit.Controls;

public partial class LayerListItem : UserControl
{
    public LayerListItem()
    {
        InitializeComponent();
    }

    public event EventHandler? NameChanged;
    public event EventHandler? VisibilityChanged;
    
    public Layer Layer { get; private set; } = null!;
    private bool blockEvents = false;

    public void SetLayer(Layer layer)
    {
        blockEvents = true;
        
        Layer = layer;
        TextBoxLayerName.Text = layer.Name;
        IconLayerVisibility.Icon = layer.Visible ? Icon.Eye : Icon.EyeOff;

        blockEvents = false;
    }


    private void ButtonLayerVisibility_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ButtonLayerVisibility == null) return;
        
        VisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TextBoxLayerName_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxLayerName == null) return;
        
        NameChanged?.Invoke(this, EventArgs.Empty);
    }
}