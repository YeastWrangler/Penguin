using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Enemy
{
    private Vector2 _position;
    private float _direction;
    private readonly float _leftBound;
    private readonly float _rightBound;
    private const float Speed = 90f;
    public const int Width = 52;
    public const int Height = 40;

    public bool IsAlive { get; private set; } = true;
    public Rectangle Bounds => new Rectangle((int)_position.X, (int)_position.Y, Width, Height);

    public Enemy(Vector2 position, float leftBound, float rightBound)
    {
        _position = position;
        _leftBound = leftBound;
        _rightBound = rightBound;
        _direction = 1f;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _position.X += _direction * Speed * dt;

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

    public void Kill() => IsAlive = false;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsAlive) return;

        int x = (int)_position.X;
        int y = (int)_position.Y;

        var bodyColor = new Color(139, 119, 101);
        var faceColor = new Color(180, 160, 135);
        var snoutColor = new Color(200, 175, 150);

        // Main body
        spriteBatch.Draw(pixel, new Rectangle(x, y + 10, Width, Height - 10), bodyColor);

        // Head
        spriteBatch.Draw(pixel, new Rectangle(x + 6, y, Width - 12, 22), bodyColor);

        // Face / lighter area
        spriteBatch.Draw(pixel, new Rectangle(x + 9, y + 2, Width - 18, 18), faceColor);

        // Snout
        spriteBatch.Draw(pixel, new Rectangle(x + 11, y + 12, Width - 22, 9), snoutColor);

        // Nostrils
        spriteBatch.Draw(pixel, new Rectangle(x + 14, y + 13, 4, 3), new Color(100, 80, 70));
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 18, y + 13, 4, 3), new Color(100, 80, 70));

        // Eyes
        spriteBatch.Draw(pixel, new Rectangle(x + 10, y + 4, 7, 7), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 17, y + 4, 7, 7), Color.Black);
        // Eye whites
        spriteBatch.Draw(pixel, new Rectangle(x + 12, y + 5, 3, 3), Color.White);
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 15, y + 5, 3, 3), Color.White);

        // Tusks
        spriteBatch.Draw(pixel, new Rectangle(x + 13, y + 20, 7, 14), Color.Ivory);
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 20, y + 20, 7, 14), Color.Ivory);
        // Tusk tips
        spriteBatch.Draw(pixel, new Rectangle(x + 14, y + 32, 5, 3), new Color(230, 225, 200));
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 19, y + 32, 5, 3), new Color(230, 225, 200));

        // Flippers / arms
        spriteBatch.Draw(pixel, new Rectangle(x - 6, y + 12, 8, 16), new Color(120, 100, 85));
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 2, y + 12, 8, 16), new Color(120, 100, 85));

        // Feet / base
        spriteBatch.Draw(pixel, new Rectangle(x + 4, y + Height - 6, 14, 6), new Color(120, 100, 85));
        spriteBatch.Draw(pixel, new Rectangle(x + Width - 18, y + Height - 6, 14, 6), new Color(120, 100, 85));

        // Direction indicator: darker side facing movement direction
        if (_direction < 0)
            spriteBatch.Draw(pixel, new Rectangle(x + 9, y + 4, 3, 3), new Color(0, 0, 0, 80));
        else
            spriteBatch.Draw(pixel, new Rectangle(x + Width - 12, y + 4, 3, 3), new Color(0, 0, 0, 80));
    }
}
