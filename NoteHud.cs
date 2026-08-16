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

        if (!string.IsNullOrEmpty(NotesModule.Text))
            RenderBlock(NotesModule.Text, pos, justify, scale);
    }

    // La note en cours d'écriture s'affiche à son emplacement final, avec le même découpage
    // en lignes : on voit exactement ce qu'on obtiendra en validant.
    private static void RenderEditor(NoteEditor editor, Vector2 pos, Vector2 justify, float scale) {
        string[] lines = editor.Buffer.Split('\n');
        float lineHeight = ActiveFont.LineHeight * scale;
        float width = BlockWidth(lines, scale);
        float height = lines.Length * lineHeight;
        Vector2 topLeft = pos - new Vector2(width * justify.X, height * justify.Y);

        // Un champ vide garde une largeur minimale, sinon le fond se réduit à rien.
        float boxWidth = Math.Max(width, CursorWidth);
        Draw.Rect(topLeft.X - Padding, topLeft.Y - Padding,
            boxWidth + Padding * 2f, height + Padding * 2f, Color.Black * 0.6f);

        DrawLines(lines, topLeft, width, justify, scale);

        // Curseur dessiné en rectangle plutôt qu'en caractère : aucune dépendance à la police.
        if (editor.BlinkTimer % 1f < 0.5f) {
            int line = editor.CursorLine;
            float lineWidth = ActiveFont.Measure(lines[line]).X * scale;
            Draw.Rect(
                topLeft.X + (width - lineWidth) * justify.X + editor.CursorColumn * scale,
                topLeft.Y + line * lineHeight,
                CursorWidth, lineHeight, Color.White);
        }
    }

    private static void RenderBlock(string text, Vector2 pos, Vector2 justify, float scale) {
        string[] lines = text.Split('\n');
        float width = BlockWidth(lines, scale);
        float height = lines.Length * ActiveFont.LineHeight * scale;
        DrawLines(lines, pos - new Vector2(width * justify.X, height * justify.Y), width, justify, scale);
    }

    // Les lignes sont dessinées une par une plutôt qu'en un seul appel : l'éditeur et la note
    // partagent alors exactement la même géométrie, curseur compris.
    private static void DrawLines(string[] lines, Vector2 topLeft, float width, Vector2 justify, float scale) {
        float lineHeight = ActiveFont.LineHeight * scale;
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i].Length == 0)
                continue;
            float lineWidth = ActiveFont.Measure(lines[i]).X * scale;
            Vector2 at = topLeft + new Vector2((width - lineWidth) * justify.X, i * lineHeight);
            ActiveFont.DrawOutline(lines[i], at, Vector2.Zero, Vector2.One * scale,
                Color.White, 2f, Color.Black);
        }
    }

    private static float BlockWidth(string[] lines, float scale) {
        float width = 0f;
        foreach (string line in lines)
            width = Math.Max(width, ActiveFont.Measure(line).X * scale);
        return width;
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
