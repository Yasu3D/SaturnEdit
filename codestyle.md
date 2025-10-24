### Member Order
Members should be defined in this order:
- Constructors / Initialize()
- EventHandlers
- Properties
- Fields
- Methods
- System Event Delegates
- UI Event Delegates

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
        
    private string exampleField = "example!";
        
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
unless it's a property with a backing field, or a different order makes more sense in context:
- `public`
- `internal`
- `private`

### Properties with Backing Fields
Backing fields of properties should be defined directly below the property, with no empty line in between:
```csharp
public int ExampleInt
{
    get => exampleInt;
    set
    {
        exampleInt = value;
    }
}
private int exampleInt = 10;
```

---

### Threading
All system event delegates that interact with the UI *must* wrap their code in a `Dispatcher.UIThread.Post()`.

Expensive UI event delegates should send work to other threads with `Task.Run()` to not block the UI thread.  
Unless the task needs to be awaited, discard it with `_ = Task.Run()` instead of awaiting.