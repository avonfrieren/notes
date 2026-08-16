using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Notes;

public class NoteHud : Entity {
    // Coordonnées HUD (1920×1080), marge sur les bords de l'écran.
    private const float Margin = 32f;
    // Marge intérieure du fond de l'éditeur.
    private const float Padding = 8f;
    private const float CursorWidth = 2f;

    public NoteHud() {
        Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        Depth = -100;
    }

    public override void Render() {
        NotesSettings settings = NotesModule.Settings;
        if (settings == null || !settings.Enabled || Scene is not Level)
            return;

        float scale = settings.Scale * 0.1f;
        (Vector2 pos, Vector2 justify) = Layout(settings.Anchor);

        NoteEditor editor = NotesModule.Instance.Editor;
        if (editor.Active) {
            RenderEditor(editor, pos, justify, scale);
            return;
        }

        string text = NotesModule.Text;
        if (string.IsNullOrEmpty(text))
            return;
        // PixelFont gère les \n au dessin comme à la mesure : le multi-ligne est gratuit.
        ActiveFont.DrawOutline(text, pos, justify, Vector2.One * scale,
            Color.White, 2f, Color.Black);
    }

    // La note en cours d'écriture s'affiche à son emplacement final : on voit exactement
    // ce qu'on obtiendra en validant.
    private static void RenderEditor(NoteEditor editor, Vector2 pos, Vector2 justify, float scale) {
        string text = editor.Buffer;
        float height = ActiveFont.LineHeight * scale;
        // Un champ vide garde une largeur minimale, sinon le fond et le curseur disparaissent.
        float width = Math.Max(ActiveFont.Measure(text).X * scale, CursorWidth);
        Vector2 topLeft = pos - new Vector2(width * justify.X, height * justify.Y);

        Draw.Rect(topLeft.X - Padding, topLeft.Y - Padding,
            width + Padding * 2f, height + Padding * 2f, Color.Black * 0.6f);

        if (text.Length > 0)
            ActiveFont.DrawOutline(text, topLeft, Vector2.Zero, Vector2.One * scale,
                Color.White, 2f, Color.Black);

        // Curseur dessiné en rectangle plutôt qu'en caractère : aucune dépendance à la police.
        if (editor.BlinkTimer % 1f < 0.5f) {
            float offset = ActiveFont.Measure(text.Substring(0, editor.CursorIndex)).X * scale;
            Draw.Rect(topLeft.X + offset, topLeft.Y, CursorWidth, height, Color.White);
        }
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
