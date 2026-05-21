public class InteractionManager
{
    public string Attack(string targetOrDirection)
    {
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
}
