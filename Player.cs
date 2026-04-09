using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenguinPlatformer;

public enum PlayerSize { Normal, Small }

public class Player
{
    private Vector2 _position;
    private Vector2 _velocity;
    private int _jumpsLeft;
    private bool _facingRight = true;
    private KeyboardState _prevKeyboard;

    // Size / hit system
    private PlayerSize _size = PlayerSize.Normal;
    private float _stunTimer = 0f;        // counts down; > 0 means invincible + blinking
    private float _knockbackTimer = 0f;   // counts down; > 0 means no player control
    private float _blinkTimer = 0f;
    private bool _blinkVisible = true;

    // Dance
    private bool _isDancing = false;
    private float _danceTimer = 0f;
    private float _danceFlipTimer = 0f;
    private float _danceBaseY;
    private float _danceYOffset;

    private const float Gravity = 1400f;
    private const float MoveSpeed = 300f;
    private const float JumpForce = -620f;
    private const float LevelWidth = 4000f;
    private const int MaxJumps = 2;

    // Normal (big) penguin size
    public const int NW = 36, NH = 46;
    // Small penguin size (~70%)
    public const int SW = 26, SH = 32;

    private const float StunDuration = 2.2f;
    private const float KnockbackDuration = 0.55f;
    private const float BlinkInterval = 0.1f;
    public const float DanceDuration = 3.5f;

    public Vector2 Position => _position;
    public Vector2 Velocity => _velocity;
    public bool IsAlive { get; private set; } = true;
    public bool IsInvincible => _stunTimer > 0;
    public bool IsNormalSize => _size == PlayerSize.Normal;
    public bool IsDancing => _isDancing;
    public bool DanceComplete => _isDancing && _danceTimer >= DanceDuration;

    public int CW => _size == PlayerSize.Normal ? NW : SW;
    public int CH => _size == PlayerSize.Normal ? NH : SH;
    public Rectangle Bounds => new Rectangle((int)_position.X, (int)_position.Y, CW, CH);

    public Player(Vector2 startPosition)
    {
        _position = startPosition;
        _jumpsLeft = MaxJumps;
        IsAlive = true;
    }

    // Returns true if the player should die (was already small when hit).
    // Applies knockback impulse and starts stun/blink regardless.
    public bool TakeHit(float enemyX)
    {
        if (_stunTimer > 0) return false; // invincibility frames — ignore hit

        // Push away from the enemy
        float pushDir = (_position.X + CW / 2f) > enemyX ? 1f : -1f;
        _velocity = new Vector2(pushDir * 270f, -360f);
        _stunTimer = StunDuration;
        _knockbackTimer = KnockbackDuration;
        _blinkTimer = BlinkInterval;

        if (_size == PlayerSize.Normal)
        {
            // Shrink: adjust Y so the bottom of the player stays at the same spot
            _position.Y += (NH - SH);
            _size = PlayerSize.Small;
            return false; // alive but now small
        }
        return true; // was already small → die
    }

    // Restore to normal size (fish power-up). Keeps bottom position.
    public void GrowUp()
    {
        if (_size == PlayerSize.Normal) return;
        _position.Y -= (NH - SH);
        _size = PlayerSize.Normal;
    }

    // Called when stomping an enemy — gives a small upward bounce
    public void BounceOff()
    {
        _velocity.Y = JumpForce * 0.55f;
        _jumpsLeft = MaxJumps;
    }

    public void Die() => IsAlive = false;

    public void StartDance()
    {
        _isDancing = true;
        _danceTimer = 0f;
        _danceFlipTimer = 0f;
        _danceBaseY = _position.Y;
        _danceYOffset = 0f;
        _velocity = Vector2.Zero;
        _stunTimer = 0f;
        _blinkVisible = true;
    }

    public void Reset(Vector2 position)
    {
        _position = position;
        _velocity = Vector2.Zero;
        IsAlive = true;
        _jumpsLeft = MaxJumps;
        _facingRight = true;
        _size = PlayerSize.Normal;
        _stunTimer = 0f;
        _knockbackTimer = 0f;
        _blinkTimer = 0f;
        _blinkVisible = true;
        _isDancing = false;
        _danceYOffset = 0f;
    }

