using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Camera
{
    private readonly Viewport _viewport;

    public Matrix Transform { get; private set; }
    public Vector2 Position { get; private set; }

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
        Transform = Matrix.Identity;
    }

    public void Follow(Vector2 target, Vector2 levelSize)
    {
        float x = MathHelper.Clamp(
            target.X - _viewport.Width / 2f,
            0,
            Math.Max(0, levelSize.X - _viewport.Width));

        float y = MathHelper.Clamp(
            target.Y - _viewport.Height / 2f,
            0,
            Math.Max(0, levelSize.Y - _viewport.Height));

        Position = new Vector2(x, y);
        Transform = Matrix.CreateTranslation(-x, -y, 0);
    }
}
