using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Notes;

public class NoteHud : Entity {
    // Coordonnées HUD (1920×1080), marge sur les bords de l'écran.
    private const float Margin = 32f;

    public NoteHud() {
        Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        Depth = -100;
    }

    public override void Render() {
        NotesSettings settings = NotesModule.Settings;
        if (settings == null || !settings.Enabled)
            return;
        string text = NotesModule.Text;
        if (string.IsNullOrEmpty(text) || Scene is not Level)
            return;

        (Vector2 pos, Vector2 justify) = Layout(settings.Anchor);
        // PixelFont gère les \n au dessin comme à la mesure : le multi-ligne est gratuit.
        ActiveFont.DrawOutline(text, pos, justify, Vector2.One * (settings.Scale * 0.1f),
            Color.White, 2f, Color.Black);
    }

    private static (Vector2 Pos, Vector2 Justify) Layout(NotesSettings.Anchors anchor) => anchor switch {
        NotesSettings.Anchors.TopCenter => (new Vector2(1920f / 2f, Margin), new Vector2(0.5f, 0f)),
        NotesSettings.Anchors.TopRight => (new Vector2(1920f - Margin, Margin), new Vector2(1f, 0f)),
        NotesSettings.Anchors.Center => (new Vector2(1920f / 2f, 1080f / 2f), new Vector2(0.5f, 0.5f)),
        NotesSettings.Anchors.BottomLeft => (new Vector2(Margin, 1080f - Margin), new Vector2(0f, 1f)),
        NotesSettings.Anchors.BottomCenter => (new Vector2(1920f / 2f, 1080f - Margin), new Vector2(0.5f, 1f)),
        NotesSettings.Anchors.BottomRight => (new Vector2(1920f - Margin, 1080f - Margin), new Vector2(1f, 1f)),
        _ => (new Vector2(Margin, Margin), Vector2.Zero),
    };
}
