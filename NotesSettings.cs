using System;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Notes;

// La valeur par défaut de Scale est rappelée sous l'entrée via [SettingSubText] :
// en changer une ici implique de mettre à jour la ligne *_SUB correspondante dans
// Dialog/English.txt et Dialog/French.txt.
public class NotesSettings : EverestModuleSettings {
    // L'ordre compte : c'est celui du cycle Tab dans l'éditeur, dans le sens de la lecture.
    public enum Anchors { TopLeft, TopCenter, TopRight, Center, BottomLeft, BottomCenter, BottomRight }

    public static readonly Anchors[] AnchorValues = (Anchors[])Enum.GetValues(typeof(Anchors));

    public bool Enabled { get; set; } = true;

    // Ancrage sur lequel l'éditeur s'ouvre ; Tab en change pendant l'écriture.
    [SettingSubText("MODOPTIONS_NOTES_ANCHOR_SUB")]
    public Anchors Anchor { get; set; } = Anchors.TopLeft;

    // Taille du texte, en dixièmes : 8 = ×0.8, la taille du HUD de tsp.
    [SettingRange(5, 20, true)]
    [SettingSubText("MODOPTIONS_NOTES_SCALE_SUB")]
    public int Scale { get; set; } = 8;

    // T existe sur AZERTY comme sur QWERTY, contrairement aux touches OEM.
    [DefaultButtonBinding(0, Keys.T)]
    public ButtonBinding OpenEditor { get; set; }
}
