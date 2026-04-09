using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenguinPlatformer;

public enum GameState { Playing, GameOver }

public struct SnowParticle
{
    public Vector2 Position;
    public float Speed;
    public int Size;
    public float Drift; // gentle horizontal drift
}

public class Game1 : Game
{
    // Starting position: on top of ground (ground top = 520, player height = 46)
    private static readonly Vector2 StartPosition = new Vector2(100, 474);
    private static readonly Vector2 LevelSize = new Vector2(4000, 600);

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont? _font;

    private Player _player = null!;
    private readonly List<Platform> _platforms = new();
    private List<Enemy> _enemies = new();
    private Camera _camera = null!;

    private int _score;
    private int _lives = 3;
    private GameState _gameState = GameState.Playing;

    private readonly List<SnowParticle> _snow = new();
    private readonly Random _rng = new();
    private KeyboardState _prevKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 600;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        try { _font = Content.Load<SpriteFont>("Font"); }
        catch { _font = null; }

        _camera = new Camera(GraphicsDevice.Viewport);
        _player = new Player(StartPosition);

        InitSnow();
        BuildLevel();
        _camera.Follow(StartPosition, LevelSize);
    }

    private void InitSnow()
    {
        _snow.Clear();
        for (int i = 0; i < 90; i++)
        {
            _snow.Add(new SnowParticle
            {
                Position = new Vector2(_rng.Next(0, 1024), _rng.Next(0, 600)),
                Speed = _rng.Next(30, 100),
                Size = _rng.Next(2, 6),
                Drift = (_rng.NextSingle() - 0.5f) * 20f
            });
        }
    }

    private void BuildLevel()
    {
        _platforms.Clear();
        _enemies.Clear();

        var iceBlue = new Color(80, 160, 210);
        var iceLighter = new Color(100, 185, 230);

        // Ground (full level width)
        _platforms.Add(new Platform(0, 520, 4000, 80, iceBlue));

        // Raised platforms — designed so the player can always reach the next one
        // Format: (x, y, width, height)
        // Each platform is within single-jump reach of adjacent platforms or ground
        _platforms.Add(new Platform(200,  420, 160, 20, iceLighter));   // P1
        _platforms.Add(new Platform(430,  350, 180, 20, iceLighter));   // P2
        _platforms.Add(new Platform(680,  270, 140, 20, iceLighter));   // P3  (needs double jump from P2)
        _platforms.Add(new Platform(870,  390, 130, 20, iceLighter));   // P4  (down from P3)
        _platforms.Add(new Platform(1060, 310, 170, 20, iceLighter));   // P5
        _platforms.Add(new Platform(1290, 240, 190, 20, iceLighter));   // P6  (double jump from P5)
        _platforms.Add(new Platform(1530, 350, 150, 20, iceLighter));   // P7
        _platforms.Add(new Platform(1730, 420, 130, 20, iceLighter));   // P8
        _platforms.Add(new Platform(1920, 300, 200, 20, iceLighter));   // P9
        _platforms.Add(new Platform(2170, 210, 160, 20, iceLighter));   // P10 (double jump)
        _platforms.Add(new Platform(2380, 380, 180, 20, iceLighter));   // P11
        _platforms.Add(new Platform(2610, 270, 150, 20, iceLighter));   // P12
        _platforms.Add(new Platform(2810, 350, 170, 20, iceLighter));   // P13
        _platforms.Add(new Platform(3030, 250, 200, 20, iceLighter));   // P14 (double jump)
        _platforms.Add(new Platform(3280, 390, 130, 20, iceLighter));   // P15
        _platforms.Add(new Platform(3460, 300, 180, 20, iceLighter));   // P16
        _platforms.Add(new Platform(3700, 220, 200, 20, iceLighter));   // P17 (double jump, near end)

        // --- Enemies (walrus) ---
        // Ground patrols (walrus Y = ground top 520 - enemy height 40 = 480)
        _enemies.Add(new Enemy(new Vector2(360,  480), 150,  600));
        _enemies.Add(new Enemy(new Vector2(770,  480), 600,  1000));
        _enemies.Add(new Enemy(new Vector2(1380, 480), 1100, 1700));
        _enemies.Add(new Enemy(new Vector2(1960, 480), 1700, 2300));
        _enemies.Add(new Enemy(new Vector2(2530, 480), 2300, 2750));
        _enemies.Add(new Enemy(new Vector2(2950, 480), 2750, 3100));
        _enemies.Add(new Enemy(new Vector2(3400, 480), 3200, 3850));

        // Platform patrols (walrus Y = platform top - 40)
        // P1 top=420: walrusY=380, patrol within platform bounds 200..360-52=308
        _enemies.Add(new Enemy(new Vector2(210, 380), 200, 340));
        // P2 top=350: walrusY=310
        _enemies.Add(new Enemy(new Vector2(450, 310), 430, 580));
        // P3 top=270: walrusY=230
        _enemies.Add(new Enemy(new Vector2(695, 230), 680, 788));
        // P5 top=310: walrusY=270
        _enemies.Add(new Enemy(new Vector2(1075, 270), 1060, 1178));
        // P6 top=240: walrusY=200
        _enemies.Add(new Enemy(new Vector2(1305, 200), 1290, 1427));
        // P9 top=300: walrusY=260
        _enemies.Add(new Enemy(new Vector2(1940, 260), 1920, 2068));
        // P12 top=270: walrusY=230
        _enemies.Add(new Enemy(new Vector2(2625, 230), 2610, 2708));
        // P14 top=250: walrusY=210
        _enemies.Add(new Enemy(new Vector2(3045, 210), 3030, 3178));
        // P17 top=220: walrusY=180
        _enemies.Add(new Enemy(new Vector2(3715, 180), 3700, 3848));
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateSnow(dt);

        if (_gameState == GameState.GameOver)
        {
            if (keyboard.IsKeyDown(Keys.R) && !_prevKeyboard.IsKeyDown(Keys.R))
                FullRestart();
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        _player.Update(gameTime, keyboard, _platforms);

        foreach (var enemy in _enemies)
            enemy.Update(gameTime);

        // Player-enemy collision
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (!enemy.IsAlive) continue;

            var pr = _player.Bounds;
            var er = enemy.Bounds;

            if (!pr.Intersects(er)) continue;

            // Stomp: player moving downward AND feet are near the top of the enemy
            bool stomp = _player.Velocity.Y > 0 && (pr.Bottom - er.Top) < 18;

            if (stomp)
            {
                enemy.Kill();
                _player.BounceOff();
                _score += 100;
            }
            else
            {
                PlayerDied();
                _prevKeyboard = keyboard;
                base.Update(gameTime);
                return;
            }
        }

        _enemies.RemoveAll(e => !e.IsAlive);

        // Fell off the bottom of the level
        if (_player.Position.Y > 700)
        {
            PlayerDied();
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        _camera.Follow(_player.Position, LevelSize);
        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    private void UpdateSnow(float dt)
    {
        for (int i = 0; i < _snow.Count; i++)
        {
            var s = _snow[i];
            s.Position.Y += s.Speed * dt;
            s.Position.X += s.Drift * dt;
            if (s.Position.Y > 620 || s.Position.X < -20 || s.Position.X > 1044)
            {
                s.Position.X = _rng.Next(0, 1024);
                s.Position.Y = -10;
            }
            _snow[i] = s;
        }
    }

    private void PlayerDied()
    {
        _lives--;
        if (_lives <= 0)
        {
            _gameState = GameState.GameOver;
        }
        else
        {
            _player.Reset(StartPosition);
            BuildLevel(); // Respawn enemies each life
            _camera.Follow(StartPosition, LevelSize);
        }
    }

    private void FullRestart()
    {
        _score = 0;
        _lives = 3;
        _gameState = GameState.Playing;
        _player.Reset(StartPosition);
        BuildLevel();
        _camera.Follow(StartPosition, LevelSize);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 60, 130));

        // Draw sky background (screen space)
        _spriteBatch.Begin();
        // Gradient sky using horizontal bands
        _spriteBatch.Draw(_pixel, new Rectangle(0,   0, 1024,  80), new Color(20,  60, 130));
        _spriteBatch.Draw(_pixel, new Rectangle(0,  80, 1024,  80), new Color(30,  80, 150));
        _spriteBatch.Draw(_pixel, new Rectangle(0, 160, 1024,  80), new Color(45, 100, 170));
        _spriteBatch.Draw(_pixel, new Rectangle(0, 240, 1024,  80), new Color(70, 140, 200));
        _spriteBatch.Draw(_pixel, new Rectangle(0, 320, 1024,  80), new Color(100,170, 220));
        _spriteBatch.Draw(_pixel, new Rectangle(0, 400, 1024, 200), new Color(130,200, 240));

        // Distant icebergs (parallax at 0.3x speed)
        float px = _camera.Position.X * 0.3f;
        DrawIceberg(50  - (int)(px % 1024),       380, 120, 80);
        DrawIceberg(350 - (int)(px % 1024),       400,  80, 60);
        DrawIceberg(700 - (int)(px % 1024),       370, 150, 90);
        DrawIceberg(1000 - (int)(px % 1024),      390,  90, 70);
        DrawIceberg(1300 - (int)(px % 1024) + 80, 385, 110, 75);
        _spriteBatch.End();

        // Draw world objects (camera transform)
        _spriteBatch.Begin(transformMatrix: _camera.Transform);

        foreach (var platform in _platforms)
            platform.Draw(_spriteBatch, _pixel);

        foreach (var enemy in _enemies)
            enemy.Draw(_spriteBatch, _pixel);

        _player.Draw(_spriteBatch, _pixel);

        _spriteBatch.End();

        // Draw HUD and snow (screen space, on top)
        _spriteBatch.Begin();

        foreach (var s in _snow)
            _spriteBatch.Draw(_pixel, new Rectangle((int)s.Position.X, (int)s.Position.Y, s.Size, s.Size),
                Color.White * 0.75f);

        DrawHud();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawIceberg(int x, int y, int w, int h)
    {
        // Iceberg body
        _spriteBatch.Draw(_pixel, new Rectangle(x, y + h / 3, w, h * 2 / 3),
            new Color(180, 220, 240, 180));
        // Snow cap (triangle approximated by two rects)
        _spriteBatch.Draw(_pixel, new Rectangle(x + w / 4, y, w / 2, h / 3),
            new Color(220, 240, 255, 160));
        _spriteBatch.Draw(_pixel, new Rectangle(x + w / 3, y - h / 6, w / 3, h / 6),
            new Color(235, 248, 255, 140));
    }

    private void DrawHud()
    {
        if (_font != null)
        {
            // Semi-transparent HUD panel
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 44), Color.Black * 0.45f);

            _spriteBatch.DrawString(_font, $"SCORE: {_score}", new Vector2(12, 8), Color.White);
            _spriteBatch.DrawString(_font, $"LIVES: {_lives}", new Vector2(200, 8), Color.White);

            string controls = "Arrow/WASD: Move  |  Space/Up: Jump (x2)  |  Stomp Walruses!";
            _spriteBatch.DrawString(_font, controls, new Vector2(12, 26), Color.LightCyan * 0.85f);

            if (_gameState == GameState.GameOver)
            {
                // Dim overlay
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * 0.6f);

                _spriteBatch.DrawString(_font, "GAME OVER", new Vector2(390, 230), Color.OrangeRed);
                _spriteBatch.DrawString(_font, $"Final Score: {_score}", new Vector2(415, 260), Color.White);
                _spriteBatch.DrawString(_font, "Press R to Play Again", new Vector2(380, 295), Color.LightYellow);
            }
        }
        else
        {
            // Fallback HUD without font: coloured bars
            // Score bar
            _spriteBatch.Draw(_pixel, new Rectangle(10, 10, Math.Min(_score / 3 + 20, 300), 10), Color.Gold);
            // Lives as hearts (red squares)
            for (int i = 0; i < _lives; i++)
                _spriteBatch.Draw(_pixel, new Rectangle(10 + i * 18, 26, 14, 14), Color.Red);

            if (_gameState == GameState.GameOver)
            {
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * 0.6f);
                // "GAME OVER" in large pixel blocks
                _spriteBatch.Draw(_pixel, new Rectangle(350, 240, 340, 50), Color.DarkRed);
                _spriteBatch.Draw(_pixel, new Rectangle(354, 244, 332, 42), Color.Red);
            }
        }
    }
}
