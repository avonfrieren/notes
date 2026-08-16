# notes

[Everest](https://everestapi.github.io/) mod for [Celeste](https://www.celestegame.com/) that
displays any text you want on screen.

No helper to install, no map to edit, no entity to place: you press a key, you type your text,
it shows up on the HUD. Useful for run notes, route reminders, TODOs while mapping, or anything
you want on screen while you play.

## Usage

Press **T** in a level (rebindable in the mod options). The game pauses, a field opens right
where the note will live, and you type. `Enter` validates, `Escape` cancels.

Reopening the field gives you the current note back, so you can edit it instead of retyping
it. Validating an empty field clears the note.

The field supports what you would expect from a text field: `←` `→` to move (`Ctrl` to move
by word), `Home` / `End`, `Backspace` and `Delete` (`Ctrl+Backspace` deletes a word), and
`Ctrl+V` to paste.

A literal `\n` becomes a line break once validated:

```
first line\nsecond line
```

Notes are **volatile**: they are never saved, they disappear when you close the game.

### Console command

The debug console (`.` by default) also works, which is handy for scripting:

```
note remember to grab the berry
```

Called without arguments, it clears the note. Beware that the console parser of the engine
splits on spaces *and* commas and knows nothing about quotes, so commas you type are dropped,
consecutive spaces collapse into one, and a note is capped at 16 words. The in-game field has
none of these limitations.

## Options

* **Enabled** — show or hide the note without clearing it
* **Anchor** — which corner or edge of the screen the note sticks to
* **Scale** — text size, in tenths
* **Write a Note** — the key that opens the field, `T` by default

## Building

Requires the .NET 8 SDK. `CelestePrefix` defaults to the Steam install path:

```
dotnet build
```

The mod is copied to `<Celeste>/Mods/notes/` on build, as an uncompressed folder.

## Changelog

### v0.2.0

* **Live typing**: press `T` in a level to open a field and write your note directly, with
  cursor movement, word deletion and clipboard paste
* Reopening the field prefills it with the current note
* The field renders at the note's final position, so you see what you are going to get
* Accents and any character your keyboard layout produces are supported, unlike the console

### v0.1.0

* **Initial release**
* `note` console command, HUD display, anchor and scale options
