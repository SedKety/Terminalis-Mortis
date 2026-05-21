using UnityEngine;

public class MovementManager
{
    private readonly Transform _target;
    private readonly float _stepDistance;

    public MovementManager(Transform target, float stepDistance)
    {
        _target = target;
        _stepDistance = stepDistance;
    }

    public string Move(MonitorCommandType directionCommand)
    {
        switch (directionCommand)
        {
            case MonitorCommandType.North:
                return Move(Vector3.forward, "North");
            case MonitorCommandType.South:
                return Move(Vector3.back, "South");
            case MonitorCommandType.East:
                return Move(Vector3.right, "East");
            case MonitorCommandType.West:
                return Move(Vector3.left, "West");
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
