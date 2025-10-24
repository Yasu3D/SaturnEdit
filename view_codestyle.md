### Region Blocks 
Methods, system event delegates and UI event delegates should be separated via region blocks:
```csharp
public class ExampleView : UserControl
{
    public ExampleView()
    {
        
    }
    
    public int ExampleInt { get; set; } = 10;
    
    public bool ExampleBool { get; set; } = true;    
    
#region Methods
    public void ExampleMethod()
    {
        
    }
    
    public void ExampleMethod2()
    {
        
    }
#endregion Methods

#region System Event Delegates
    private void OnExample1Changed(object? sender, ExampleArgs args)
    {
        
    }
    
    private void OnExample2Changed(object? sender, ExampleArgs args)
    {
        
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void Button_OnClick(object? sender, ExampleArgs args)
    {
        
    }
    
    private void TextBox_OnTextChanged(object? sender, ExampleArgs args)
    {
        
    }
#endregion UI Event Delegates
}
```

---

### Access Modifier Order
Members with certain access modifiers should be defined in this order,  
unless a different order makes more sense in context:
- `private`
- `internal`
- `public`

---

### Threading
All system event delegates that interact with the UI *must* wrap their code in a `Dispatcher.UIThread.Post()`.

Expensive UI event delegates should send work to other threads with `Task.Run()`, to not block the UI thread.  
Unless the task needs to be awaited, discard it with `_ = Task.Run()` instead of awaiting.