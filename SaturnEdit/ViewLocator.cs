using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;

namespace SaturnEdit;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            IDockable dockable when !string.IsNullOrEmpty(dockable.Title) => 
                new TextBox { Text = $"Content for {dockable.Title}", AcceptsReturn = true },
            string text => new TextBox { Text = text, AcceptsReturn = true },
            _ => new TextBlock { Text = data?.ToString() ?? "No Content" }
        };
    }

    public bool Match(object? data) => true;
}