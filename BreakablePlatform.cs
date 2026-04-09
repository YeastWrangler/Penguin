using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenguinPlatformer;

public class BreakablePlatform
{
    public Rectangle Bounds { get; }
    public bool IsBroken { get; private set; }

    private float _standTime = 0f;
    private const float BreakTime = 3f;

    // 0 = intact, 1 = broken
    public float CrackProgress => MathHelper.Clamp(_standTime / BreakTime, 0f, 1f);

    public BreakablePlatform(int x, int y, int width, int height)
    {
        Bounds = new Rectangle(x, y, width, height);
    }

    // Call every frame. playerOn = true if the penguin's feet are on this platform.
    public void Update(GameTime gameTime, bool playerOn)
    {
        if (IsBroken) return;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (playerOn)
        {
            _standTime += dt;
            if (_standTime >= BreakTime)
                IsBroken = true;
        }
    }

    public void Reset()
    {
        IsBroken = false;
        _standTime = 0f;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (IsBroken) return;

        float crack = CrackProgress;

        // Shake offset increases as platform gets close to breaking
        int shakeX = crack > 0.55f
            ? (int)(MathF.Sin(_standTime * 28f) * (crack - 0.55f) * 14f)
            : 0;

        int x = Bounds.X + shakeX;
        int y = Bounds.Y;
        int w = Bounds.Width;
        int h = Bounds.Height;

        // Base colour shifts from icy blue → cracked grey-orange
        var baseColor = Color.Lerp(
            new Color(140, 210, 240),   // intact: bright ice blue
            new Color(180, 140, 100),   // broken: dusty orange-grey
            crack);

        spriteBatch.Draw(pixel, new Rectangle(x, y, w, h), baseColor);

        // Snow / shine on top (fades with cracks)
        spriteBatch.Draw(pixel, new Rectangle(x, y, w, 4), Color.White * (1f - crack * 1.2f));

        // Crack lines — each one appears at a threshold and darkens with progression
        var crackColor = Color.Lerp(new Color(60, 100, 130), new Color(80, 50, 30), crack);

        if (crack > 0.15f)
        {
            // First crack: diagonal from left-centre toward right
            spriteBatch.Draw(pixel, new Rectangle(x + w / 5, y + 3, w * 3 / 5, 2), crackColor);
            spriteBatch.Draw(pixel, new Rectangle(x + w / 5, y + 4, w / 3, 2), crackColor);
        }
        if (crack > 0.30f)
        {
            // Second crack: opposite angle
            spriteBatch.Draw(pixel, new Rectangle(x + w * 2 / 5, y + 2, w / 3, 2), crackColor * 0.9f);
            spriteBatch.Draw(pixel, new Rectangle(x + w * 2 / 5 + 4, y + 8, w / 4, 2), crackColor);
        }
        if (crack > 0.50f)
        {
            // Third crack: short vertical
            spriteBatch.Draw(pixel, new Rectangle(x + w / 3, y + 1, 2, h - 2), crackColor);
            spriteBatch.Draw(pixel, new Rectangle(x + w * 2 / 3, y + 3, 2, h - 4), crackColor * 0.8f);
        }
        if (crack > 0.70f)
        {
            // Heavy cracking: more lines + darkening overlay
            spriteBatch.Draw(pixel, new Rectangle(x + w / 6, y + 6, w * 2 / 3, 2), crackColor);
            spriteBatch.Draw(pixel, new Rectangle(x + w / 4, y, 2, h), crackColor);
            spriteBatch.Draw(pixel, new Rectangle(x + w * 3 / 4, y, 2, h), crackColor);

            // Danger overlay (reddish tint)
            spriteBatch.Draw(pixel, new Rectangle(x, y, w, h),
                Color.OrangeRed * ((crack - 0.70f) * 1.5f));
        }
        if (crack > 0.88f)
        {
            // Almost breaking: dark grey overlay, very unstable looking
            spriteBatch.Draw(pixel, new Rectangle(x, y, w, h), Color.DarkGray * 0.45f);

            // Warning flash
            float flash = (MathF.Sin(_standTime * 20f) + 1f) / 2f;
            spriteBatch.Draw(pixel, new Rectangle(x, y, w, h), Color.Red * (flash * 0.35f));
        }

        // Small "fragile" triangle markers at corners (visible on intact platforms)
        if (crack < 0.3f)
        {
            spriteBatch.Draw(pixel, new Rectangle(x + 2,     y + 2, 4, 4), new Color(255, 220, 100, 160));
            spriteBatch.Draw(pixel, new Rectangle(x + w - 6, y + 2, 4, 4), new Color(255, 220, 100, 160));
        }
    }
}
