namespace NeoFPS
{
    public interface IBreathHandler
    {
        float breathCounter { get; }
        float breathStrength { get; }

        float GetBreathCycle();
        float GetBreathCycle(float offset);
        float GetBreathCycle(float offset, float multiplier);
    }
}
