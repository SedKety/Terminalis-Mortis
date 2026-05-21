public class InteractionManager
{
    private readonly GridGenerator _gridGenerator;

    public InteractionManager(GridGenerator gridGenerator)
    {
        _gridGenerator = gridGenerator;
    }

    public string Attack(string targetOrDirection)
    {
        if (_gridGenerator != null)
        {
            return _gridGenerator.Attack(targetOrDirection);
        }

        if (string.IsNullOrWhiteSpace(targetOrDirection))
        {
            return "Attack where? Use: attack <direction or target>";
        }

        return "Attacking " + targetOrDirection.Trim() + "...";
    }

    public string Interact(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return "Interact with what? Use: interact <target>";
        }

        return "Interacting with " + target.Trim() + "...";
    }

    public string GetCurrentPath()
    {
        if (_gridGenerator == null)
        {
            return "Path unavailable.";
        }

        return _gridGenerator.GetCurrentPath();
    }

    public string GetStatus()
    {
        if (_gridGenerator == null)
        {
            return "Status unavailable.";
        }

        return _gridGenerator.GetStatusOverview();
    }

    public string GetTutorial()
    {
        if (_gridGenerator == null)
        {
            return "Tutorial unavailable.";
        }

        return _gridGenerator.GetQuickTutorial();
    }

    public string LocateNext(string parameters)
    {
        if (_gridGenerator == null)
        {
            return "Locate unavailable.";
        }

        return _gridGenerator.LocateNextFile();
    }
}
