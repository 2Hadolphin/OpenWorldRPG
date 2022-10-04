namespace NeoSaveGames.Serialization
{
    public enum SerializationContext : byte
    {
        Root,
        MetaData,
        Scene,
        DontDestroyOnLoad,
        GameObject,
        ObjectNeoSerialized,
        ObjectNeoFormatted,
        ObjectUnformatted,
        ComponentNeoSerialized,
        ComponentNeoFormatted,
        ComponentUnformatted,
        ScriptableObjectNeoSerialized,
        ScriptableObjectNeoFormatted,
        ScriptableObjectUnformatted
    }
}
