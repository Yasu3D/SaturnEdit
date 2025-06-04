using Avalonia.Controls;

namespace SaturnEdit.Views.StageEditor;

public partial class StageEditorView : UserControl
{
    private readonly MainWindow mainWindow;
    
    public StageEditorView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
    }
}