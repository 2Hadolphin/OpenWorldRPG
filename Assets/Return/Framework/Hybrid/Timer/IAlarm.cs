using System;

public interface IAlarm : IDisposable
{
    /// <summary>
    /// Invoke during update phase with custom frequency.
    /// </summary>
    event Action Bell;

    /// <summary>
    /// Invoke and pass the delta time since last call.
    /// </summary>
    event Action<float> GapBell;
}
