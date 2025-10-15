using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

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
    
    private async void InitializeControl()
    {
        try
        {
            // race conditions... yay :(
            await Task.Delay(1);
            
            TextBlockChannelName.Text = ChannelName;
            ButtonSound.IsVisible = HasSoundButton;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}