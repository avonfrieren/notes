using System;

namespace Celeste.Mod.Notes;

public class NotesModule : EverestModule {
    public static NotesModule Instance { get; private set; }

    public override Type SettingsType => typeof(NotesSettings);
    public static NotesSettings Settings => (NotesSettings)Instance._Settings;

    // Volatile : jamais écrit dans les settings. Le texte survit aux changements de room
    // et aux morts, il disparaît à la fermeture du jeu.
    public static string Text { get; set; } = "";

    public NotesModule() {
        Instance = this;
    }

    public override void Load() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        Logger.Log(LogLevel.Info, "notes", "notes loaded.");
    }

    public override void Unload() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        Text = "";
    }

    private void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
        if (level.Entities.FindFirst<NoteHud>() == null)
            level.Add(new NoteHud());
    }
}
