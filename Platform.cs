using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Platform
{
    public Rectangle Bounds { get; }
    private readonly Color _color;

    public Platform(int x, int y, int width, int height, Color color)
    {
        Bounds = new Rectangle(x, y, width, height);
        _color = color;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // Main body
        spriteBatch.Draw(pixel, Bounds, _color);

        // Snow cap on top
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 5), Color.White);

        // Slight darker bottom edge for depth
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - 4, Bounds.Width, 4),
            new Color(_color.R - 30, _color.G - 30, _color.B - 20));

        // Ice shine marks
        for (int i = 0; i < Bounds.Width; i += 40)
        {
            spriteBatch.Draw(pixel, new Rectangle(Bounds.X + i + 8, Bounds.Y + 8, 14, 4),
                Color.White * 0.4f);
        }
    }
}
