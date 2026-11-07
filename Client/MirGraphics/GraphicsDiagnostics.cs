using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Client.MirGraphics
{
    internal enum GraphicsMetric
    {
        Scene, Present, TextureRead, TextureDecode, TextureUpload, Cleanup, Render
    }

    /// <summary>
    /// Opt-in CPU wall-clock diagnostics, not GPU timings. Set MIR2_GRAPHICS_TRACE=1
    /// before startup. Metric timings are inclusive: nested metrics overlap and must
    /// not be summed as exclusive frame costs. No paths, settings or player data are logged.
    /// </summary>
    internal static class GraphicsDiagnostics
    {
        private const int MetricCount = 7;
        private const int QueueCapacity = 2;
        private static readonly object Gate = new object();
        private static readonly long WindowTicks = Stopwatch.Frequency * 5L;
        private static readonly double TickMilliseconds = 1000.0 / Stopwatch.Frequency;
        private static volatile bool _enabled;
        private static bool _stopped;
        private static string _failureReason;
        private static long _droppedWindows;
        private static long _previousFrame;
        private static Window _window;
        private static BlockingCollection<Window> _queue;
        private static Thread _writer;
        private static double[] _frameBounds;

        public static bool Enabled => _enabled;
        // Fixed diagnostic category only; never exposes filesystem paths or exception text.
        public static string FailureReason => Volatile.Read(ref _failureReason);
        public static long DroppedWindows => Interlocked.Read(ref _droppedWindows);

        static GraphicsDiagnostics()
        {
            try
            {
                if (Environment.GetEnvironmentVariable("MIR2_GRAPHICS_TRACE") != "1") return;
                _frameBounds = new double[]
                {
                    1, 2, 4, 8, 12, 16, 20, 25, 33.334, 50, 66.667, 100, 200, 500, 1000,
                    double.PositiveInfinity
                };
                _window = new Window(Stopwatch.GetTimestamp());
                _queue = new BlockingCollection<Window>(QueueCapacity);
                string directory = Path.Combine(Environment.CurrentDirectory, "Logs");
                Directory.CreateDirectory(directory);
                string name = "Graphics-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff", CultureInfo.InvariantCulture)
                    + "-" + Environment.ProcessId + "-" + Guid.NewGuid().ToString("N") + ".csv";
                // Open synchronously once so an unwritable target disables tracing immediately.
                var stream = new StreamWriter(new FileStream(Path.Combine(directory, name), FileMode.CreateNew,
                    FileAccess.Write, FileShare.Read), new UTF8Encoding(false));
                _writer = new Thread(() => WriteLoop(stream)) { IsBackground = true, Name = "Graphics diagnostics" };
                try
                {
                    _enabled = true;
                    _writer.Start();
                }
                catch
                {
                    stream.Dispose();
                    throw;
                }
            }
            catch
            {
                _enabled = false;
                _failureReason = "initialization_failed";
            }
        }

        public static long Timestamp() => _enabled ? Stopwatch.GetTimestamp() : 0;

        public static void Record(GraphicsMetric metric, long start)
        {
            if (!_enabled || start <= 0 || (uint)metric >= MetricCount) return;
            long elapsed = Stopwatch.GetTimestamp() - start;
            if (elapsed < 0) return;
            lock (Gate)
            {
                if (!_enabled) return;
                int index = (int)metric;
                _window.Count[index]++;
                _window.Sum[index] += elapsed;
                if (elapsed > _window.Max[index]) _window.Max[index] = elapsed;
            }
        }

        /// <summary>Call once per successfully rendered frame, then PublishIfDue.
        /// The first frame has no interval sample; intervals span window boundaries.</summary>
        public static void RecordFrame(long timestamp)
        {
            if (!_enabled || timestamp <= 0) return;
            lock (Gate)
            {
                if (!_enabled) return;
                _window.Frames++;
                if (_previousFrame > 0 && timestamp >= _previousFrame)
                {
                    long ticks = timestamp - _previousFrame;
                    double ms = ticks * TickMilliseconds;
                    int bucket = 0;
                    while (ms > _frameBounds[bucket]) bucket++;
                    _window.Histogram[bucket]++;
                    _window.Intervals++;
                    _window.FrameSum += ticks;
                    if (ticks > _window.FrameMax) _window.FrameMax = ticks;
                    if (ms > 33.334) _window.Over33++;
                    if (ms > 50) _window.Over50++;
                    if (ms > 100) _window.Over100++;
                }
                _previousFrame = timestamp;
            }
        }

        public static void RecordDraw() { if (_enabled) AddCounter(0); }
        public static void RecordFlush() { if (_enabled) AddCounter(1); }
        public static void RecordStateChange() { if (_enabled) AddCounter(2); }

        private static void AddCounter(int counter)
        {
            lock (Gate)
            {
                if (!_enabled) return;
                if (counter == 0) _window.Draws++;
                else if (counter == 1) _window.Flushes++;
                else _window.StateChanges++;
            }
        }

        /// <summary>Latest gauge values, not sums. Bytes are an estimate, not driver VRAM usage.</summary>
        public static void UpdateResources(int textures, int controls, long estimatedTextureBytes)
        {
            if (!_enabled) return;
            lock (Gate)
            {
                if (!_enabled) return;
                _window.Textures = textures;
                _window.Controls = controls;
                _window.TextureBytes = estimatedTextureBytes;
            }
        }

        public static void PublishIfDue()
        {
            if (!_enabled) return;
            long now = Stopwatch.GetTimestamp();
            lock (Gate)
            {
                if (!_enabled || now - _window.Start < WindowTicks) return;
                Publish(now);
            }
        }

        // Caller owns Gate. At most one allocation batch per five seconds, never per frame.
        private static void Publish(long now)
        {
            Window completed = _window;
            completed.End = now;
            _window = new Window(now)
            {
                Textures = completed.Textures, Controls = completed.Controls, TextureBytes = completed.TextureBytes
            };
            if (!_queue.TryAdd(completed)) Interlocked.Increment(ref _droppedWindows);
        }

        /// <summary>Call once during application shutdown, after producers stop.
        /// Bounded wait (1.5 seconds); a blocked filesystem cannot block game shutdown indefinitely.</summary>
        public static void Stop()
        {
            Thread writer;
            lock (Gate)
            {
                if (_stopped) return;
                _stopped = true;
                if (_enabled)
                {
                    Publish(Stopwatch.GetTimestamp());
                    _enabled = false;
                }
                if (_queue != null) _queue.CompleteAdding();
                writer = _writer;
            }
            if (writer != null && writer.IsAlive && !writer.Join(1500))
                Volatile.Write(ref _failureReason, "shutdown_timeout");
        }

        private sealed class Window
        {
            public readonly long Start;
            public long End, Frames, Intervals, FrameSum, FrameMax, Draws, Flushes, StateChanges;
            public long Over33, Over50, Over100;
            public int Textures, Controls;
            public long TextureBytes;
            public readonly long[] Count = new long[MetricCount];
            public readonly long[] Sum = new long[MetricCount];
            public readonly long[] Max = new long[MetricCount];
            public readonly long[] Histogram = new long[16];
            public Window(long start) { Start = start; }
        }

        private static void WriteLoop(StreamWriter stream)
        {
            try
            {
                using (stream)
                {
                    var header = new StringBuilder("record_type,window_seconds,frames,frame_intervals,frame_sum_ms,frame_max_ms,frame_p50_upper_ms_approx,frame_p95_upper_ms_approx,frame_p99_upper_ms_approx,draws,flushes,state_changes,textures,controls,estimated_texture_bytes,dropped_windows_total,timing_semantics,frame_over_33_334ms,frame_over_50ms,frame_over_100ms");
                    foreach (GraphicsMetric metric in Enum.GetValues<GraphicsMetric>())
                        header.Append(',').Append(metric).Append("_count,").Append(metric).Append("_sum_ms,").Append(metric).Append("_max_ms");
                    stream.WriteLine(header);
                    stream.Flush();
                    foreach (Window window in _queue.GetConsumingEnumerable())
                    {
                        WriteWindow(stream, window, "window");
                        stream.Flush();
                    }
                    // Terminal metadata makes queue loss visible even if the last window was dropped.
                    WriteWindow(stream, new Window(0), "stop");
                }
            }
            catch
            {
                _enabled = false;
                Volatile.Write(ref _failureReason, "writer_failed");
            }
        }

        private static void WriteWindow(StreamWriter stream, Window window, string type)
        {
            var row = new StringBuilder(type);
            Append(row, (window.End - window.Start) / (double)Stopwatch.Frequency);
            Append(row, window.Frames); Append(row, window.Intervals);
            Append(row, window.FrameSum * TickMilliseconds); Append(row, window.FrameMax * TickMilliseconds);
            Append(row, Percentile(window, 50)); Append(row, Percentile(window, 95)); Append(row, Percentile(window, 99));
            Append(row, window.Draws); Append(row, window.Flushes); Append(row, window.StateChanges);
            Append(row, window.Textures); Append(row, window.Controls); Append(row, window.TextureBytes);
            Append(row, Interlocked.Read(ref _droppedWindows));
            row.Append(",inclusive_nested");
            Append(row, window.Over33); Append(row, window.Over50); Append(row, window.Over100);
            for (int i = 0; i < MetricCount; i++)
            {
                Append(row, window.Count[i]); Append(row, window.Sum[i] * TickMilliseconds);
                Append(row, window.Max[i] * TickMilliseconds);
            }
            stream.WriteLine(row);
        }

        private static double Percentile(Window window, int percent)
        {
            if (window.Intervals == 0) return 0;
            long target = (long)Math.Ceiling(window.Intervals * (percent / 100.0));
            long count = 0;
            for (int i = 0; i < window.Histogram.Length; i++)
            {
                count += window.Histogram[i];
                if (count >= target) return _frameBounds[i];
            }
            return double.PositiveInfinity;
        }

        private static void Append(StringBuilder row, long value) => row.Append(',').Append(value.ToString(CultureInfo.InvariantCulture));
        private static void Append(StringBuilder row, double value) => row.Append(',').Append(value.ToString("0.######", CultureInfo.InvariantCulture));
    }
}
