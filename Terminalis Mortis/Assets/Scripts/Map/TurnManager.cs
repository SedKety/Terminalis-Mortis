using System;

public class TurnManager
{
    public event Action<int> OnTurnAdvanced;

    public int CurrentTurn { get; private set; }

    public void AdvanceTurn()
    {
        CurrentTurn++;
        OnTurnAdvanced?.Invoke(CurrentTurn);
    }
}
