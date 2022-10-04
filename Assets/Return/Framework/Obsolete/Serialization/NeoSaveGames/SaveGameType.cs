using System;

namespace NeoSaveGames
{
    public enum SaveGameType
    {
        Quicksave,
        Autosave,
        Manual
    }

    [Flags]
    public enum SaveGameTypeFilter
    {
        None = 0,
        Quicksave = 1,
        Autosave = 2,
        Manual = 4,
        All = 7
    }
}
