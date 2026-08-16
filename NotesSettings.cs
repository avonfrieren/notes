namespace Celeste.Mod.Notes;

// La valeur par défaut de Scale est rappelée sous l'entrée via [SettingSubText] :
// en changer une ici implique de mettre à jour la ligne *_SUB correspondante dans
// Dialog/English.txt et Dialog/French.txt.
public class NotesSettings : EverestModuleSettings {
    public enum Anchors { TopLeft, TopCenter, TopRight, Center, BottomLeft, BottomCenter, BottomRight }

    public bool Enabled { get; set; } = true;

    public Anchors Anchor { get; set; } = Anchors.TopLeft;

    // Taille du texte, en dixièmes : 8 = ×0.8, la taille du HUD de tsp.
    [SettingRange(5, 20, true)]
    [SettingSubText("MODOPTIONS_NOTES_SCALE_SUB")]
    public int Scale { get; set; } = 8;
}
