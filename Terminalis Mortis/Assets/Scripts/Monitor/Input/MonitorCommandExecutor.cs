using System.Collections.Generic;

public class MonitorCommandExecutor
{
    private readonly Dictionary<MonitorCommandType, MonitorCommandResult> _results;
    private readonly MovementManager _movementManager;
    private readonly InteractionManager _interactionManager;

    public MonitorCommandExecutor(MovementManager movementManager, InteractionManager interactionManager)
    {
        _movementManager = movementManager;
        _interactionManager = interactionManager;

        _results = new Dictionary<MonitorCommandType, MonitorCommandResult>
        {
            { MonitorCommandType.Unknown, new MonitorCommandResult { Output = "Unknown command. Type help for commands." } },
            { MonitorCommandType.Attack, new MonitorCommandResult { Output = "Attack where? Use: attack <direction or target>" } },
            { MonitorCommandType.Interact, new MonitorCommandResult { Output = "Interact with what? Use: interact <target>" } },
            { MonitorCommandType.Move, new MonitorCommandResult { Output = "Move where? Use: move <north|south|east|west>" } },
            { MonitorCommandType.Help, new MonitorCommandResult { Output = "Commands: north | south | east | west | move <direction> | attack <direction or target> | interact <target> | help | clear" } },
            { MonitorCommandType.Clear, new MonitorCommandResult { ClearTerminal = true } }
        };
    }

    public MonitorCommandResult Execute(MonitorCommandType commandType, string parameters)
    {
        if (IsDirection(commandType))
        {
            return new MonitorCommandResult { Output = _movementManager.Move(commandType) };
        }

        if (commandType == MonitorCommandType.Move)
        {
            return new MonitorCommandResult { Output = _movementManager.Move(parameters) };
        }

        if (commandType == MonitorCommandType.Attack)
        {
            return new MonitorCommandResult { Output = _interactionManager.Attack(parameters) };
        }

        if (commandType == MonitorCommandType.Interact)
        {
            return new MonitorCommandResult { Output = _interactionManager.Interact(parameters) };
        }

        if (_results.TryGetValue(commandType, out MonitorCommandResult result))
        {
            result.OnExecute?.Invoke(parameters);
            return result;
        }

        return _results[MonitorCommandType.Unknown];
    }

    private static bool IsDirection(MonitorCommandType commandType)
    {
        return commandType == MonitorCommandType.North
            || commandType == MonitorCommandType.South
            || commandType == MonitorCommandType.East
            || commandType == MonitorCommandType.West;
    }
}
