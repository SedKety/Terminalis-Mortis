using UnityEngine;

public class MovementManager
{
    private readonly Transform _target;
    private readonly float _stepDistance;
    private readonly GridGenerator _gridGenerator;

    public MovementManager(Transform target, float stepDistance)
    {
        _target = target;
        _stepDistance = stepDistance;
        _gridGenerator = null;
    }

    public MovementManager(GridGenerator gridGenerator)
    {
        _target = null;
        _stepDistance = 0f;
        _gridGenerator = gridGenerator;
    }

    public string Move(MonitorCommandType directionCommand)
    {
        if (_gridGenerator != null)
        {
            _gridGenerator.TryMovePlayer(directionCommand, out string result);
            return result;
        }

        switch (directionCommand)
        {
            case MonitorCommandType.North:
                return Move(Vector3.forward, "North");
            case MonitorCommandType.South:
                return Move(Vector3.back, "South");
            case MonitorCommandType.East:
                return Move(Vector3.left, "East");
            case MonitorCommandType.West:
                return Move(Vector3.right, "West");
            default:
                return "Move where? Use: move <north|south|east|west>";
        }
    }

    public string Move(string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
        {
            return "Move where? Use: move <north|south|east|west>";
        }

        string token = direction.Trim().ToLowerInvariant();

        switch (token)
        {
            case "north":
            case "n":
                return Move(MonitorCommandType.North);
            case "south":
            case "s":
                return Move(MonitorCommandType.South);
            case "east":
            case "e":
                return Move(MonitorCommandType.East);
            case "west":
            case "w":
                return Move(MonitorCommandType.West);
            default:
                return "Unknown direction. Use: north | south | east | west";
        }
    }

    private string Move(Vector3 direction, string directionName)
    {
        if (_target == null)
        {
            return "No movement target set.";
        }

        _target.position += direction * _stepDistance;
        return "Moving " + directionName + "...";
    }
}
