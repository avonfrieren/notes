# notes

[Everest](https://everestapi.github.io/) mod for [Celeste](https://www.celestegame.com/) that
displays any text you want on screen.

## Changelog

### v0.4.2

* Cycling back through the anchors moved from `Shift+Tab` to **`Ctrl+Tab`**, since `Shift+Tab`
  belongs to the Steam overlay

### v0.4.1

* Two more anchors, **Center Left** and **Center Right**, which brings the grid to a full 3×3

### v0.4.0

* **Several notes at once**: one per anchor, each at a fixed position, so clearing one never
  moves another
* `Tab` / `Shift+Tab` while writing carries the note from one anchor to the next
* The field opens on the anchor you last wrote to

### v0.3.0

* The field is now a real **multi-line text area**: `Enter` breaks the line, `Ctrl+Enter`
  validates
* `↑` / `↓` move between lines and keep your horizontal position on screen, not your
  character number
* `Home` / `End` work per line, `Ctrl` extends them to the whole note
* Pasting a multi-line clipboard keeps its line breaks

### v0.2.0

* **Live typing**: press `T` in a level to open a field and write your note directly, with
  cursor movement, word deletion and clipboard paste
* Reopening the field prefills it with the current note
* The field renders at the note's final position, so you see what you are going to get
* Accents and any character your keyboard layout produces are supported, unlike the console

### v0.1.0

* **Initial release**
* `note` console command, HUD display, anchor and scale options
