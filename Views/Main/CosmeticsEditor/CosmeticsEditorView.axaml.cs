using Avalonia.Controls;

namespace SaturnEdit.Views.CosmeticsEditor;

public partial class CosmeticsEditorView : UserControl
{
    private readonly MainWindow mainWindow;
    
    public CosmeticsEditorView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
    }
}