    public void Update(GameTime gameTime, KeyboardState keyboard, List<Platform> platforms)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // --- DANCE MODE ---
        if (_isDancing)
        {
            _danceTimer += dt;
            _danceFlipTimer += dt;
            if (_danceFlipTimer >= 0.18f)
            {
                _facingRight = !_facingRight;
                _danceFlipTimer = 0f;
            }
            // Hop up and down with a bouncy sine curve
            _danceYOffset = -MathF.Abs(MathF.Sin(_danceTimer * 5.5f)) * 38f;
            _prevKeyboard = keyboard;
            return;
        }

        // --- STUN / BLINK TICK ---
        if (_stunTimer > 0)
        {
            _stunTimer -= dt;
            _blinkTimer -= dt;
            if (_blinkTimer <= 0)
            {
                _blinkVisible = !_blinkVisible;
                _blinkTimer = BlinkInterval;
            }
            if (_stunTimer <= 0)
            {
                _stunTimer = 0f;
                _blinkVisible = true;
            }
        }

        // --- KNOCKBACK PHASE (no input control) ---
        if (_knockbackTimer > 0)
        {
            _knockbackTimer -= dt;
            _velocity.Y += Gravity * dt;

            _position.X += _velocity.X * dt;
            _position.X = MathHelper.Clamp(_position.X, 0, LevelWidth - CW);

            _position.Y += _velocity.Y * dt;

            foreach (var platform in platforms)
            {
                if (!Bounds.Intersects(platform.Bounds)) continue;
                if (_velocity.Y >= 0)
                {
                    _position.Y = platform.Bounds.Top - CH;
                    _jumpsLeft = MaxJumps;
                }
                else
                {
                    _position.Y = platform.Bounds.Bottom;
                }
                _velocity.Y = 0;
            }

            _prevKeyboard = keyboard;
            return;
        }

        // --- NORMAL INPUT ---
        float moveX = 0;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
        {
            moveX = -1f;
            _facingRight = false;
        }
        else if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
        {
            moveX = 1f;
            _facingRight = true;
        }

