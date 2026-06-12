using System.Windows.Input;

namespace AutoPageTurner.Commands;

public sealed class RelayCommand : ICommand
{
    private readonly Action execute;

    private readonly Func<bool>? canExecute;

    public RelayCommand(
        Action execute,
        Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(
        object? parameter)
    {
        return canExecute?.Invoke() ?? true;
    }

    public void Execute(
        object? parameter)
    {
        execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(
            this,
            EventArgs.Empty);
    }

    public event EventHandler? CanExecuteChanged;
}
