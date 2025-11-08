using System.Globalization;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit.Controls;

public partial class ProofreaderProblemItem : UserControl
{
    public ProofreaderProblemItem()
    {
        InitializeComponent();
    }

    public ProofreaderProblem? Problem { get; private set; } = null;

#region Methods
    public void SetProblem(ProofreaderProblem problem)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Problem = problem;
            if (Problem == null) return;

            TextBlockTimestamp.Text = $"{problem.Measure}' {problem.Tick}";
            TextBlockPosition.Text = problem.Position == -1 ? "-" : problem.Position.ToString(CultureInfo.InvariantCulture);
            TextBlockSize.Text = problem.Size == -1 ? "-" : problem.Size.ToString(CultureInfo.InvariantCulture);
            
            TextBlockType.Bind(TextBlock.TextProperty, new DynamicResourceExtension(problem.TypeKey));
            TextBlockProblem.Bind(TextBlock.TextProperty, new DynamicResourceExtension(problem.ProblemKey));
        });
    }
#endregion Methods
}