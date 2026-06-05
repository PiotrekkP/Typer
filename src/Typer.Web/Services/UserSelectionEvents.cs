namespace Typer.Web.Services;

public interface IUserSelectionEvents
{
    event Action? SelectionChanged;

    void NotifySelectionChanged();
}

public sealed class UserSelectionEvents : IUserSelectionEvents
{
    public event Action? SelectionChanged;

    public void NotifySelectionChanged() => SelectionChanged?.Invoke();
}
