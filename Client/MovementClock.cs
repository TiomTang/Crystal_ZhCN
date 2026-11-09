namespace Client
{
    internal static class MovementClock
    {
        private const long Interval = 100;

        // Preserve phase after a short delay. Never replay missed movement ticks.
        // A whole missed interval, initialization or reset starts a fresh schedule.
        internal static long NextDeadline(long now, long deadline)
        {
            if (deadline <= 0 || now < deadline || now - deadline >= Interval)
                return now + Interval;
            return deadline + Interval;
        }
    }
}
