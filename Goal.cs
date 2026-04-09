using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class Goal
{
    private readonly Vector2 _base; // ground-level position
    private float _time = 0f;

    private const int PoleH = 90;
    private const int StarCX = 18; // star center X offset from _base.X
    private const int StarCY = 110; // star center height above _base.Y

    // Trigger: the pole + star column
    public Rectangle TriggerBounds => new Rectangle(
        (int)_base.X + StarCX - 18,
        (int)_base.Y - PoleH - 20,
        42,
        PoleH + 20);

    public Goal(Vector2 groundPosition) => _base = groundPosition;

    public void Update(GameTime gameTime)
    {
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int bx = (int)_base.X;
        int by = (int)_base.Y;

        // Base block
        spriteBatch.Draw(pixel, new Rectangle(bx + 4, by - 8, 28, 8), new Color(160, 130, 40));
        spriteBatch.Draw(pixel, new Rectangle(bx + 6, by - 6, 24, 4), new Color(220, 180, 60));

        // Pole
        spriteBatch.Draw(pixel, new Rectangle(bx + StarCX - 4, by - PoleH, 8, PoleH), new Color(200, 165, 50));
        // Pole highlight
        spriteBatch.Draw(pixel, new Rectangle(bx + StarCX - 3, by - PoleH, 2, PoleH), new Color(240, 210, 100));

        float pulse = (MathF.Sin(_time * 2.8f) + 1f) / 2f;
        var outerStar = Color.Lerp(Color.Gold, Color.Yellow, pulse);
        var innerStar = Color.Lerp(new Color(255, 240, 120), Color.White, pulse * 0.6f);

        // Star drawn as overlapping cross bars + corner points
        int cx = bx + StarCX;
        int cy = by - StarCY;

        // Glow backdrop
        spriteBatch.Draw(pixel, new Rectangle(cx - 22, cy - 22, 44, 44),
            Color.Gold * (0.15f + pulse * 0.2f));

        // Wide horizontal bar
        spriteBatch.Draw(pixel, new Rectangle(cx - 18, cy - 6, 36, 12), outerStar);
        // Tall vertical bar
        spriteBatch.Draw(pixel, new Rectangle(cx - 6, cy - 18, 12, 36), outerStar);
        // Inner diamond corners
        spriteBatch.Draw(pixel, new Rectangle(cx - 12, cy - 12, 24, 24), outerStar);
        // Star center
        spriteBatch.Draw(pixel, new Rectangle(cx - 7, cy - 7, 14, 14), innerStar);
        // Bright hot center
        spriteBatch.Draw(pixel, new Rectangle(cx - 3, cy - 3, 6, 6), Color.White);

        // Spinning sparkles around the star
        for (int i = 0; i < 4; i++)
        {
            float angle = _time * 2f + i * MathF.PI / 2f;
            int sx = cx + (int)(MathF.Cos(angle) * 22);
            int sy = cy + (int)(MathF.Sin(angle) * 22);
            spriteBatch.Draw(pixel, new Rectangle(sx - 2, sy - 2, 5, 5), Color.Gold * (0.5f + pulse * 0.5f));
        }

        // "FINISH" banner on pole
        spriteBatch.Draw(pixel, new Rectangle(bx - 2, by - PoleH + 10, 40, 18), new Color(200, 50, 50));
        spriteBatch.Draw(pixel, new Rectangle(bx, by - PoleH + 12, 36, 14), new Color(240, 80, 80));
        // Three white stripes to suggest text
        for (int i = 0; i < 3; i++)
            spriteBatch.Draw(pixel, new Rectangle(bx + 4, by - PoleH + 14 + i * 4, 28, 2), Color.White * 0.8f);
    }
}
