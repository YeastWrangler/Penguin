using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenguinPlatformer;

public enum GameState { Playing, Dancing, LevelComplete, GameOver }

public struct SnowParticle
{
    public Vector2 Position;
    public float Speed;
    public int Size;
    public float Drift;
}

public struct CelebParticle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Life;
    public float MaxLife;
    public int Size;
}

public class Game1 : Game
{
    private static readonly Vector2 StartPosition = new Vector2(100, 474);
    private static readonly Vector2 Level1Size = new Vector2(4000, 600);
    private static readonly Vector2 Level2Size = new Vector2(4500, 600);
    private Vector2 CurrentLevelSize => _currentLevel == 1 ? Level1Size : Level2Size;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont? _font;

    private Player _player = null!;
    private Camera _camera = null!;

    // Level 1 entities
    private readonly List<Platform> _platforms = new();
    private List<Enemy> _enemies = new();          // walruses (level 1)
    private Fish _fish = null!;
    private Goal _goal = null!;

    // Level 2 entities
    private List<Whale> _whales = new();
    private List<BreakablePlatform> _breakablePlatforms = new();

    private int _currentLevel = 1;
    private int _score;
    private int _lives = 3;
    private GameState _gameState = GameState.Playing;

    // Level transition banner
    private float _levelBannerTimer = 0f;
    private const float LevelBannerDuration = 2.8f;

    private readonly List<SnowParticle> _snow = new();
    private readonly List<CelebParticle> _celebParticles = new();
    private readonly Random _rng = new();
    private KeyboardState _prevKeyboard;

