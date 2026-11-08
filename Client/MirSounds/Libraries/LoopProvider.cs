using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Client.MirSounds.Libraries
{
    internal class LoopProvider : ISoundLibrary, IDisposable
    {
        public int Index { get; set; }
        public long ExpireTime { get; set; }

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private int _unscaledVolume;
        private string _fileName;
        private bool _loop;
        private bool _isDisposing;

        public static LoopProvider TryCreate(int index, string fileName, int volume, bool loop)
        {
            lock (SoundManager.AudioLock)
            {
                if (!SoundManager.CanPlay) return null;
                fileName = Path.Combine(Settings.SoundPath, fileName);
                string fileType = Path.GetExtension(fileName);

                // attempt to find file
                if (String.IsNullOrEmpty(fileType))
                {
                    foreach (String ext in SoundManager.SupportedFileTypes)
                    {
                        if (File.Exists($"{fileName}{ext}"))
                        {
                            fileName = $"{fileName}{ext}";
                            fileType = ext;

                            break;
                        }
                    }
                }

                if (SoundManager.SupportedFileTypes.Contains(fileType) &&
                    File.Exists(fileName))
                {
                    var sound = new LoopProvider(index, fileName, volume, loop);
                    if (SoundManager.CanPlay) return sound;
                    sound.Dispose();
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        public LoopProvider(int index, string fileName, int volume, bool loop)
        {
            Index = index;
            _loop = loop;
            _fileName = fileName;

            try { Play(volume); }
            catch
            {
                Dispose();
                throw;
            }
            if (!SoundManager.CanPlay) Dispose();
        }

        public bool IsPlaying()
        {
            lock (SoundManager.AudioLock)
                return SoundManager.CanPlay && !_isDisposing && outputDevice?.PlaybackState == PlaybackState.Playing;
        }

        public void Play(int volume)
        {
            SoundManager.WithAudio(() =>
            {
                if (_isDisposing) return;
                try
                {
                    if (outputDevice?.PlaybackState == PlaybackState.Playing)
                    {
                        return;
                    }

                    ExpireTime = CMain.Time + Settings.SoundCleanMinutes * 60 * 1000;

                    if (outputDevice == null)
                    {
                        outputDevice = new WaveOutEvent();
                        outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
                    }

                    if (audioFile == null)
                    {
                        audioFile = new AudioFileReader(_fileName);
                        outputDevice.Init(audioFile);
                    }

                    // loop or sound already cached
                    if (outputDevice?.PlaybackState == PlaybackState.Stopped)
                    {
                        audioFile.Seek(0, SeekOrigin.Begin);
                    }

                    outputDevice.Volume = ScaleVolume(volume);
                    outputDevice.Play();
                }
                catch
                {
                    // This instance may not yet belong to SoundManager during construction.
                    Dispose();
                    throw;
                }
            });
            if (!SoundManager.CanPlay) Dispose();
        }

        public void SetVolume(int vol)
        {
            SoundManager.WithAudio(() =>
            {
                if (!_isDisposing && outputDevice != null) outputDevice.Volume = ScaleVolume(vol);
            });
        }

        public void Stop()
        {
            Dispose();
        }

        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            lock (SoundManager.AudioLock)
            {
                if (_isDisposing || !SoundManager.CanPlay || !ReferenceEquals(sender, outputDevice)) return;
                if (e.Exception != null)
                {
                    if (SoundManager.IsDeviceException(e.Exception)) SoundManager.DisableAudio(e.Exception);
                    else System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.Exception).Throw();
                    return;
                }
                if (_loop) Play(_unscaledVolume);
            }
        }

        public void Dispose()
        {
            lock (SoundManager.AudioLock)
            {
                if (_isDisposing) return;
                _isDisposing = true;
                var device = outputDevice;
                var file = audioFile;
                outputDevice = null;
                audioFile = null;
                if (device != null) device.PlaybackStopped -= OutputDevice_PlaybackStopped;
                try
                {
                    SoundManager.DisposeOutput(device);
                }
                finally { file?.Dispose(); }
            }
        }

        private float ScaleVolume(int volume)
        {
            _unscaledVolume = volume;

            float scaled = 0.0f + (float)(volume - 0) / (100 - 0) * (1.0f - 0.0f);
            return scaled;
        }
    }
}
