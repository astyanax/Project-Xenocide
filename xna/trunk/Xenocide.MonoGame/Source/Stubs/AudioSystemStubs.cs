using Microsoft.Xna.Framework;

namespace AudioSystem
{
    public interface IAudioSystem
    {
        bool UseAudio { get; set; }
        bool IsInitialized { get; }
        void Initialize(Game game);
        void LoadSound(string soundName);
        void Play(string soundName);
        void PlayMusic(string musicName);
        void PlayRandomMusic();
        void PlayRandomMusic(int someArg);
        void PlayRandomMusic(string category);
        void PlaySound(string soundName);
        void StopMusic();
        float MusicVolume { get; set; }
        float SoundVolume { get; set; }
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);
    }

    public class FmodGameComponent : GameComponent
    {
        public FmodGameComponent(Game game) : base(game) { }
    }
}
