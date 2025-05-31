using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;

namespace SaturnEdit;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void SetLocale(string key)
    {
        if (Current == null) return;
        
        ResourceInclude? oldLocale = Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("Locales") ?? false);
        
        Uri uri = new($"avares://SaturnEdit/Assets/Locales/{key}.axaml");
        ResourceInclude newLocale = new(uri) { Source = uri };

        if (oldLocale != null) Current.Resources.MergedDictionaries.Remove(oldLocale);
        Current.Resources.MergedDictionaries.Add(newLocale);
    }
}