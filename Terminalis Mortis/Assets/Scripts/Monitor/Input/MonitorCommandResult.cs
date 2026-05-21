using System;

public class MonitorCommandResult
{
    public bool ClearTerminal { get; set; }
    public string Output { get; set; }
    public Action<string> OnExecute { get; set; } = null;
}
