# notes

[Everest](https://everestapi.github.io/) mod for [Celeste](https://www.celestegame.com/) that
displays any text you want on screen.

No helper to install, no map to edit, no entity to place: you open the debug console, you type
your text, it shows up on the HUD. Useful for run notes, route reminders, TODOs while mapping,
or anything you want on screen while you play.

## Usage

Open the debug console (`.` or `` ` ``, rebindable in Everest's mod options) inside a level:

```
note remember to grab the berry
```

The text appears on the HUD and stays there across rooms and deaths. To clear it, call the
command with no argument:

```
note
```

A literal `\n` becomes a line break:

```
note first line\nsecond line
```

Notes are **volatile**: they are never saved, they disappear when you close the game.

### Known limitations

The console parser of the engine splits on spaces *and* commas, and knows nothing about
quotes. So commas you type are dropped, consecutive spaces collapse into one, and a note is
capped at 16 words. Live typing in a proper text area is planned for v0.2.

## Options

* **Enabled** — show or hide the note without clearing it
* **Anchor** — which corner or edge of the screen the note sticks to
* **Scale** — text size, in tenths

## Building

Requires the .NET 8 SDK. `CelestePrefix` defaults to the Steam install path:

```
dotnet build
```

The mod is copied to `<Celeste>/Mods/notes/` on build, as an uncompressed folder.

## Changelog

### v0.1.0

* **Initial release**
* `note` console command, HUD display, anchor and scale options
