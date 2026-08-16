# notes

[Everest](https://everestapi.github.io/) mod for [Celeste](https://www.celestegame.com/) that
displays any text you want on screen.

No helper to install, no map to edit, no entity to place: you press a key, you type your text,
it shows up on the HUD. Useful for run notes, route reminders, TODOs while mapping, or anything
you want on screen while you play.

## Usage

Press **T** in a level (rebindable in the mod options). The game pauses, a text area opens
right where the note will live, and you type. `Ctrl+Enter` validates, `Escape` cancels.

Reopening the field gives you the current note back, so you can edit it instead of retyping
it. Validating an empty field clears the note.

| Key | |
|---|---|
| `Enter` | new line |
| `Ctrl+Enter` | validate |
| `Escape` | cancel |
| `←` `→` `↑` `↓` | move the cursor (`Ctrl` moves by word) |
| `Home` / `End` | start / end of line (`Ctrl` for the whole note) |
| `Backspace` / `Delete` | delete (`Ctrl+Backspace` deletes a word) |
| `Ctrl+V` | paste, line breaks included |
| `Tab` / `Ctrl+Tab` | move the note to the next / previous anchor |

Moving up and down keeps your horizontal position on screen rather than your character
number, since the game font is not monospaced.

Notes are **volatile**: they are never saved, they disappear when you close the game.

### Several notes at once

Every anchor holds its own note, and there are nine of them — the three corners and the middle
of each edge, plus the center of the screen. Positions are fixed: clearing a note never makes
another one slide somewhere else.

`Tab` (and `Ctrl+Tab` to go backwards) while writing carries the note to the next anchor and
drops it there when you validate,
which is also how you move a note you already wrote. The field opens on the anchor you last
wrote to. Validating on an anchor that already holds a note replaces it.

### Console command

The debug console (`.` by default) also works, which is handy for scripting:

```
note remember to grab the berry
```

It writes to the current anchor, the one the field would open on. Called without arguments, it
clears that note. A literal `\n` becomes a line break, since the console cannot take a real
one:

```
note first line\nsecond line
```

Beware that the console parser of the engine splits on spaces *and* commas and knows nothing
about quotes, so commas you type are dropped, consecutive spaces collapse into one, and a note
is capped at 16 words. The in-game field has none of these limitations.

## Options

* **Enabled** — show or hide the note without clearing it
* **Anchor** — where the field opens; `Tab` moves it while writing
* **Scale** — text size, in tenths
* **Write a Note** — the key that opens the field, `T` by default

## Building

Requires the .NET 8 SDK. `CelestePrefix` defaults to the Steam install path:

```
dotnet build
```

The mod is copied to `<Celeste>/Mods/notes/` on build, as an uncompressed folder.

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
