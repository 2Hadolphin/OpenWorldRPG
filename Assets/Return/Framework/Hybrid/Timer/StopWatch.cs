namespace Return.Timers
{
    public partial class Timer
    {
        public class StopWatch
        {
            public float TimeStart;

            public virtual float GetPassTime()
            {
                return Time - TimeStart;
            }
        }



    }
}
