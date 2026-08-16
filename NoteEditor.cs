using System;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.Notes;

// Saisie live d'une note. La recette vient du chat de CelesteNet : les caractères arrivent
// par TextInput.OnInput (déjà résolus par le layout clavier, donc AZERTY et accents gratuits),
// le jeu est mis en pause pendant la frappe, et les VirtualButton du jeu sont consommés
// pendant la saisie et quelques frames après pour que la touche de validation ne déclenche
// pas un dash ou le menu pause.
//
// Le buffer contient de vrais retours à la ligne : Entrée en insère un, Ctrl+Entrée valide.
public class NoteEditor {
    // Frames de consommation d'inputs après la fermeture.
    private const int ConsumeFrames = 2;
    // Colonne cible non calculée : les Up/Down consécutifs la conservent, tout le reste la jette.
    private const float NoColumn = -1f;

    public bool Active { get; private set; }
    public string Buffer { get; private set; } = "";
    public int CursorIndex { get; private set; }
    public float BlinkTimer { get; private set; }

    // Ancrage visé par la note en cours d'écriture, et celui d'où elle vient : Tab déplace
    // la note, donc valider ailleurs qu'à l'origine vide l'emplacement de départ.
    public NotesSettings.Anchors Anchor { get; private set; }
    public NotesSettings.Anchors OriginAnchor { get; private set; }

    private bool sceneWasPaused;
    private int consumeInput;
    private bool ignoreCharsThisFrame;
    private float desiredColumn = NoColumn;

    private readonly KeyRepeat left = new(Keys.Left);
    private readonly KeyRepeat right = new(Keys.Right);
    private readonly KeyRepeat up = new(Keys.Up);
    private readonly KeyRepeat down = new(Keys.Down);
    private readonly KeyRepeat delete = new(Keys.Delete);

    private static VirtualButton[] gameButtons;

    // Résolus au premier usage : Input.Initialize() a forcément tourné à ce moment-là.
    private static VirtualButton[] GameButtons => gameButtons ??= new[] {
        Input.Jump, Input.Dash, Input.Grab, Input.Talk, Input.Pause, Input.ESC,
        Input.MenuConfirm, Input.MenuCancel, Input.MenuJournal, Input.QuickRestart,
    };

    // Ligne du curseur, et sa distance au début de cette ligne en unités de police
    // (non mises à l'échelle) : le HUD s'en sert pour placer le curseur.
    public int CursorLine {
        get {
            int line = 0;
            for (int i = 0; i < CursorIndex; i++)
                if (Buffer[i] == '\n')
                    line++;
            return line;
        }
    }

    public float CursorColumn {
        get {
            int start = LineStart(CursorIndex);
            return ActiveFont.Measure(Buffer.Substring(start, CursorIndex - start)).X;
        }
    }

    public void Update() {
        if (Active) {
            // Avant orig(Level.Update) : sinon le menu pause s'ouvre avant qu'on ait consommé.
            ConsumeGameButtons();
            UpdateActive();
        } else {
            if (consumeInput > 0) {
                ConsumeGameButtons();
                consumeInput--;
            }
            if (NotesModule.Settings.Enabled && NotesModule.Settings.OpenEditor.Pressed)
                Open();
        }
        ignoreCharsThisFrame = false;
    }

    public void Open() {
        // La console debug a déjà la main sur le clavier.
        if (Active || Engine.Commands.Open)
            return;

        // On repart de la note en cours : ouvrir sert autant à éditer qu'à écrire.
        Anchor = NotesModule.Settings.Anchor;
        OriginAnchor = Anchor;
        Buffer = NotesModule.GetText(Anchor);
        CursorIndex = Buffer.Length;
        BlinkTimer = 0f;
        desiredColumn = NoColumn;
        Active = true;
        // La touche d'ouverture ne doit pas s'écrire dans le champ.
        ignoreCharsThisFrame = true;

        sceneWasPaused = Engine.Scene.Paused;
        Engine.Scene.Paused = true;
        TextInput.OnInput += OnChar;
    }

    public void Close(bool validate) {
        if (!Active)
            return;

        TextInput.OnInput -= OnChar;
        Engine.Scene.Paused = sceneWasPaused;
        Active = false;
        consumeInput = ConsumeFrames;

        if (validate) {
            // La note a voyagé : son emplacement de départ se vide.
            if (Anchor != OriginAnchor)
                NotesModule.SetText(OriginAnchor, "");
            NotesModule.SetText(Anchor, Buffer);
            // La prochaine ouverture repart d'où on vient de laisser la note.
            NotesModule.Settings.Anchor = Anchor;
        }
        Buffer = "";
        CursorIndex = 0;
    }

    private void UpdateActive() {
        float dt = Engine.RawDeltaTime;
        BlinkTimer += dt;

        bool control = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);

        if (MInput.Keyboard.Pressed(Keys.Enter)) {
            if (control) {
                Close(true);
                return;
            }
            Insert("\n");
        }
        if (MInput.Keyboard.Pressed(Keys.Escape)) {
            Close(false);
            return;
        }

        if (control && MInput.Keyboard.Pressed(Keys.V))
            Insert((TextInput.GetClipboardText() ?? "").Replace("\r\n", "\n").Replace('\r', '\n'));

        // Tab déplace le champ d'un ancrage à l'autre, la note en cours d'écriture suit.
        if (MInput.Keyboard.Pressed(Keys.Tab))
            CycleAnchor(MInput.Keyboard.Check(Keys.LeftShift) || MInput.Keyboard.Check(Keys.RightShift) ? -1 : 1);

