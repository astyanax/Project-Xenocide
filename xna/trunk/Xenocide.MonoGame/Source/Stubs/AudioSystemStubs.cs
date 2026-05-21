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

    public class FmodGameComponent : GameComponent, IAudioSystem
    {
        public FmodGameComponent(Game game) : base(game) { }
        public bool UseAudio { get; set; } = true;
        public bool IsInitialized => true;
        public float MusicVolume { get; set; } = 1f;
        public float SoundVolume { get; set; } = 1f;
        public void Initialize(Game game) { }
        public void LoadSound(string soundName) { }
        public void Play(string soundName) { }
        public void PlayMusic(string musicName) { }
        public void PlayRandomMusic() { }
        public void PlayRandomMusic(int someArg) { }
        public void PlayRandomMusic(string category) { }
        public void PlaySound(string soundName) { }
        public void StopMusic() { }
        public void SetMasterVolume(float volume) { }
        public void SetMusicVolume(float volume) { }
        public void SetSoundVolume(float volume) { }
    }
}