        bool jumpJustPressed =
            (keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W)) &&
            !(_prevKeyboard.IsKeyDown(Keys.Space) || _prevKeyboard.IsKeyDown(Keys.Up) || _prevKeyboard.IsKeyDown(Keys.W));

        if (jumpJustPressed && _jumpsLeft > 0)
        {
            _velocity.Y = JumpForce;
            _jumpsLeft--;
        }

        _velocity.Y += Gravity * dt;
        _velocity.X = moveX * MoveSpeed;

        // Move X + collide
        _position.X += _velocity.X * dt;
        _position.X = MathHelper.Clamp(_position.X, 0, LevelWidth - CW);

        foreach (var platform in platforms)
        {
            if (!Bounds.Intersects(platform.Bounds)) continue;
            if (_velocity.X > 0)
                _position.X = platform.Bounds.Left - CW;
            else if (_velocity.X < 0)
                _position.X = platform.Bounds.Right;
            _velocity.X = 0;
        }

        // Move Y + collide
        _position.Y += _velocity.Y * dt;

        foreach (var platform in platforms)
        {
            if (!Bounds.Intersects(platform.Bounds)) continue;
            if (_velocity.Y >= 0)
            {
                _position.Y = platform.Bounds.Top - CH;
                _jumpsLeft = MaxJumps;
            }
            else
            {
                _position.Y = platform.Bounds.Bottom;
            }
            _velocity.Y = 0;
        }

        _prevKeyboard = keyboard;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!_blinkVisible) return;

        int x = (int)_position.X;
        int y = (int)_position.Y + (int)_danceYOffset;
        int w = CW;
        int h = CH;

        // Mx: mirrors an X offset depending on facing direction.
        // offset = distance from the "facing" edge, rw = rect width.
        int Mx(int offset, int rw) => _facingRight ? x + offset : x + w - offset - rw;

        if (_size == PlayerSize.Normal)
            DrawNormal(spriteBatch, pixel, x, y, w, h, Mx);
        else
            DrawSmall(spriteBatch, pixel, x, y, w, h, Mx);
    }

    private void DrawNormal(SpriteBatch sp, Texture2D px, int x, int y, int w, int h,
                             Func<int, int, int> Mx)
    {
        // Black tuxedo body
        sp.Draw(px, new Rectangle(x, y, w, h), Color.Black);

        // White belly
        sp.Draw(px, new Rectangle(Mx(8, 20), y + 16, 20, 28), Color.White);

        // White face
        sp.Draw(px, new Rectangle(Mx(5, 26), y + 1, 26, 20), Color.White);

        // Eyes (two black patches on white face, symmetric)
        sp.Draw(px, new Rectangle(Mx(6, 6), y + 3, 6, 7), Color.Black);
        sp.Draw(px, new Rectangle(Mx(20, 6), y + 3, 6, 7), Color.Black);
        // Eye shines
        sp.Draw(px, new Rectangle(Mx(7, 3), y + 4, 3, 3), Color.White);
        sp.Draw(px, new Rectangle(Mx(21, 3), y + 4, 3, 3), Color.White);

        // Orange beak (on the facing side)
        sp.Draw(px, new Rectangle(Mx(26, 8), y + 11, 8, 5), new Color(255, 140, 0));
        sp.Draw(px, new Rectangle(Mx(28, 4), y + 13, 4, 3), new Color(200, 100, 0));

        // Orange feet
        sp.Draw(px, new Rectangle(x + 2, y + h - 5, 14, 5), new Color(255, 140, 0));
        sp.Draw(px, new Rectangle(x + w - 16, y + h - 5, 14, 5), new Color(255, 140, 0));

        // Wing flipper (on the back/non-facing side)
        sp.Draw(px, new Rectangle(Mx(0, 7), y + 14, 7, 22), Color.Black);

        // Dance: raise both wings wide
        if (_isDancing)
        {
            float wave = (MathF.Sin(_danceTimer * 8f) + 1f) / 2f;
            int armRaise = (int)(wave * 10f);
            sp.Draw(px, new Rectangle(x - 8, y + 6 + armRaise, 10, 18), Color.Black);
            sp.Draw(px, new Rectangle(x + w - 2, y + 6 + armRaise, 10, 18), Color.Black);

            // Rosy cheeks during dance
            sp.Draw(px, new Rectangle(Mx(9, 5), y + 13, 5, 4), new Color(255, 150, 150, 180));
            sp.Draw(px, new Rectangle(Mx(18, 5), y + 13, 5, 4), new Color(255, 150, 150, 180));
        }
    }

    private void DrawSmall(SpriteBatch sp, Texture2D px, int x, int y, int w, int h,
                            Func<int, int, int> Mx)
    {
        // Black tuxedo body (smaller)
        sp.Draw(px, new Rectangle(x, y, w, h), Color.Black);

        // White belly
        sp.Draw(px, new Rectangle(Mx(5, 16), y + 11, 16, 19), Color.White);

        // White face
        sp.Draw(px, new Rectangle(Mx(4, 18), y + 1, 18, 14), Color.White);

        // Eyes
        sp.Draw(px, new Rectangle(Mx(5, 5), y + 2, 5, 6), Color.Black);
        sp.Draw(px, new Rectangle(Mx(14, 5), y + 2, 5, 6), Color.Black);
        // Eye shines
        sp.Draw(px, new Rectangle(Mx(6, 2), y + 3, 2, 2), Color.White);
        sp.Draw(px, new Rectangle(Mx(15, 2), y + 3, 2, 2), Color.White);

        // Orange beak
        sp.Draw(px, new Rectangle(Mx(19, 6), y + 8, 6, 4), new Color(255, 140, 0));

        // Orange feet
        sp.Draw(px, new Rectangle(x + 1, y + h - 4, 10, 4), new Color(255, 140, 0));
        sp.Draw(px, new Rectangle(x + w - 11, y + h - 4, 10, 4), new Color(255, 140, 0));

        // Wing flipper
        sp.Draw(px, new Rectangle(Mx(0, 5), y + 10, 5, 15), Color.Black);
    }
}
