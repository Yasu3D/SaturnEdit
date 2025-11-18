using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnData.Notation.Core;

namespace SaturnEdit.Controls;

public partial class BookmarkMarker : UserControl
{
    public BookmarkMarker()
    {
        InitializeComponent();
    }

    public event EventHandler? Click;

    public Bookmark? Bookmark { get; private set; } = null;

#region Methods
    public void SetBookmark(Bookmark bookmark, double sliderWidth, float sliderMaximum)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Bookmark = bookmark;

            BorderMarker.BorderBrush = new SolidColorBrush(bookmark.Color);
            BorderMarker.Background = new SolidColorBrush(bookmark.Color - 0x80000000);
            
            TextBlockColor.Text = $"#{bookmark.Color - 0xFF000000:X6}";
            TextBlockMessage.Text = bookmark.Message;

            double t = sliderMaximum == 0 ? 0 : bookmark.Timestamp.Time / sliderMaximum;
            double offset = t * (sliderWidth - 11);
            Margin = new(offset, 0, 0, 0);
        });
    }
#endregion Methods
    
#region UI Event Handlers
    private void Control_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed) return;
        
        Click?.Invoke(this, EventArgs.Empty);
    }
#endregion UI Event Handlers
}