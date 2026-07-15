using Microsoft.Xna.Framework;

using ProjectXenocide.Assets;

namespace AudioSystem
{
    public interface IAudioSystem
    {
        bool UseAudio { get; set; }
        bool IsInitialized { get; }
        void Initialize(Game game);
        void LoadSound(SoundId id);
        void LoadSound(string soundName);
        void Play(SoundId id);
        void Play(string soundName);
        void PlayMusic(string musicName);
        void PlayRandomMusic();
        void PlayRandomMusic(int someArg);
        void PlayRandomMusic(string category);
        void PlaySound(SoundId id);
        void PlaySound(string soundName);
        void StopMusic();
        float MusicVolume { get; set; }
        float SoundVolume { get; set; }
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);
    }
}
