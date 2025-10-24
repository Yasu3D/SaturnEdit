using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using FluentIcons.Common;

namespace SaturnEdit.Windows.Dialogs.ModalDialog;

public enum ModalDialogResult
{
    Cancel = 0,
    Primary = 1,
    Secondary = 2,
    Tertiary = 3,
}

public partial class ModalDialogWindow : Window
{
    public ModalDialogWindow()
    {
        InitializeComponent();
    }

    public ModalDialogResult Result = ModalDialogResult.Cancel;
    public Icon DialogIcon = FluentIcons.Common.Icon.Home;
    public string WindowTitleKey = "";
    public string HeaderKey = "";
    public string ParagraphKey = "";
    public string ButtonPrimaryKey = "";
    public string ButtonSecondaryKey = "";
    public string ButtonTertiaryKey = "";

#region Methods
    public void InitializeDialog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            FluentIconDialog.Icon = DialogIcon;
            TextBlockWindowTitle.Bind(TextBlock.TextProperty, new DynamicResourceExtension(WindowTitleKey));
            TextBlockHeader.Bind(TextBlock.TextProperty, new DynamicResourceExtension(HeaderKey));
            TextBlockParagraph.Bind(TextBlock.TextProperty, new DynamicResourceExtension(ParagraphKey));
            TextBlockButtonPrimary.Bind(TextBlock.TextProperty, new DynamicResourceExtension(ButtonPrimaryKey));
            TextBlockButtonSecondary.Bind(TextBlock.TextProperty, new DynamicResourceExtension(ButtonSecondaryKey));
            TextBlockButtonTertiary.Bind(TextBlock.TextProperty, new DynamicResourceExtension(ButtonTertiaryKey));

            ButtonPrimary.IsVisible = ButtonPrimaryKey != "";
            ButtonSecondary.IsVisible = ButtonSecondaryKey != "";
            ButtonTertiary.IsVisible = ButtonTertiaryKey != "";
            
            Title = TextBlockWindowTitle.Text;
        });
    }
#endregion Methods

#region UI Event Delegates
    private void ButtonPrimary_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Primary;
        Close();
    }

    private void ButtonSecondary_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Secondary;
        Close();
    }
    
    private void ButtonTertiary_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Tertiary;
        Close();
    }
#endregion UI Event Delegates
}