using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using Gum.Forms;
using Gum.Forms.Controls;

namespace GumTest;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GumService GumUI => GumService.Default;

    private SoundEffectInstance _musicInstance;
    private SoundEffect _loadedMusic;
    private Label _volumeLabel;
    private Label _statusLabel;
    private Slider _volumeSlider;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GumUI.Initialize(this);

        var panel = new StackPanel();
        panel.AddToRoot();
        panel.Width = 450;

        var title = new Label();
        title.Text = "Gum Test - Audio Diagnostics (MGCB)";
        panel.AddChild(title);

        _statusLabel = new Label();
        _statusLabel.Text = "Ready. Press Play to load via MGCB.";
        panel.AddChild(_statusLabel);

        var playButton = new Button();
        playButton.Text = "Play Main Theme";
        playButton.Click += OnPlayClicked;
        panel.AddChild(playButton);

        var stopButton = new Button();
        stopButton.Text = "Stop Music";
        stopButton.Click += OnStopClicked;
        panel.AddChild(stopButton);

        _volumeLabel = new Label();
        _volumeLabel.Text = "Volume: 100";
        panel.AddChild(_volumeLabel);

        _volumeSlider = new Slider();
        _volumeSlider.Minimum = 0;
        _volumeSlider.Maximum = 100;
        _volumeSlider.Value = 100;
        _volumeSlider.ValueChanged += OnVolumeChanged;
        panel.AddChild(_volumeSlider);

        var quitButton = new Button();
        quitButton.Text = "Quit";
        quitButton.Click += (s, e) => Exit();
        panel.AddChild(quitButton);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    private void OnPlayClicked(object sender, EventArgs e)
    {
        try
        {
            StopMusic();

            _statusLabel.Text = "Loading via ContentManager...";
            _loadedMusic = Content.Load<SoundEffect>("Audio/Music/main_theme");

            if (_loadedMusic == null)
            {
                _statusLabel.Text = "FAILED: Content.Load returned null";
                return;
            }

            if (_loadedMusic.Duration.TotalMilliseconds <= 0)
            {
                _statusLabel.Text = "FAILED: zero-duration SoundEffect (corrupt .xnb?)";
                return;
            }

            _musicInstance = _loadedMusic.CreateInstance();
            _musicInstance.IsLooped = true;
            _musicInstance.Volume = (float)(_volumeSlider.Value / 100.0);
            _musicInstance.Play();

            var state = _musicInstance.State;
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Xenocide", "audio_debug.log");

            _statusLabel.Text = $"PLAYING via MGCB .xnb\n"
                + $"Duration={_loadedMusic.Duration.TotalSeconds:F1}s\n"
                + $"State={state}\n"
                + $"MasterVolume={SoundEffect.MasterVolume:F2}\n"
                + $"InstanceVolume={_musicInstance.Volume:F2}\n"
                + $"Log: {logPath}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"EXCEPTION: {ex.GetType().Name}\n{ex.Message}";
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        StopMusic();
        _statusLabel.Text = "Stopped.";
    }

    private void OnVolumeChanged(object sender, EventArgs e)
    {
        var vol = (float)(_volumeSlider.Value / 100.0);
        _volumeLabel.Text = $"Volume: {_volumeSlider.Value:F0}";
        if (_musicInstance != null)
            _musicInstance.Volume = vol;
    }

    private void StopMusic()
    {
        if (_musicInstance != null)
        {
            _musicInstance.Stop();
            _musicInstance.Dispose();
            _musicInstance = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        GumUI.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GumUI.Draw();

        base.Draw(gameTime);
    }
}