        if (left.Check(dt) && CursorIndex > 0)
            MoveCursor(control ? PreviousWord() : CursorIndex - 1);
        else if (right.Check(dt) && CursorIndex < Buffer.Length)
            MoveCursor(control ? NextWord() : CursorIndex + 1);
        else if (up.Check(dt))
            MoveVertically(-1);
        else if (down.Check(dt))
            MoveVertically(1);
        else if (MInput.Keyboard.Pressed(Keys.Home))
            MoveCursor(control ? 0 : LineStart(CursorIndex));
        else if (MInput.Keyboard.Pressed(Keys.End))
            MoveCursor(control ? Buffer.Length : LineEnd(CursorIndex));

        if (delete.Check(dt) && CursorIndex < Buffer.Length) {
            Buffer = Buffer.Remove(CursorIndex, 1);
            BlinkTimer = 0f;
        }
    }

    private void OnChar(char c) {
        if (!Active || ignoreCharsThisFrame)
            return;

        if (c == (char)8) {
            if (CursorIndex == 0)
                return;
            // Ctrl+Backspace supprime le mot précédent, sans franchir le début de ligne.
            int target = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl)
                ? PreviousWord()
                : CursorIndex - 1;
            Buffer = Buffer.Remove(target, CursorIndex - target);
            MoveCursor(target);
            return;
        }

        // Entrée, Tab et compagnie sont traités au clavier dans UpdateActive().
        if (char.IsControl(c))
            return;

        Insert(c.ToString());
    }

    private void Insert(string text) {
        if (string.IsNullOrEmpty(text))
            return;
        Buffer = Buffer.Insert(CursorIndex, text);
        MoveCursor(CursorIndex + text.Length);
    }

    private void MoveCursor(int index) {
        CursorIndex = Calc.Clamp(index, 0, Buffer.Length);
        desiredColumn = NoColumn;
        // Curseur plein pendant qu'on édite, il ne se remet à clignoter qu'à l'arrêt.
        BlinkTimer = 0f;
    }

    // Monte ou descend d'une ligne en visant la même position horizontale qu'avant, et pas
    // le même numéro de caractère : la police est à chasse variable.
    private void MoveVertically(int direction) {
        int start = LineStart(CursorIndex);
        float column = desiredColumn >= 0f ? desiredColumn : CursorColumn;

        int targetStart, targetEnd;
        if (direction < 0) {
            if (start == 0)
                return;
            targetStart = LineStart(start - 1);
            targetEnd = start - 1;
        } else {
            int end = LineEnd(CursorIndex);
            if (end >= Buffer.Length)
                return;
            targetStart = end + 1;
            targetEnd = LineEnd(targetStart);
        }

        // La largeur du préfixe croît avec l'index : dès qu'on s'éloigne, c'est fini.
        int best = targetStart;
        float bestDistance = float.MaxValue;
        for (int i = targetStart; i <= targetEnd; i++) {
            float distance = Math.Abs(ActiveFont.Measure(Buffer.Substring(targetStart, i - targetStart)).X - column);
            if (distance >= bestDistance)
                break;
            bestDistance = distance;
            best = i;
        }

        CursorIndex = best;
        BlinkTimer = 0f;
        desiredColumn = column;
    }

    private void CycleAnchor(int direction) {
        NotesSettings.Anchors[] values = NotesSettings.AnchorValues;
        int index = Array.IndexOf(values, Anchor) + direction;
        Anchor = values[(index + values.Length) % values.Length];
        BlinkTimer = 0f;
    }

    private int LineStart(int index) {
        if (index <= 0)
            return 0;
        int newline = Buffer.LastIndexOf('\n', index - 1);
        return newline < 0 ? 0 : newline + 1;
    }

    private int LineEnd(int index) {
        int newline = Buffer.IndexOf('\n', index);
        return newline < 0 ? Buffer.Length : newline;
    }

    private int PreviousWord() {
        int start = LineStart(CursorIndex);
        int from = CursorIndex;
        if (from > start && Buffer[from - 1] == ' ')
            from--;
        int previous = from <= start ? -1 : Buffer.LastIndexOf(' ', from - 1);
        return previous < start ? start : previous + 1;
    }

    private int NextWord() {
        int end = LineEnd(CursorIndex);
        int next = Buffer.IndexOf(' ', CursorIndex);
        return next < 0 || next >= end ? end : next + 1;
    }

    private static void ConsumeGameButtons() {
        foreach (VirtualButton button in GameButtons) {
            button.ConsumeBuffer();
            button.ConsumePress();
        }
    }

    // Répétition maison pour les touches lues au clavier : SDL ne répète que la saisie texte.
    private class KeyRepeat {
        private const float InitialDelay = 0.3f;
        private const float RepeatDelay = 0.05f;

        private readonly Keys key;
        private float timer;
        private bool repeating;

        public KeyRepeat(Keys key) {
            this.key = key;
        }

        public bool Check(float dt) {
            if (!MInput.Keyboard.Check(key)) {
                timer = 0f;
                repeating = false;
                return false;
            }
            if (MInput.Keyboard.Pressed(key)) {
                timer = 0f;
                repeating = false;
                return true;
            }
            timer += dt;
            float delay = repeating ? RepeatDelay : InitialDelay;
            if (timer < delay)
                return false;
            timer -= delay;
            repeating = true;
            return true;
        }
    }
}
