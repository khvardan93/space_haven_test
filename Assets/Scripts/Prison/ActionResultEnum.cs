namespace Prison
{
    /// <summary>Outcome of a player action. The UI uses it to grey out buttons and explain why.</summary>
    public enum ActionResultEnum
    {
        Ok,
        InvalidSlot,
        Occupied,
        Empty,
        MaxLevel,
        NotEnoughResources,
        NotEnoughLaser
    }
}
