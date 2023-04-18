using System.Diagnostics;

namespace Engine.Services
{
    public class StopWatchLogger
    {
        private readonly Stopwatch stopwatch;

        public StopWatchLogger()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Log(string message)
        {
            stopwatch.Stop();
            Logger.LogDebug("StopLog", $"{message}, Time: {stopwatch.ElapsedMilliseconds}, Ticks: {stopwatch.ElapsedTicks}");
            stopwatch.Restart();
        }
    }
}