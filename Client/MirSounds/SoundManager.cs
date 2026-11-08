using Client.MirSounds.Libraries;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Client.MirSounds
{
    public static class SoundManager
    {
        private static Dictionary<int, string> _indexList => SoundList.Indexes;
        private static List<KeyValuePair<long, int>> _delayList = new List<KeyValuePair<long, int>>();
        private static Dictionary<int, CachedSound> _cachedOneShots = new Dictionary<int, CachedSound>();

        private static Dictionary<int, LoopProvider> _loopingSounds = new Dictionary<int, LoopProvider>();
        private static LoopProvider _music;
        private static WaveOutEvent _OneShots;
        private static MixingSampleProvider mixer;

        private static int _vol;
        private static int _musicVol;

        internal static readonly object AudioLock = new object();
        private static volatile bool _audioUnavailable;
        private static volatile bool _disposed;
        public static event Action<Exception> AudioUnavailable;
        internal static bool CanPlay => !_audioUnavailable && !_disposed;

        internal static bool IsDeviceException(Exception exception)
        {
            // Invalid parameters and unsupported formats remain application errors.
            // MMSYSERR_ERROR, BADDEVICEID, NOTENABLED, ALLOCATED, NODRIVER.
            if (exception is NAudio.MmException mm)
                return (int)mm.Result is 1 or 2 or 3 or 4 or 6;
            return exception is System.Runtime.InteropServices.COMException com &&
                (com.HResult == unchecked((int)0x88890004) ||
                 com.HResult == unchecked((int)0x8889000F) ||
                 com.HResult == unchecked((int)0x88890010) ||
                 com.HResult == unchecked((int)0x88890026));
        }

        internal static void WithAudio(Action action)
        {
            lock (AudioLock)
            {
                if (!CanPlay) return;
                try { action(); }
                catch (Exception ex) when (IsDeviceException(ex)) { DisableAudio(ex); }
            }
        }

        internal static void DisableAudio(Exception exception)
        {
            lock (AudioLock)
            {
                if (!CanPlay) return;
                _audioUnavailable = true;
                try { ReleaseResources(); }
                finally { AudioUnavailable?.Invoke(exception); }
            }
        }

        private static void ReleaseResources()
        {
            // Detach first: Dispose can synchronously raise PlaybackStopped.
            var oneShots = _OneShots;
            if (oneShots != null) oneShots.PlaybackStopped -= OneShots_PlaybackStopped;
            var music = _music;
            var loops = _loopingSounds.Values.ToArray();
            _OneShots = null;
            _music = null;
            mixer = null;
            _loopingSounds.Clear();
            _cachedOneShots.Clear();
            _delayList.Clear();
            Exception failure = null;
            void Release(IDisposable resource)
            {
                try
                {
                    if (resource is WaveOutEvent output) DisposeOutput(output);
                    else resource?.Dispose();
                }
                catch (Exception ex) when (IsDeviceException(ex)) { }
                catch (Exception ex) { failure ??= ex; }
            }
            Release(oneShots);
            Release(music);
            foreach (var sound in loops) Release(sound);
            if (failure != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }

        internal static void DisposeOutput(WaveOutEvent output)
        {
            try { output?.Dispose(); }
            catch (Exception ex) when (IsDeviceException(ex))
            {
                // NAudio 2.2.1 Stop sets Stopped before waveOutReset. If reset fails,
                // Dispose exits before releasing buffers/handles; retry the cleanup once.
                try { output?.Dispose(); }
                catch (Exception retry) when (IsDeviceException(retry)) { }
            }
        }

        public static readonly List<string> SupportedFileTypes;
        private static long _checkSoundTime;
        public static ISoundLibrary Music => _music;

        public static int Vol
        {
            get { return _vol; }
            set
            {
                if (_vol == value) return;
                _vol = value;

                AdjustAllVolumes();
            }
        }

        public static int MusicVol
        {
            get { return _musicVol; }
            set
            {
                if (_musicVol == value) return;
                _musicVol = value;

                WithAudio(() => _music?.SetVolume(MusicVol));
            }
        }

        static SoundManager()
        {
            _checkSoundTime = CMain.Time + 30 * 1000;

            SupportedFileTypes = new List<string>
            {
                ".wav",
                ".mp3"
            };

            SoundList.LoadSoundList();
        }

        public static void Create()
        {
            WithAudio(() =>
            {
                if (_OneShots != null) return;
                int sampleRate = 44100;
                int channelCount = 2;

                _OneShots = new WaveOutEvent();
                _OneShots.PlaybackStopped += OneShots_PlaybackStopped;
                mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
                mixer.ReadFully = true;
                _OneShots.Init(mixer);
                _OneShots.Volume = ScaleVolume(_vol);
                _OneShots.Play();
            });
        }

        private static void OneShots_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            lock (AudioLock)
            {
                if (!CanPlay || !ReferenceEquals(sender, _OneShots) || e.Exception == null) return;
                if (IsDeviceException(e.Exception)) DisableAudio(e.Exception);
                else System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.Exception).Throw();
            }
        }

        public static void PlaySound(int index, bool loop = false, int delay = 0)
        {
            WithAudio(() =>
            {
                CheckSoundTimeOut();

                if (delay > 0)
                {
                    _delayList.Add(new KeyValuePair<long, int>(CMain.Time + delay, index));
                    return;
                }

                if (!_indexList.ContainsKey(index))
                {
                    string filename = index > 20000 ?
                                        string.Format("M{0:0}-{1:0}", (index - 20000) / 10, index % 10) :
                                        string.Format("{0:000}-{1:0}", index / 10, index % 10);

                    _indexList.Add(index, filename);
                }

                if (!loop)
                {
                    if (!_cachedOneShots.TryGetValue(index, out CachedSound cachedSound))
                    {
                        cachedSound = new CachedSound(index, _indexList[index]);
                        _cachedOneShots.Add(index, cachedSound);
                    }

                    if (cachedSound.AudioData?.Length > 0)
                    {
                        AddMixerInput(new OneShotProvider(cachedSound));
                        cachedSound.ExpireTime = CMain.Time + Settings.SoundCleanMinutes * 60 * 1000;
                    }
                }
                else
                {
                    var sound = LoopProvider.TryCreate(index, _indexList[index], MusicVol, loop);
                    if (sound != null)
                    {
                        if (_loopingSounds.Remove(index, out var previous)) previous.Dispose();
                        _loopingSounds.Add(index, sound);
                        sound.Play(Vol);
                    }
                }
            });
        }

        public static void StopSound(int index)
        {
            WithAudio(() =>
            {
                if (_loopingSounds.Remove(index, out var sound)) sound.Dispose();
            });
        }

        public static void PlayMusic(int index, bool loop = false)
        {
            WithAudio(() =>
            {
                StopMusic();

                if (_indexList.TryGetValue(index, out string value))
                {
                    _music = LoopProvider.TryCreate(index, value, MusicVol, loop);
                }
            });
        }

        public static void StopMusic()
        {
            WithAudio(() =>
            {
                var music = _music;
                _music = null;
                music?.Dispose();
            });
        }

        public static void ProcessDelayedSounds()
        {
            WithAudio(() =>
            {
                if (_delayList.Count == 0) return;

                var sounds = _delayList.Where(x => x.Key <= CMain.Time).ToList();

                foreach (var sound in sounds)
                {
                    _delayList.Remove(sound);

                    PlaySound(sound.Value);
                }
            });
        }

        private static void AdjustAllVolumes()
        {
            WithAudio(() =>
            {
                if (_OneShots != null)
                {
                    _OneShots.Volume = ScaleVolume(Vol);
                }

                foreach (var sound in _loopingSounds.Values.ToArray())
                {
                    sound.SetVolume(Vol);
                }
            });
        }

        private static float ScaleVolume(int volume)
        {
            float scaled = 0.0f + (float)(volume - 0) / (100 - 0) * (1.0f - 0.0f);
            return scaled;
        }

        private static void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        private static ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        private static void CheckSoundTimeOut()
        {
            if (CMain.Time >= _checkSoundTime)
            {
                _checkSoundTime = CMain.Time + 30 * 1000;

                List<int> keysToRemove = new List<int>();
                foreach (var key in _cachedOneShots.Keys)
                {
                    if (CMain.Time > _cachedOneShots[key].ExpireTime)
                    {
                        keysToRemove.Add(key);
                    }
                }

                keysToRemove.ForEach(key => { _cachedOneShots.Remove(key); });

                keysToRemove.Clear();
                foreach (var key in _loopingSounds.Keys)
                {
                    if (CMain.Time > _loopingSounds[key].ExpireTime)
                    {
                        keysToRemove.Add(key);
                    }
                }

                keysToRemove.ForEach(key =>
                {
                    var sound = _loopingSounds[key];
                    _loopingSounds.Remove(key);
                    sound.Dispose();
                });
            }
        }

        public static void Dispose()
        {
            lock (AudioLock)
            {
                _disposed = true;
                ReleaseResources();
            }
        }
    }
}
