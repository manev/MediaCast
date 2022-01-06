namespace MediaCast;

public class ActionCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    private readonly Action<object> _executeAction;

    public ActionCommand(Action<object> executeAction)
    {
        _executeAction = executeAction;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        _executeAction?.Invoke(parameter);
    }
}
