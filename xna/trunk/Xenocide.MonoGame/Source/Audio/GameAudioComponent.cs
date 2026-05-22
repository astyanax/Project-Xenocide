using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Assets;

namespace AudioSystem
{
    public class GameAudioComponent : GameComponent, IAudioSystem
    {
        private readonly Dictionary<string, SoundEffect> _sounds = new();
        private readonly List<SongDef> _songDefs = new();
        private readonly Random _random = new();
        private SoundEffectInstance _currentMusic;
        private string _currentCategory;
        private List<SongDef> _currentPlaylist;
        private bool _autoAdvance;

        private float _musicVolume = 1f;
        private float _soundVolume = 1f;

        private const string SfxPrefix = "Audio/Sounds/";
        private const string MusicPrefix = "Audio/Music/";

        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Xenocide", "audio_debug.log");

        public bool UseAudio { get; set; } = true;
        public bool IsInitialized { get; private set; }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Math.Clamp(value, 0f, 1f);
                if (_currentMusic != null)
                    _currentMusic.Volume = _musicVolume;
            }
        }

        public float SoundVolume
        {
            get => _soundVolume;
            set => _soundVolume = Math.Clamp(value, 0f, 1f);
        }

        public GameAudioComponent(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            Log("=== GameAudioComponent Initialize ===");
            Log($"App base: {AppDomain.CurrentDomain.BaseDirectory}");
            Log($"Content root: {Game.Content.RootDirectory}");

            var testPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Game.Content.RootDirectory,
                "Audio", "Music", "main_theme.xnb");
            Log($"Expected path: {testPath}");
            Log($"File exists: {File.Exists(testPath)}");

            foreach (var kvp in AssetRegistry.MusicDefs)
                _songDefs.Add(new SongDef(kvp.Value.AssetName, kvp.Value.Category));

            Log($"Registered {_songDefs.Count} music track definitions");
            IsInitialized = true;
            base.Initialize();
        }

        public void Initialize(Game game)
        {
        }

        public void LoadSound(SoundId id)
        {
            LoadSound(AssetRegistry.SoundPath(id));
        }

        public void LoadSound(string soundName)
        {
            if (!UseAudio) return;

            var assetName = LegacyToSfxPath(soundName);
            Log($"LoadSound: '{soundName}' -> asset '{assetName}'");
            try
            {
                _sounds[soundName] = Game.Content.Load<SoundEffect>(assetName);
                var sfx = _sounds[soundName];
                Log($"  OK (duration={sfx.Duration.TotalSeconds:F2}s)");
            }
            catch (Exception ex)
            {
                Log($"  FAILED: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public void PlaySound(SoundId id)
        {
            PlaySound(AssetRegistry.SoundPath(id));
        }

        public void PlaySound(string soundName)
        {
            if (!UseAudio || !_sounds.TryGetValue(soundName, out var sfx)) return;

            var instance = sfx.CreateInstance();
            instance.Volume = _soundVolume;
            instance.Play();
        }

        public void Play(SoundId id)
        {
            PlaySound(id);
        }

        public void Play(string soundName)
        {
            PlaySound(soundName);
        }

        public void PlayMusic(string musicName)
        {
            if (!UseAudio) return;

            StopMusicInternal();
            _autoAdvance = false;

            var assetName = LegacyToMusicPath(musicName);
            Log($"PlayMusic: '{musicName}' -> asset '{assetName}'");
            try
            {
                var sfx = Game.Content.Load<SoundEffect>(assetName);
                _currentMusic = sfx.CreateInstance();
                _currentMusic.IsLooped = true;
                _currentMusic.Volume = _musicVolume;
                _currentMusic.Play();
                Log($"  Playing (duration={sfx.Duration.TotalSeconds:F2}s, state={_currentMusic.State})");
            }
            catch (Exception ex)
            {
                Log($"  FAILED: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public void PlayRandomMusic()
        {
            Log($"PlayRandomMusic() called. UseAudio={UseAudio}, songs={_songDefs.Count}");
            if (!UseAudio || _songDefs.Count == 0) return;

            StopMusicInternal();
            _currentCategory = null;
            _currentPlaylist = _songDefs;
            _autoAdvance = true;
            PlayRandomFromCurrentPlaylist();
        }

        public void PlayRandomMusic(string category)
        {
            Log($"PlayRandomMusic(\"{category}\") called");
            if (!UseAudio) return;

            StopMusicInternal();

            _currentCategory = category;

            var filtered = _songDefs
                .Where(s => string.Equals(s.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filtered.Count == 0)
            {
                Log($"  No songs in category '{category}', falling back to all");
                PlayRandomMusic();
                return;
            }

            _currentPlaylist = filtered;
            _autoAdvance = true;
            PlayRandomFromCurrentPlaylist();
        }

        public void PlayRandomMusic(int someArg)
        {
            PlayRandomMusic();
        }

        public void StopMusic()
        {
            StopMusicInternal();
            _autoAdvance = false;
            _currentCategory = null;
            _currentPlaylist = null;
        }

        public void SetMasterVolume(float volume)
        {
            SoundEffect.MasterVolume = Math.Clamp(volume, 0f, 1f);
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
        }

        public void SetSoundVolume(float volume)
        {
            SoundVolume = volume;
        }

        public override void Update(GameTime gameTime)
        {
            if (_autoAdvance && _currentMusic != null && _currentMusic.State == SoundState.Stopped)
            {
                Log("Auto-advancing to next track");
                _currentMusic.Dispose();
                _currentMusic = null;
                PlayRandomFromCurrentPlaylist();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopMusicInternal();
            }
            base.Dispose(disposing);
        }

        private void PlayRandomFromCurrentPlaylist()
        {
            if (_currentPlaylist == null || _currentPlaylist.Count == 0) return;

            var def = _currentPlaylist[_random.Next(_currentPlaylist.Count)];
            Log($"PlayRandom: trying asset '{def.AssetName}'");
            try
            {
                var sfx = Game.Content.Load<SoundEffect>(def.AssetName);
                _currentMusic = sfx.CreateInstance();
                _currentMusic.Volume = _musicVolume;
                _currentMusic.Play();
                Log($"  Playing (duration={sfx.Duration.TotalSeconds:F2}s, state={_currentMusic.State})");
            }
            catch (Exception ex)
            {
                Log($"  FAILED: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void StopMusicInternal()
        {
            if (_currentMusic != null)
            {
                _currentMusic.Stop();
                _currentMusic.Dispose();
                _currentMusic = null;
            }
        }

        private static string LegacyToSfxPath(string legacyPath)
        {
            var name = legacyPath.Replace('\\', '/');
            if (name.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 4);
            return SfxPrefix + name;
        }

        private static string LegacyToMusicPath(string legacyPath)
        {
            var name = legacyPath.Replace('\\', '/');
            if (name.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 4);
            return MusicPrefix + name;
        }

        private static void Log(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} [Audio] {message}";
            Debug.WriteLine(line);
            try
            {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            catch
            {
            }
        }

        private class SongDef
        {
            public readonly string AssetName;
            public readonly string Category;

            public SongDef(string assetName, string category)
            {
                AssetName = assetName;
                Category = category;
            }
        }
    }
}
