using Avalonia.Controls;

namespace SaturnEdit.Views.StageEditor;

public partial class StageEditorView : UserControl
{
    private readonly MainView mainView;
    
    public StageEditorView(MainView mainView)
    {
        InitializeComponent();
        this.mainView = mainView;
    }
}