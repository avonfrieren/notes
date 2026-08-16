using System.Linq;
using Monocle;

namespace Celeste.Mod.Notes;

public static class NoteCommands {
    // Le parseur de Monocle découpe la ligne sur les espaces et les virgules, ignore les
    // guillemets et n'accepte que string/int/float/bool : la seule façon de recevoir une
    // phrase est d'aligner des paramètres optionnels puis de les rejoindre. Deux
    // conséquences assumées en v0.1 : les virgules tapées disparaissent, et les espaces
    // consécutifs sont écrasés.
    [Command("note", "display a note on the HUD, or clear it when called without arguments")]
    public static void Note(
        string w1 = null, string w2 = null, string w3 = null, string w4 = null,
        string w5 = null, string w6 = null, string w7 = null, string w8 = null,
        string w9 = null, string w10 = null, string w11 = null, string w12 = null,
        string w13 = null, string w14 = null, string w15 = null, string w16 = null) {
        string[] words = {
            w1, w2, w3, w4, w5, w6, w7, w8, w9, w10, w11, w12, w13, w14, w15, w16,
        };
        // Le "\n" littéral est la seule façon de saisir un retour à la ligne dans la console.
        string text = string.Join(" ", words.Where(word => !string.IsNullOrEmpty(word)))
            .Replace("\\n", "\n");

        NotesModule.Text = text;
        Engine.Commands.Log(text.Length == 0 ? "note cleared" : $"note: {text}");
    }
}
