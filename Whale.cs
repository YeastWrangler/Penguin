using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Whale
{
    private Vector2 _position;
    private float _direction = 1f;
    private readonly float _leftBound;
    private readonly float _rightBound;
    private int _health = 2;
    private float _staggerTimer = 0f;
    private float _shakeX = 0f;

    private const float NormalSpeed = 50f;
    public const int Width = 72;
    public const int Height = 52;

    public bool IsAlive => _health > 0;
    public bool IsHurt => _health == 1;
    public bool IsStaggered => _staggerTimer > 0;

    public Rectangle Bounds => new Rectangle(
        (int)_position.X, (int)_position.Y, Width, Height);

    public Whale(Vector2 position, float leftBound, float rightBound)
    {
        _position = position;
        _leftBound = leftBound;
        _rightBound = rightBound;
    }

    // Returns true if the whale is killed by this hit
    public bool TakeHit()
    {
        _health--;
        _staggerTimer = 1.1f;
        _shakeX = 0f;
        return _health <= 0;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_staggerTimer > 0)
        {
            _staggerTimer -= dt;
            _shakeX = MathF.Sin(_staggerTimer * 35f) * 4f;
            return; // stop moving while staggered
        }
        _shakeX = 0f;

        _position.X += _direction * NormalSpeed * dt;

        if (_position.X <= _leftBound)
        {
            _position.X = _leftBound;
            _direction = 1f;
        }
        else if (_position.X + Width >= _rightBound)
        {
            _position.X = _rightBound - Width;
            _direction = -1f;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsAlive) return;

        // Apply shake offset during stagger
        int bx = (int)_position.X + (int)_shakeX;
        int by = (int)_position.Y;
        bool facingRight = _direction >= 0;

        // Mirror helper: X offset from the facing edge, rect width
        int Mx(int ofs, int rw) => facingRight ? bx + ofs : bx + Width - ofs - rw;

        // Body colour changes when hurt
        var bodyColor   = IsHurt ? new Color(180, 130, 110) : new Color(80, 120, 170);
        var bellyColor  = IsHurt ? new Color(240, 200, 180) : new Color(220, 230, 245);
        var headColor   = IsHurt ? new Color(150, 100, 90)  : new Color(60,  95, 145);
        var finColor    = IsHurt ? new Color(140, 90,  80)  : new Color(50,  80, 130);

        // Main body
        spriteBatch.Draw(pixel, new Rectangle(bx + 6, by + 18, 56, 26), bodyColor);

        // Head / snout (facing side)
        spriteBatch.Draw(pixel, new Rectangle(Mx(48, 22), by + 12, 22, 32), headColor);

        // Underbelly (lighter)
        spriteBatch.Draw(pixel, new Rectangle(bx + 10, by + 30, 44, 14), bellyColor);

        // Dorsal fin (top, slightly toward back)
        spriteBatch.Draw(pixel, new Rectangle(Mx(20, 14), by + 6, 14, 14), finColor);
        spriteBatch.Draw(pixel, new Rectangle(Mx(22, 10), by + 10, 10, 10), finColor);

        // Tail fins (at back)
        spriteBatch.Draw(pixel, new Rectangle(Mx(0, 12), by + 16, 12, 10), finColor);
        spriteBatch.Draw(pixel, new Rectangle(Mx(0, 12), by + 28, 12, 10), finColor);
        spriteBatch.Draw(pixel, new Rectangle(Mx(0, 8),  by + 24, 8,  6),  bodyColor); // tail join

        // Eye (near front)
        spriteBatch.Draw(pixel, new Rectangle(Mx(56, 8), by + 18, 8, 8), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(Mx(57, 3), by + 19, 3, 3), Color.White);

        // Blowhole (top of head area)
        spriteBatch.Draw(pixel, new Rectangle(Mx(36, 8), by + 16, 8, 5), new Color(30, 60, 110));

        // Mouth line
        spriteBatch.Draw(pixel, new Rectangle(Mx(60, 8), by + 34, 8, 3), new Color(30, 60, 110));

        // Blowhole water spray (when not staggered and healthy)
        if (!IsStaggered && !IsHurt)
        {
            float spray = MathF.Sin((float)Environment.TickCount64 / 400f);
            int sprayH = (int)(6 + spray * 3);
            spriteBatch.Draw(pixel, new Rectangle(Mx(37, 6), by + 16 - sprayH, 6, sprayH),
                new Color(150, 210, 240, 180));
        }

        // Hurt marks (small X marks when health == 1)
        if (IsHurt && !IsStaggered)
        {
            spriteBatch.Draw(pixel, new Rectangle(Mx(28, 12), by + 20, 12, 3), new Color(180, 50, 50));
            spriteBatch.Draw(pixel, new Rectangle(Mx(30, 4),  by + 18, 4, 10), new Color(180, 50, 50));
        }

        // Stagger stars orbiting above
        if (IsStaggered)
        {
            // Flash the body orange
            spriteBatch.Draw(pixel, new Rectangle(bx + 6, by + 18, 56, 26),
                Color.OrangeRed * 0.45f);

            for (int i = 0; i < 3; i++)
            {
                float angle = _staggerTimer * 9f + i * (MathF.PI * 2f / 3f);
                int sx = bx + Width / 2 + (int)(MathF.Cos(angle) * 18);
                int sy = by - 10 + (int)(MathF.Sin(angle) * 7);
                Color[] starColors = { Color.Yellow, Color.Orange, Color.Red };
                spriteBatch.Draw(pixel, new Rectangle(sx - 4, sy - 4, 8, 8), starColors[i]);
                spriteBatch.Draw(pixel, new Rectangle(sx - 1, sy - 4, 2, 8), Color.White * 0.7f);
                spriteBatch.Draw(pixel, new Rectangle(sx - 4, sy - 1, 8, 2), Color.White * 0.7f);
            }
        }
    }
}
