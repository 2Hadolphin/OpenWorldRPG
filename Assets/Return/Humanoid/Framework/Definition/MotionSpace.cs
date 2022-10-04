namespace Return.Humanoid.Motion
{
    public enum MotionType
    {
        Interactor,
        Inertia,
        Plot,
        Majeure

    }
    public enum MotionSpace
    {
        Standard = 1 << 1,
        CameraSpace = 1 << 2,
        RTS = 1 << 3,
    }
}