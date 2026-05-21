using System;

public class DungeonFileNavigator
{
    private readonly string _rootPath;
    private readonly string[] _files =
    {
        "Malware",
        "Virus",
        "Trojans",
        "Ransomware",
        "Worm",
        "Rootkit"
    };

    public int CurrentFileIndex { get; private set; }
    public bool IsComplete { get; private set; }
    public int FileCount => _files.Length;

    public DungeonFileNavigator(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || rootPath.IndexOf("dungeon", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            _rootPath = "Users/FreeRobux";
        }
        else
        {
            _rootPath = rootPath.Trim('/');
        }
    }

    public string GetCurrentPath()
    {
        int index = Math.Clamp(CurrentFileIndex, 0, _files.Length - 1);
        return _rootPath + "/" + _files[index];
    }

    public string GetCurrentFileName()
    {
        int index = Math.Clamp(CurrentFileIndex, 0, _files.Length - 1);
        return _files[index];
    }

    public bool TryAdvance(out string nextPath)
    {
        if (IsComplete)
        {
            nextPath = GetCurrentPath();
            return false;
        }

        if (CurrentFileIndex + 1 < _files.Length)
        {
            CurrentFileIndex++;
            nextPath = GetCurrentPath();
            return true;
        }

        IsComplete = true;
        nextPath = GetCurrentPath();
        return false;
    }

    public bool TryGetNextPath(out string nextPath)
    {
        if (IsComplete)
        {
            nextPath = GetCurrentPath();
            return false;
        }

        if (CurrentFileIndex + 1 < _files.Length)
        {
            nextPath = _rootPath + "/" + _files[CurrentFileIndex + 1];
            return true;
        }

        nextPath = GetCurrentPath();
        return false;
    }
}
