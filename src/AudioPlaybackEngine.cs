using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;

namespace AudioKeepAlive
{
    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int deviceId, float volume, int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent() { DeviceNumber = deviceId };
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Volume = volume;
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
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

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        private static AudioPlaybackEngine? _instance;
        private static float _currentVolume;

        public static AudioPlaybackEngine Instance(int deviceId, float volume)
        {
            if(_currentVolume != volume && _instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }

            _currentVolume = volume;

            if (_instance == null)
                _instance = new AudioPlaybackEngine(deviceId, volume, 44100, 2);

            return _instance;
        }
    }
}
