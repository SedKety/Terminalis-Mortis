using System;
using System.Collections.Generic;

public class MonitorCommandParser
{
    private readonly Dictionary<string, MonitorCommandType> _commands = new Dictionary<string, MonitorCommandType>(StringComparer.OrdinalIgnoreCase)
    {
        { "north", MonitorCommandType.North },
        { "n", MonitorCommandType.North },
        { "south", MonitorCommandType.South },
        { "s", MonitorCommandType.South },
        { "east", MonitorCommandType.East },
        { "e", MonitorCommandType.East },
        { "west", MonitorCommandType.West },
        { "w", MonitorCommandType.West },
        { "help", MonitorCommandType.Help },
        { "attack", MonitorCommandType.Attack },
        { "interact", MonitorCommandType.Interact },
        { "use", MonitorCommandType.Interact },
        { "move", MonitorCommandType.Move },
        { "clear", MonitorCommandType.Clear },
        { "cls", MonitorCommandType.Clear }
    };

    public MonitorCommandType Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return MonitorCommandType.Unknown;
        }

        string commandToken = input.Trim();
        int firstSpaceIndex = commandToken.IndexOf(' ');

        if (firstSpaceIndex >= 0)
        {
            commandToken = commandToken.Substring(0, firstSpaceIndex);
        }

        if (_commands.TryGetValue(commandToken, out MonitorCommandType commandType))
        {
            return commandType;
        }

        return MonitorCommandType.Unknown;
    }

    public MonitorCommandType ParseWithParameters(string input, out string parameters)
    {
        parameters = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return MonitorCommandType.Unknown;
        }

        string trimmedInput = input.Trim();
        int firstSpaceIndex = trimmedInput.IndexOf(' ');

        string commandToken;
        string parsedParameters;

        if (firstSpaceIndex >= 0)
        {
            commandToken = trimmedInput.Substring(0, firstSpaceIndex);
            parsedParameters = trimmedInput.Substring(firstSpaceIndex + 1).Trim();
        }
        else
        {
            commandToken = trimmedInput;
            parsedParameters = string.Empty;
        }

        if (!_commands.TryGetValue(commandToken, out MonitorCommandType commandType))
        {
            commandType = MonitorCommandType.Unknown;
            parsedParameters = trimmedInput;
        }

        parameters = parsedParameters;
        return commandType;
    }
}
