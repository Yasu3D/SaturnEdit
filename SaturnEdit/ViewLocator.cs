using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using StaticViewLocator;

namespace SaturnEdit;

[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;

        Type type = data.GetType();
        if (s_views.TryGetValue(type, out Func<Control>? func))
        {
            return func.Invoke();
        }

        // Fallback for simple content
        return new TextBlock { Text = data.ToString() };
    }

    public bool Match(object? data)
    {
        return data is IDockable or not null;
    }
}