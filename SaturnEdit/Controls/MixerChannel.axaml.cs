using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SaturnEdit.Controls;

public partial class MixerChannel : UserControl
{
    public MixerChannel()
    {
        InitializeComponent();
        InitializeControl();
    }

    public static readonly StyledProperty<string> ChannelNameProperty = AvaloniaProperty.Register<MixerChannel, string>(nameof(ChannelName), defaultValue: "");
    public string ChannelName
    {
        get => GetValue(ChannelNameProperty);
        set => SetValue(ChannelNameProperty, value);
    }
    
    public static readonly StyledProperty<bool> HasSoundButtonProperty = AvaloniaProperty.Register<MixerChannel, bool>(nameof(HasSoundButton), defaultValue: true);
    public bool HasSoundButton
    {
        get => GetValue(HasSoundButtonProperty);
        set => SetValue(HasSoundButtonProperty, value);
    }
    
    public static readonly StyledProperty<bool> HasSoloButtonProperty = AvaloniaProperty.Register<MixerChannel, bool>(nameof(HasSoloButton), defaultValue: true);
    public bool HasSoloButton
    {
        get => GetValue(HasSoloButtonProperty);
        set => SetValue(HasSoloButtonProperty, value);
    }
    
    private async void InitializeControl()
    {
        try
        {
            // race conditions... yay :(
            await Task.Delay(1);
            
            TextBlockChannelName.Text = ChannelName;
            ButtonSolo.IsVisible = HasSoloButton;
            ButtonSound.IsVisible = HasSoundButton;
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}