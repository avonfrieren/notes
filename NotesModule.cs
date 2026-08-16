using System;

namespace Celeste.Mod.Notes;

public class NotesModule : EverestModule {
    public static NotesModule Instance { get; private set; }

    public override Type SettingsType => typeof(NotesSettings);
    public static NotesSettings Settings => (NotesSettings)Instance._Settings;

    // Volatile : jamais écrit dans les settings. Le texte survit aux changements de room
    // et aux morts, il disparaît à la fermeture du jeu.
    public static string Text { get; set; } = "";

    public NoteEditor Editor { get; } = new();

    public NotesModule() {
        Instance = this;
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
        Text = "";
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
