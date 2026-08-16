using System;

namespace Celeste.Mod.Notes;

public class NotesModule : EverestModule {
    public static NotesModule Instance { get; private set; }

    public override Type SettingsType => typeof(NotesSettings);
    public static NotesSettings Settings => (NotesSettings)Instance._Settings;

    // Une note par ancrage, à emplacement fixe : effacer une note n'en fait glisser aucune
    // autre. Volatile, comme tout le reste : jamais écrit dans les settings, le contenu
    // disparaît à la fermeture du jeu.
    private static readonly string[] texts = new string[NotesSettings.AnchorValues.Length];

    public NoteEditor Editor { get; } = new();

    public NotesModule() {
        Instance = this;
    }

    public static string GetText(NotesSettings.Anchors anchor) => texts[(int)anchor] ?? "";

    public static void SetText(NotesSettings.Anchors anchor, string text) {
        texts[(int)anchor] = text ?? "";
    }

    public static void ClearAll() {
        Array.Clear(texts);
    }

    public override void Load() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        On.Celeste.Level.Update += OnLevelUpdate;
        Logger.Log(LogLevel.Info, "notes", "notes loaded.");
    }

    public override void Unload() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        On.Celeste.Level.Update -= OnLevelUpdate;
        Editor.Close(false);
        ClearAll();
    }

    private void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        // Un changement de map ne doit pas laisser l'éditeur ouvert sur une scène morte.
        if (isFromLoader)
            Editor.Close(false);
        if (level.Entities.FindFirst<NoteHud>() == null)
            level.Add(new NoteHud());
    }

    // L'éditeur passe avant orig() : il doit avoir consommé Input.Pause et Input.ESC
    // avant que Level.Update ne les lise et n'ouvre le menu pause.
    private void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        Editor.Update();
        orig(self);
    }
}
