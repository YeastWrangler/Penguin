using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenguinPlatformer;

public class Player
{
    private Vector2 _position;
    private Vector2 _velocity;
    private int _jumpsLeft;
    private bool _facingRight = true;
    private KeyboardState _prevKeyboard;

    private const float Gravity = 1400f;
    private const float MoveSpeed = 300f;
    private const float JumpForce = -620f;
    public const int Width = 36;
    public const int Height = 46;
    private const int MaxJumps = 2;
    private const float LevelWidth = 4000f;

    public Vector2 Position => _position;
    public Vector2 Velocity => _velocity;
    public bool IsAlive { get; private set; } = true;
    public Rectangle Bounds => new Rectangle((int)_position.X, (int)_position.Y, Width, Height);

    public Player(Vector2 startPosition)
    {
        _position = startPosition;
        _jumpsLeft = MaxJumps;
        IsAlive = true;
    }

    public void Update(GameTime gameTime, KeyboardState keyboard, List<Platform> platforms)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

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

        // Move X, resolve horizontal collisions
        _position.X += _velocity.X * dt;
        _position.X = MathHelper.Clamp(_position.X, 0, LevelWidth - Width);

        foreach (var platform in platforms)
        {
            if (!Bounds.Intersects(platform.Bounds)) continue;
            if (_velocity.X > 0)
                _position.X = platform.Bounds.Left - Width;
            else if (_velocity.X < 0)
                _position.X = platform.Bounds.Right;
            _velocity.X = 0;
        }

        // Move Y, resolve vertical collisions
        _position.Y += _velocity.Y * dt;

        foreach (var platform in platforms)
        {
            if (!Bounds.Intersects(platform.Bounds)) continue;

            if (_velocity.Y >= 0)
            {
                _position.Y = platform.Bounds.Top - Height;
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

    public void BounceOff()
    {
        _velocity.Y = JumpForce * 0.55f;
        _jumpsLeft = MaxJumps;
    }

    public void Die() => IsAlive = false;

    public void Reset(Vector2 position)
    {
        _position = position;
        _velocity = Vector2.Zero;
        IsAlive = true;
        _jumpsLeft = MaxJumps;
        _facingRight = true;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int x = (int)_position.X;
        int y = (int)_position.Y;

        // When facing left, mirror all X offsets: mirrorX = x + Width - offsetX - rectWidth
        int Mx(int offsetX, int rectW) => _facingRight ? x + offsetX : x + Width - offsetX - rectW;

        // Black tuxedo body
        spriteBatch.Draw(pixel, new Rectangle(x, y, Width, Height), Color.Black);

        // White belly
        spriteBatch.Draw(pixel, new Rectangle(Mx(8, 20), y + 16, 20, 28), Color.White);

        // White face / head
        spriteBatch.Draw(pixel, new Rectangle(Mx(5, 26), y + 1, 26, 20), Color.White);

        // Black eye patches (slightly asymmetric — bigger on one side for character)
        spriteBatch.Draw(pixel, new Rectangle(Mx(6, 6), y + 3, 6, 7), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(Mx(22, 6), y + 3, 6, 7), Color.Black);

        // White eye shine
        spriteBatch.Draw(pixel, new Rectangle(Mx(7, 3), y + 4, 3, 3), Color.White);
        spriteBatch.Draw(pixel, new Rectangle(Mx(23, 3), y + 4, 3, 3), Color.White);

        // Orange beak (faces direction of travel)
        var beakColor = new Color(255, 140, 0);
        spriteBatch.Draw(pixel, new Rectangle(Mx(24, 8), y + 11, 8, 5), beakColor);
        // Beak tip
        spriteBatch.Draw(pixel, new Rectangle(Mx(28, 4), y + 13, 4, 3), new Color(220, 110, 0));

        // Orange feet
        var feetColor = new Color(255, 140, 0);
        spriteBatch.Draw(pixel, new Rectangle(x + 2, y + Height - 5, 14, 5), feetColor);
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 16, y + Height - 5, 14, 5), feetColor);

        // Tuxedo wing / flipper
        spriteBatch.Draw(pixel, new Rectangle(Mx(0, 8), y + 14, 8, 22), Color.Black);
    }
}
