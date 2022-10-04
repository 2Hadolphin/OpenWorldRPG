namespace Return.Framework.Stats
{
    public enum StatOption
    {
        None,
        /// <summary>Add to the Stat Value </summary>
        AddValue,
        /// <summary>Set a new Stat Value </summary>
        SetValue,
        /// <summary>Remove to the Stat Value </summary>
        SubstractValue,
        /// <summary>Modify Add|Remove the Stat MAX Value </summary>
        ModifyMaxValue,
        /// <summary>Set a new Stat MAX Value </summary>
        SetMaxValue,
        /// <summary>Enable the Degeneration </summary>
        Degenerate,
        /// <summary>Disable the Degeneration </summary>
        StopDegenerate,
        /// <summary>Enable the Regeneration </summary>
        Regenerate,
        /// <summary>Disable the Regeneration </summary>
        StopRegenerate,
        /// <summary>Reset the Stat to the Default Min or Max Value </summary>
        Reset,
        /// <summary>Reduce the Value of the Stat by a percent</summary>
        ReduceByPercent,
        /// <summary>Increase the Value of the Stat by a percent</summary>
        IncreaseByPercent,
        /// <summary>Sets the multiplier of a stat</summary>
        Multiplier,
        /// <summary>Reset the Stat to the Max Value</summary>
        ResetToMax,
        /// <summary>Reset the Stat to the Min Value</summary>
        ResetToMin,
    }
}