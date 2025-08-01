using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using SaturnEdit.Systems;

namespace SaturnEdit;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
            
        base.OnFrameworkInitializationCompleted();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        SetTheme();
        SetLocale();
    }
    
    private void SetTheme()
    {
        RequestedThemeVariant = SettingsSystem.EditorSettings.Theme switch
        {
            EditorSettings.EditorThemeOptions.Light => ThemeVariant.Light,
            EditorSettings.EditorThemeOptions.Dark => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private void SetLocale()
    {
        try
        {
            if (Current == null) return;

            ResourceInclude? oldLocale = Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("Locales") ?? false);

            Uri uri = new($"avares://SaturnEdit/Assets/Locales/{SettingsSystem.EditorSettings.Locale}.axaml");
            ResourceInclude newLocale = new(uri) { Source = uri };

            if (oldLocale != null) Current.Resources.MergedDictionaries.Remove(oldLocale);
            Current.Resources.MergedDictionaries.Add(newLocale);
        }
        catch
        {
            // Default to known locale if something explodes. That'll invoke the SettingsChanged event, and essentially "recursively" call this method again.
            SettingsSystem.EditorSettings.Locale = "en-US";
        }
    }
}