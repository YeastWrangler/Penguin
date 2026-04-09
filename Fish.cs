using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Fish
{
    private readonly Vector2 _position;
    private float _bobTime = 0f;
    private float _glowPulse = 0f;

    public bool IsCollected { get; private set; }

    // Hitbox sits at the bobbing position
    private const int W = 34;
    private const int H = 22;

    public Rectangle Bounds => new Rectangle(
        (int)_position.X,
        (int)(_position.Y + MathF.Sin(_bobTime) * 5f),
        W, H);

    public Fish(Vector2 position) => _position = position;

    public void Update(GameTime gameTime)
    {
        if (IsCollected) return;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _bobTime += dt * 2.2f;
        _glowPulse += dt * 4f;
    }

    public void Collect() => IsCollected = true;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (IsCollected) return;

        int x = (int)_position.X;
        int y = (int)(_position.Y + MathF.Sin(_bobTime) * 5f);

        float glow = (MathF.Sin(_glowPulse) + 1f) / 2f;

        // Outer glow ring (gold aura)
        spriteBatch.Draw(pixel, new Rectangle(x - 4, y - 4, W + 8, H + 8),
            Color.Gold * (0.25f + glow * 0.2f));

        // Tail fins (two triangular pieces)
        spriteBatch.Draw(pixel, new Rectangle(x, y, 10, 8), new Color(70, 150, 200));
        spriteBatch.Draw(pixel, new Rectangle(x, y + 14, 10, 8), new Color(70, 150, 200));
        spriteBatch.Draw(pixel, new Rectangle(x + 2, y + 4, 7, 14), new Color(70, 150, 200)); // join

        // Main body
        spriteBatch.Draw(pixel, new Rectangle(x + 8, y + 4, 22, 14), new Color(110, 190, 230));

        // Body highlight (top shine)
        spriteBatch.Draw(pixel, new Rectangle(x + 10, y + 5, 16, 4), new Color(160, 220, 255));

        // Scale pattern
        spriteBatch.Draw(pixel, new Rectangle(x + 12, y + 6, 5, 5), new Color(130, 200, 240));
        spriteBatch.Draw(pixel, new Rectangle(x + 19, y + 6, 5, 5), new Color(130, 200, 240));
        spriteBatch.Draw(pixel, new Rectangle(x + 15, y + 11, 5, 5), new Color(130, 200, 240));

        // Head (darker)
        spriteBatch.Draw(pixel, new Rectangle(x + 24, y + 3, 10, 16), new Color(70, 140, 185));

        // Dorsal fin (top)
        spriteBatch.Draw(pixel, new Rectangle(x + 14, y, 12, 5), new Color(80, 160, 210));

        // Eye
        spriteBatch.Draw(pixel, new Rectangle(x + 27, y + 6, 5, 5), Color.Black);
        // Eye shine
        spriteBatch.Draw(pixel, new Rectangle(x + 28, y + 7, 2, 2), Color.White);

        // Mouth
        spriteBatch.Draw(pixel, new Rectangle(x + 32, y + 12, 2, 3), new Color(50, 100, 150));

        // Gold star sparkles around fish (power-up indicator)
        var sparkle = Color.Gold * (0.6f + glow * 0.4f);
        spriteBatch.Draw(pixel, new Rectangle(x - 6, y + H / 2 - 2, 4, 4), sparkle);
        spriteBatch.Draw(pixel, new Rectangle(x + W + 2, y + H / 2 - 2, 4, 4), sparkle);
        spriteBatch.Draw(pixel, new Rectangle(x + W / 2 - 2, y - 6, 4, 4), sparkle);
        spriteBatch.Draw(pixel, new Rectangle(x + W / 2 - 2, y + H + 2, 4, 4), sparkle);
    }
}
