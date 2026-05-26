using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using NLog;

using ProjectXenocide.Assets;

namespace AudioSystem
{
    public class GameAudioComponent : GameComponent, IAudioSystem
    {
        private static readonly Logger Log = LogManager.GetLogger("Audio");

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
            Log.Info("=== GameAudioComponent Initialize ===");
            Log.Info("App base: {0}", AppDomain.CurrentDomain.BaseDirectory);
            Log.Info("Content root: {0}", Game.Content.RootDirectory);

            var testPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Game.Content.RootDirectory,
                "Audio", "Music", "main_theme.xnb");
            Log.Info("Expected path: {0}", testPath);
            Log.Info("File exists: {0}", File.Exists(testPath));

            foreach (var kvp in AssetRegistry.MusicDefs)
                _songDefs.Add(new SongDef(kvp.Value.AssetName, kvp.Value.Category));

            Log.Info("Registered {0} music track definitions", _songDefs.Count);
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
            Log.Info("LoadSound: '{0}' -> asset '{1}'", soundName, assetName);
            try
            {
                _sounds[soundName] = Game.Content.Load<SoundEffect>(assetName);
                var sfx = _sounds[soundName];
                Log.Info("  OK (duration={0:F2}s)", sfx.Duration.TotalSeconds);
            }
            catch (Exception ex)
            {
                Log.Warn("  FAILED: {0}: {1}", ex.GetType().Name, ex.Message);
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
            Log.Info("PlayMusic: '{0}' -> asset '{1}'", musicName, assetName);
            try
            {
                var sfx = Game.Content.Load<SoundEffect>(assetName);
                _currentMusic = sfx.CreateInstance();
                _currentMusic.IsLooped = true;
                _currentMusic.Volume = _musicVolume;
                _currentMusic.Play();
                Log.Info("  Playing (duration={0:F2}s, state={1})", sfx.Duration.TotalSeconds, _currentMusic.State);
            }
            catch (Exception ex)
            {
                Log.Warn("  FAILED: {0}: {1}", ex.GetType().Name, ex.Message);
            }
        }

        public void PlayRandomMusic()
        {
            Log.Info("PlayRandomMusic() called. UseAudio={0}, songs={1}", UseAudio, _songDefs.Count);
            if (!UseAudio || _songDefs.Count == 0) return;

            StopMusicInternal();
            _currentCategory = null;
            _currentPlaylist = _songDefs;
            _autoAdvance = true;
            PlayRandomFromCurrentPlaylist();
        }

        public void PlayRandomMusic(string category)
        {
            Log.Info("PlayRandomMusic(\"{0}\") called", category);
            if (!UseAudio) return;

            StopMusicInternal();

            _currentCategory = category;

            var filtered = _songDefs
                .Where(s => string.Equals(s.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filtered.Count == 0)
            {
                Log.Info("  No songs in category '{0}', falling back to all", category);
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
                Log.Info("Auto-advancing to next track");
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
            Log.Info("PlayRandom: trying asset '{0}'", def.AssetName);
            try
            {
                var sfx = Game.Content.Load<SoundEffect>(def.AssetName);
                _currentMusic = sfx.CreateInstance();
                _currentMusic.Volume = _musicVolume;
                _currentMusic.Play();
                Log.Info("  Playing (duration={0:F2}s, state={1})", sfx.Duration.TotalSeconds, _currentMusic.State);
            }
            catch (Exception ex)
            {
                Log.Warn("  FAILED: {0}: {1}", ex.GetType().Name, ex.Message);
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
