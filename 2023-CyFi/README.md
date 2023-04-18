# Game Engine

This project contains the 2023-CyFi project for the Entelect Challenge 2023

## Game Rules

The game for 2022 is CyFi. Detailed game rules can be found [here](GAMERULES.md)

## Configuration Options

The engine will respect the following environment variables to change how the game is run:

- `GameSettings.NumberOfPlayers`
    - This sets the expected amount of bots/players to connect before a game will be run
 
- `GameSettings.Levels[1-4].Seed`
    - This sets the seeds of the four worlds that will be generated in this game, they are used to generate the map's terrain and distribute items about the world. 

When these are not specified, the values present in `/CyFi/appsettings.Development.json` will be used.

## Bespin Release Format

For cloud squad the gitlab config needs to be configured for each new release/bugfix:

`year.phase.version`
