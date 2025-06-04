using Avalonia.Controls;

namespace SaturnEdit.Views.CosmeticsEditor;

public partial class CosmeticsEditorView : UserControl
{
    private readonly MainView mainView;
    
    public CosmeticsEditorView(MainView mainView)
    {
        InitializeComponent();
        this.mainView = mainView;
    }
}