    private static readonly Color[] CelebColors =
    {
        Color.Gold, Color.Yellow, Color.LightBlue, Color.White,
        Color.Pink, Color.Orange, Color.Cyan, Color.LimeGreen
    };

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 600;
    }

    protected override void Initialize() => base.Initialize();

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
        LoadLevel(1);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Level building
    // ──────────────────────────────────────────────────────────────────────────

    private void LoadLevel(int level)
    {
        _currentLevel = level;
        _celebParticles.Clear();
        _player.Reset(StartPosition);

        if (level == 1) BuildLevel1();
        else             BuildLevel2();

        _camera.Follow(StartPosition, CurrentLevelSize);
    }

    private void BuildLevel1()
    {
        _platforms.Clear();
        _enemies.Clear();
        _whales.Clear();
        _breakablePlatforms.Clear();

        var iceBlue    = new Color(80, 160, 210);
        var iceLighter = new Color(100, 185, 230);

        _platforms.Add(new Platform(0,    520, 4000, 80, iceBlue));    // ground
        _platforms.Add(new Platform(200,  420, 160, 20, iceLighter));  // P1
        _platforms.Add(new Platform(430,  350, 180, 20, iceLighter));  // P2
        _platforms.Add(new Platform(680,  270, 140, 20, iceLighter));  // P3
        _platforms.Add(new Platform(870,  390, 130, 20, iceLighter));  // P4
        _platforms.Add(new Platform(1060, 310, 170, 20, iceLighter));  // P5
        _platforms.Add(new Platform(1290, 240, 190, 20, iceLighter));  // P6  ← fish
        _platforms.Add(new Platform(1530, 350, 150, 20, iceLighter));  // P7
        _platforms.Add(new Platform(1730, 420, 130, 20, iceLighter));  // P8
        _platforms.Add(new Platform(1920, 300, 200, 20, iceLighter));  // P9
        _platforms.Add(new Platform(2170, 210, 160, 20, iceLighter));  // P10
        _platforms.Add(new Platform(2380, 380, 180, 20, iceLighter));  // P11
        _platforms.Add(new Platform(2610, 270, 150, 20, iceLighter));  // P12
        _platforms.Add(new Platform(2810, 350, 170, 20, iceLighter));  // P13
        _platforms.Add(new Platform(3030, 250, 200, 20, iceLighter));  // P14
        _platforms.Add(new Platform(3280, 390, 130, 20, iceLighter));  // P15
        _platforms.Add(new Platform(3460, 300, 180, 20, iceLighter));  // P16
        _platforms.Add(new Platform(3700, 220, 200, 20, iceLighter));  // P17

        // Ground-patrol walruses (Y = 520 - 40 = 480)
        _enemies.Add(new Enemy(new Vector2(360,  480), 150,  600));
        _enemies.Add(new Enemy(new Vector2(770,  480), 600,  1000));
        _enemies.Add(new Enemy(new Vector2(1380, 480), 1100, 1700));
        _enemies.Add(new Enemy(new Vector2(1960, 480), 1700, 2300));
        _enemies.Add(new Enemy(new Vector2(2530, 480), 2300, 2750));
        _enemies.Add(new Enemy(new Vector2(2950, 480), 2750, 3100));
        _enemies.Add(new Enemy(new Vector2(3400, 480), 3200, 3850));

        // Platform walruses (Y = platform top - 40)
        _enemies.Add(new Enemy(new Vector2(210,  380), 200,  340));   // P1 top=420
        _enemies.Add(new Enemy(new Vector2(450,  310), 430,  580));   // P2 top=350
        _enemies.Add(new Enemy(new Vector2(695,  230), 680,  788));   // P3 top=270
        _enemies.Add(new Enemy(new Vector2(1075, 270), 1060, 1178));  // P5 top=310
        _enemies.Add(new Enemy(new Vector2(1305, 200), 1290, 1427));  // P6 top=240
        _enemies.Add(new Enemy(new Vector2(1940, 260), 1920, 2068));  // P9 top=300
        _enemies.Add(new Enemy(new Vector2(2625, 230), 2610, 2708));  // P12 top=270
        _enemies.Add(new Enemy(new Vector2(3045, 210), 3030, 3178));  // P14 top=250
        _enemies.Add(new Enemy(new Vector2(3715, 180), 3700, 3848));  // P17 top=220

        // Fish on P6 (top=240, fish H=22 → Y=218)
        _fish = new Fish(new Vector2(1362, 218));

        // Goal at end of level 1
        _goal = new Goal(new Vector2(3870, 520));
    }

    private void BuildLevel2()
    {
        _platforms.Clear();
        _enemies.Clear();
        _whales.Clear();
        _breakablePlatforms.Clear();

        var groundColor = new Color(55, 110, 165);
        var solidColor  = new Color(80, 160, 210);

        // Ground (full level 2 width)
        _platforms.Add(new Platform(0, 520, 4500, 80, groundColor));

        // Solid platforms
        _platforms.Add(new Platform(100,  430, 130, 20, solidColor));  // P1
        _platforms.Add(new Platform(350,  350, 120, 20, solidColor));  // P2
        _platforms.Add(new Platform(590,  430, 120, 20, solidColor));  // P3
        _platforms.Add(new Platform(810,  340, 130, 20, solidColor));  // P4
        _platforms.Add(new Platform(1020, 250, 130, 20, solidColor));  // P5
        _platforms.Add(new Platform(1250, 390, 120, 20, solidColor));  // P6
        _platforms.Add(new Platform(1450, 310, 130, 20, solidColor));  // P7
        _platforms.Add(new Platform(1680, 220, 130, 20, solidColor));  // P8
        _platforms.Add(new Platform(1900, 350, 130, 20, solidColor));  // P9
        _platforms.Add(new Platform(2120, 260, 130, 20, solidColor));  // P10 ← fish
        _platforms.Add(new Platform(2350, 170, 150, 20, solidColor));  // P11 (very high)
        _platforms.Add(new Platform(2560, 340, 120, 20, solidColor));  // P12
        _platforms.Add(new Platform(2770, 450, 120, 20, solidColor));  // P13
        _platforms.Add(new Platform(2990, 310, 130, 20, solidColor));  // P14
        _platforms.Add(new Platform(3210, 220, 130, 20, solidColor));  // P15
        _platforms.Add(new Platform(3440, 370, 120, 20, solidColor));  // P16
        _platforms.Add(new Platform(3660, 280, 140, 20, solidColor));  // P17
        _platforms.Add(new Platform(3900, 390, 130, 20, solidColor));  // P18
        _platforms.Add(new Platform(4130, 280, 130, 20, solidColor));  // P19
        _platforms.Add(new Platform(4330, 390, 130, 20, solidColor));  // P20 (near goal)

        // Breakable ice patches (spread throughout, some essential stepping-stones)
        void B(int x, int y, int w = 90) => _breakablePlatforms.Add(new BreakablePlatform(x, y, w, 20));
        B(240, 400);   // between ground and P1
        B(490, 310);   // P2 area
        B(720, 390);   // P3↔P4
        B(940, 300);   // P4↔P5 (risky)
        B(1150, 340);  // mid-level
        B(1360, 260);  // steps to P7
        B(1570, 280);  // steps to P8
        B(1800, 300);  //
        B(2010, 210);  // high breakable
        B(2250, 225);  // leads to P11 (high)
        B(2470, 390);  //
        B(2680, 400);  //
        B(2890, 270);  //
        B(3100, 270);  //
        B(3330, 300);  //
        B(3560, 340);  //
        B(3800, 350);  //
        B(4040, 340);  // near end
        B(4240, 340);  // near goal

        // Ground whales (Y = 520 - 52 = 468)
        _whales.Add(new Whale(new Vector2(300,  468), 0,    600));
        _whales.Add(new Whale(new Vector2(750,  468), 500,  1050));
        _whales.Add(new Whale(new Vector2(1350, 468), 1050, 1700));
        _whales.Add(new Whale(new Vector2(2100, 468), 1700, 2500));
        _whales.Add(new Whale(new Vector2(2900, 468), 2500, 3200));
        _whales.Add(new Whale(new Vector2(3700, 468), 3200, 4150));

        // Platform whales (Y = platform.top - 52)
        _whales.Add(new Whale(new Vector2(360,  298), 350,  448));    // P2 top=350 → Y=298
        _whales.Add(new Whale(new Vector2(1030, 198), 1020, 1118));   // P5 top=250 → Y=198
        _whales.Add(new Whale(new Vector2(1690, 168), 1680, 1778));   // P8 top=220 → Y=168
        _whales.Add(new Whale(new Vector2(2360, 118), 2350, 2468));   // P11 top=170 → Y=118 (boss)
        _whales.Add(new Whale(new Vector2(3220, 168), 3210, 3308));   // P15 top=220 → Y=168

        // Fish on P10 (top=260, fish H=22 → Y=238)
        _fish = new Fish(new Vector2(2162, 238));

        // Goal at end of level 2
        _goal = new Goal(new Vector2(4380, 520));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Update
    // ──────────────────────────────────────────────────────────────────────────

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape)) Exit();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateSnow(dt);

        if (_levelBannerTimer > 0)
            _levelBannerTimer -= dt;

        // ── GAME OVER ──────────────────────────────────────────────────────
        if (_gameState == GameState.GameOver)
        {
            if (keyboard.IsKeyDown(Keys.R) && !_prevKeyboard.IsKeyDown(Keys.R))
                FullRestart();
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        // ── LEVEL COMPLETE ─────────────────────────────────────────────────
        if (_gameState == GameState.LevelComplete)
        {
            if (keyboard.IsKeyDown(Keys.R) && !_prevKeyboard.IsKeyDown(Keys.R))
                FullRestart();
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        // ── DANCING ────────────────────────────────────────────────────────
        if (_gameState == GameState.Dancing)
        {
            _player.Update(gameTime, keyboard, BuildSolidRects());
            UpdateCelebParticles(dt);
            _camera.Follow(_player.Position, CurrentLevelSize);

            if (_player.DanceComplete)
            {
                if (_currentLevel == 1)
                {
                    // Transition to level 2
                    _levelBannerTimer = LevelBannerDuration;
                    LoadLevel(2);
                    _gameState = GameState.Playing;
                }
                else
                {
                    _gameState = GameState.LevelComplete;
                }
            }
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        // ── PLAYING ────────────────────────────────────────────────────────
        var solidRects = BuildSolidRects();
        _player.Update(gameTime, keyboard, solidRects);
        _fish.Update(gameTime);
        _goal.Update(gameTime);

        foreach (var e in _enemies) e.Update(gameTime);
        foreach (var w in _whales)  w.Update(gameTime);

        // Update breakable platforms — detect if player is standing on each one
        var pb = _player.Bounds;
        foreach (var bp in _breakablePlatforms)
        {
            bool standingOn = !bp.IsBroken
                && Math.Abs(pb.Bottom - bp.Bounds.Top) <= 3
                && pb.Right > bp.Bounds.Left
                && pb.Left  < bp.Bounds.Right;
            bp.Update(gameTime, standingOn);
        }

        // Walrus (level 1) enemy collisions
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (!enemy.IsAlive) continue;

            var pr = _player.Bounds;
            var er = enemy.Bounds;
            if (!pr.Intersects(er)) continue;

            bool stomp = _player.Velocity.Y > 0 && (pr.Bottom - er.Top) < 18;
            if (stomp)
            {
                enemy.Kill();
                _player.BounceOff();
                _score += 100;
            }
            else
            {
                if (HandlePlayerHit(enemy.Bounds.Center.X, keyboard)) return;
            }
        }
        _enemies.RemoveAll(e => !e.IsAlive);

        // Whale (level 2) enemy collisions — needs 2 stomps
        for (int i = _whales.Count - 1; i >= 0; i--)
        {
            var whale = _whales[i];
            if (!whale.IsAlive) continue;

            var pr = _player.Bounds;
            var wr = whale.Bounds;
            if (!pr.Intersects(wr)) continue;

            bool stomp = _player.Velocity.Y > 0 && (pr.Bottom - wr.Top) < 22;
            if (stomp)
            {
                bool killed = whale.TakeHit();
                _player.BounceOff();
                _score += killed ? 300 : 150;
            }
            else
            {
                if (HandlePlayerHit(whale.Bounds.Center.X, keyboard)) return;
            }
        }
        _whales.RemoveAll(w => !w.IsAlive);

        // Fish power-up
        if (!_fish.IsCollected && _player.Bounds.Intersects(_fish.Bounds))
        {
            if (_player.IsNormalSize) { _lives++; _score += 500; }
            else                      { _player.GrowUp(); _score += 200; }
            _fish.Collect();
        }

        // Goal / level end trigger
        if (_player.Bounds.Intersects(_goal.TriggerBounds))
        {
            _player.StartDance();
            SpawnCelebration();
            _gameState = GameState.Dancing;
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        // Fell off level
        if (_player.Position.Y > 700)
        {
            PlayerDied();
            _prevKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        _camera.Follow(_player.Position, CurrentLevelSize);
        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    // Builds the combined list of solid rectangles for this frame
    private List<Rectangle> BuildSolidRects()
    {
        var rects = new List<Rectangle>(_platforms.Count + _breakablePlatforms.Count);
        foreach (var p in _platforms)
            rects.Add(p.Bounds);
        foreach (var bp in _breakablePlatforms)
            if (!bp.IsBroken)
                rects.Add(bp.Bounds);
        return rects;
    }

    // Returns true if the game loop should return early (player died)
    private bool HandlePlayerHit(int enemyCenterX, KeyboardState keyboard)
    {
        bool died = _player.TakeHit(enemyCenterX);
        if (died)
        {
            PlayerDied();
            _prevKeyboard = keyboard;
            base.Update(new GameTime()); // tick the base
            return true;
        }
        return false;
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

    private void SpawnCelebration()
    {
        _celebParticles.Clear();
        var center = _player.Position + new Vector2(_player.CW / 2f, _player.CH / 2f);
        for (int i = 0; i < 70; i++)
        {
            float angle = _rng.NextSingle() * MathF.PI * 2f;
            float speed = _rng.Next(80, 450);
            float life  = _rng.NextSingle() * 1.8f + 0.6f;
            _celebParticles.Add(new CelebParticle
            {
                Position = center,
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed - 260f),
                Color    = CelebColors[_rng.Next(CelebColors.Length)],
                Life     = life,
                MaxLife  = life,
                Size     = _rng.Next(5, 14)
            });
        }
    }

    private void UpdateCelebParticles(float dt)
    {
        for (int i = _celebParticles.Count - 1; i >= 0; i--)
        {
            var p = _celebParticles[i];
            p.Velocity.Y += 600f * dt;
            p.Position   += p.Velocity * dt;
            p.Life       -= dt;
            if (p.Life <= 0) _celebParticles.RemoveAt(i);
            else             _celebParticles[i] = p;
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
            // Respawn at the start of the current level
            LoadLevel(_currentLevel);
        }
    }

    private void FullRestart()
    {
        _score = 0;
        _lives = 3;
        _gameState = GameState.Playing;
        LoadLevel(1);
    }

    private void InitSnow()
    {
        _snow.Clear();
        for (int i = 0; i < 90; i++)
        {
            _snow.Add(new SnowParticle
            {
                Position = new Vector2(_rng.Next(0, 1024), _rng.Next(0, 600)),
                Speed    = _rng.Next(30, 100),
                Size     = _rng.Next(2, 6),
                Drift    = (_rng.NextSingle() - 0.5f) * 20f
            });
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Draw
    // ──────────────────────────────────────────────────────────────────────────

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 60, 130));

        // ── SKY (screen space) ────────────────────────────────────────────
        bool level2 = _currentLevel == 2;
        _spriteBatch.Begin();
        if (level2)
        {
            // Darker, stormier sky for level 2
            _spriteBatch.Draw(_pixel, new Rectangle(0,   0, 1024,  80), new Color(10,  30,  80));
            _spriteBatch.Draw(_pixel, new Rectangle(0,  80, 1024,  80), new Color(15,  45,  105));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 160, 1024,  80), new Color(25,  60,  130));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 240, 1024,  80), new Color(40,  85,  155));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 320, 1024,  80), new Color(65, 115,  175));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 400, 1024, 200), new Color(90, 150,  205));
        }
        else
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0,   0, 1024,  80), new Color(20,  60, 130));
            _spriteBatch.Draw(_pixel, new Rectangle(0,  80, 1024,  80), new Color(30,  80, 150));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 160, 1024,  80), new Color(45, 100, 170));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 240, 1024,  80), new Color(70, 140, 200));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 320, 1024,  80), new Color(100,170, 220));
            _spriteBatch.Draw(_pixel, new Rectangle(0, 400, 1024, 200), new Color(130,200, 240));
        }

        float px = _camera.Position.X * 0.3f;
        DrawIceberg(50   - (int)(px % 1200),      380, 120, 80);
        DrawIceberg(350  - (int)(px % 1200),      400,  80, 60);
        DrawIceberg(700  - (int)(px % 1200),      370, 150, 90);
        DrawIceberg(1000 - (int)(px % 1200) + 80, 390,  90, 70);
        DrawIceberg(1300 - (int)(px % 1200) + 80, 385, 110, 75);
        _spriteBatch.End();

        // ── WORLD (camera transform) ──────────────────────────────────────
        _spriteBatch.Begin(transformMatrix: _camera.Transform);

        foreach (var p  in _platforms)          p.Draw(_spriteBatch, _pixel);
        foreach (var bp in _breakablePlatforms) bp.Draw(_spriteBatch, _pixel);

        _fish.Draw(_spriteBatch, _pixel);
        _goal.Draw(_spriteBatch, _pixel);

        foreach (var e in _enemies) e.Draw(_spriteBatch, _pixel);
        foreach (var w in _whales)  w.Draw(_spriteBatch, _pixel);

        _player.Draw(_spriteBatch, _pixel);

        foreach (var p in _celebParticles)
        {
            float alpha = p.Life / p.MaxLife;
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)p.Position.X - p.Size / 2, (int)p.Position.Y - p.Size / 2, p.Size, p.Size),
                p.Color * alpha);
        }

        _spriteBatch.End();

        // ── HUD + SNOW (screen space) ─────────────────────────────────────
        _spriteBatch.Begin();

        foreach (var s in _snow)
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)s.Position.X, (int)s.Position.Y, s.Size, s.Size),
                Color.White * 0.75f);

        DrawHud();

        // Level transition banner
        if (_levelBannerTimer > 0)
        {
            float fade = MathHelper.Clamp(_levelBannerTimer / 0.6f, 0f, 1f);
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * (fade * 0.75f));

            if (_font != null)
            {
                _spriteBatch.DrawString(_font, $"LEVEL {_currentLevel}",
                    new Vector2(430, 255), Color.Gold * fade);
                string subtitle = _currentLevel == 2
                    ? "Beware the Whales!  Watch out for cracking ice!"
                    : "";
                if (subtitle.Length > 0)
                    _spriteBatch.DrawString(_font, subtitle, new Vector2(240, 285), Color.LightCyan * fade);
            }
            else
            {
                // Fallback banner
                _spriteBatch.Draw(_pixel, new Rectangle(380, 240, 280, 50),
                    Color.Gold * (fade * 0.9f));
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawIceberg(int x, int y, int w, int h)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(x, y + h / 3, w, h * 2 / 3), new Color(180, 220, 240, 180));
        _spriteBatch.Draw(_pixel, new Rectangle(x + w / 4, y, w / 2, h / 3), new Color(220, 240, 255, 160));
        _spriteBatch.Draw(_pixel, new Rectangle(x + w / 3, y - h / 6, w / 3, h / 6), new Color(235, 248, 255, 140));
    }

    private void DrawHud()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 44), Color.Black * 0.45f);

        if (_font != null)
        {
            _spriteBatch.DrawString(_font, $"LEVEL {_currentLevel}", new Vector2(12,  8), Color.LightYellow);
            _spriteBatch.DrawString(_font, $"SCORE: {_score}",       new Vector2(110, 8), Color.White);
            _spriteBatch.DrawString(_font, $"LIVES: {_lives}",       new Vector2(310, 8), Color.White);

            string sizeText  = _player.IsNormalSize ? "BIG" : "SMALL — find the fish!";
            var    sizeColor = _player.IsNormalSize ? Color.LightCyan : Color.Yellow;
            _spriteBatch.DrawString(_font, sizeText, new Vector2(470, 8), sizeColor);

            string controls = _currentLevel == 2
                ? "Jump on Whales TWICE to kill!  Stand 3s on cracked ice = fall!"
                : "Stomp Walruses!  Collect the fish!";
            _spriteBatch.DrawString(_font, controls, new Vector2(12, 27), Color.LightCyan * 0.8f);

            if (_gameState == GameState.GameOver)
            {
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * 0.6f);
                _spriteBatch.DrawString(_font, "GAME OVER",              new Vector2(400, 228), Color.OrangeRed);
                _spriteBatch.DrawString(_font, $"Final Score: {_score}", new Vector2(390, 256), Color.White);
                _spriteBatch.DrawString(_font, "Press R to Play Again",  new Vector2(375, 286), Color.LightYellow);
            }

            if (_gameState == GameState.LevelComplete)
            {
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * 0.5f);
                _spriteBatch.DrawString(_font, "YOU WIN!  GAME COMPLETE!", new Vector2(330, 218), Color.Gold);
                _spriteBatch.DrawString(_font, $"Final Score: {_score}",   new Vector2(390, 248), Color.White);
                _spriteBatch.DrawString(_font, $"Lives remaining: {_lives}", new Vector2(372, 272), Color.LightCyan);
                _spriteBatch.DrawString(_font, "Press R to Play Again",    new Vector2(375, 302), Color.LightYellow);
            }
        }
        else
        {
            // Fallback (no font)
            _spriteBatch.Draw(_pixel, new Rectangle(10, 10, Math.Min(_score / 3 + 20, 300), 10), Color.Gold);
            for (int i = 0; i < _lives; i++)
                _spriteBatch.Draw(_pixel, new Rectangle(10 + i * 18, 26, 14, 14), Color.Red);
            // Level indicator
            for (int i = 0; i < _currentLevel; i++)
                _spriteBatch.Draw(_pixel, new Rectangle(350 + i * 18, 10, 14, 14), Color.Gold);

            if (_gameState is GameState.GameOver or GameState.LevelComplete)
            {
                _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1024, 600), Color.Black * 0.6f);
                var c = _gameState == GameState.LevelComplete ? Color.Gold : Color.DarkRed;
                _spriteBatch.Draw(_pixel, new Rectangle(320, 240, 400, 60), c);
            }
        }
    }
}
