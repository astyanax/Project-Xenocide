using Microsoft.Xna.Framework;
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
    private Label clickCountLabel;

    private int clickCount = 0;

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
        panel.Width = 400;

        var title = new Label();
        title.Text = "Gum Test - Project Xenocide";
        panel.AddChild(title);

        var description = new Label();
        description.Text = "If you can see this, Gum is working!";
        panel.AddChild(description);

        var clickButton = new Button();
        clickButton.Text = "Click me";
        clickButton.Click += (s, e) =>
        {
            clickCount++;
            clickCountLabel.Text = $"Clicked {clickCount} times";
        };
        panel.AddChild(clickButton);

        clickCountLabel = new Label();
        clickCountLabel.Text = "Clicked 0 times";
        panel.AddChild(clickCountLabel);

        var slider = new Slider();
        slider.Minimum = 0;
        slider.Maximum = 100;
        slider.Value = 50;
        panel.AddChild(slider);

        var sliderValueLabel = new Label();
        slider.ValueChanged += (s, e) =>
        {
            sliderValueLabel.Text = $"Slider: {slider.Value:F0}";
        };
        sliderValueLabel.Text = $"Slider: {slider.Value:F0}";
        panel.AddChild(sliderValueLabel);

        var textBox = new TextBox();
        textBox.Text = "Type here...";
        panel.AddChild(textBox);

        var textBoxMirror = new Label();
        textBox.TextChanged += (s, e) =>
        {
            textBoxMirror.Text = $"You typed: {textBox.Text}";
        };
        panel.AddChild(textBoxMirror);

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
