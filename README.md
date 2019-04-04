# Readme

**Touhou Replay Manager** is a tool to help manage your Touhou replay files. It allows you to easily backup, restore and keep track of your replays and also serves as a unified game launcher.

This tool supports:

- Mainline Touhou games from 6 through to 16
- Touhou 9.5 - Shoot the Bullet
- Touhou 12.5 - Double Spoiler
- Touhou 12.8 - Great Fairy Wars
- Touhou 14.3 - Impossible Spell Card
- Touhou 16.5 - Violet Detector

Replay decoding was developed based on info provided by [http://opensrc.club/projects/thrp/](http://opensrc.club/projects/thrp/), [https://pytouhou.linkmauve.fr/doc/06/t6rp.xhtml](https://pytouhou.linkmauve.fr/doc/06/t6rp.xhtml) and Fluorohydride's [threp](https://github.com/Fluorohydride/threp)

## Features

- Move, copy and rename replays to and from a backup folder for each game
- View replay data like player name, score, etc directly within the program for easier organisation of replays
- Launch the specified game executable
- Open the game folder, or either of the specified replay folders

## Todo

- full replay data viewing functionality similar to the online replay uploaders
- applocale support for game launching
- load game info from a separate non-hardcoded file, to allow for easy updates or fangames
- tracking unique replay files with MD5 sum and an SQLite database
- thcrap integration (highly unlikely, but would pretty much turn this into an all-in-one touhou hub)
