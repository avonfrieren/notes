using System;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.Notes;

// Saisie live d'une note. La recette vient du chat de CelesteNet : les caractères arrivent
// par TextInput.OnInput (déjà résolus par le layout clavier, donc AZERTY et accents gratuits),
// le jeu est mis en pause pendant la frappe, et les VirtualButton du jeu sont consommés
// pendant la saisie et quelques frames après pour que la touche de validation ne déclenche
// pas un dash ou le menu pause.
public class NoteEditor {
    // Frames de consommation d'inputs après la fermeture.
    private const int ConsumeFrames = 2;

    public bool Active { get; private set; }
    public string Buffer { get; private set; } = "";
    public int CursorIndex { get; private set; }
    public float BlinkTimer { get; private set; }

    private bool sceneWasPaused;
    private int consumeInput;
    private bool ignoreCharsThisFrame;

    private readonly KeyRepeat left = new(Keys.Left);
    private readonly KeyRepeat right = new(Keys.Right);
    private readonly KeyRepeat delete = new(Keys.Delete);

    private static VirtualButton[] gameButtons;

    // Résolus au premier usage : Input.Initialize() a forcément tourné à ce moment-là.
    private static VirtualButton[] GameButtons => gameButtons ??= new[] {
        Input.Jump, Input.Dash, Input.Grab, Input.Talk, Input.Pause, Input.ESC,
        Input.MenuConfirm, Input.MenuCancel, Input.MenuJournal, Input.QuickRestart,
    };

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
        Buffer = NotesModule.Text.Replace("\n", "\\n");
        CursorIndex = Buffer.Length;
        BlinkTimer = 0f;
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

        if (validate)
            NotesModule.Text = Buffer.Replace("\\n", "\n");
        Buffer = "";
        CursorIndex = 0;
    }

    private void UpdateActive() {
        float dt = Engine.RawDeltaTime;
        BlinkTimer += dt;

        if (MInput.Keyboard.Pressed(Keys.Enter)) {
            Close(true);
            return;
        }
        if (MInput.Keyboard.Pressed(Keys.Escape)) {
            Close(false);
            return;
        }

        bool control = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);

        if (control && MInput.Keyboard.Pressed(Keys.V)) {
            string pasted = TextInput.GetClipboardText() ?? "";
            // Un collé multi-ligne devient des \n littéraux : le champ reste sur une ligne.
            Insert(pasted.Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n"));
        }

        if (left.Check(dt) && CursorIndex > 0)
            MoveCursor(control ? PreviousWord() : CursorIndex - 1);
        else if (right.Check(dt) && CursorIndex < Buffer.Length)
            MoveCursor(control ? NextWord() : CursorIndex + 1);
        else if (MInput.Keyboard.Pressed(Keys.Home))
            MoveCursor(0);
        else if (MInput.Keyboard.Pressed(Keys.End))
            MoveCursor(Buffer.Length);

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
            // Ctrl+Backspace supprime le mot précédent.
            int target = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl)
                ? PreviousWord()
                : CursorIndex - 1;
            Buffer = Buffer.Remove(target, CursorIndex - target);
            MoveCursor(target);
            return;
        }

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
        // Curseur plein pendant qu'on édite, il ne se remet à clignoter qu'à l'arrêt.
        BlinkTimer = 0f;
    }

    private int PreviousWord() {
        int from = CursorIndex;
        if (from > 0 && Buffer[from - 1] == ' ')
            from--;
        int previous = Buffer.LastIndexOf(' ', Math.Max(from - 1, 0));
        return previous < 0 ? 0 : previous + 1;
    }

    private int NextWord() {
        int next = Buffer.IndexOf(' ', CursorIndex);
        return next < 0 ? Buffer.Length : next + 1;
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
