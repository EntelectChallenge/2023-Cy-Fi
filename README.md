# Entelect Challenge 2023 - Cy-Fi - Release 2023.2.0

The Entelect Challenge is an annual coding competition where students, professional developers, and enthusiasts develop an intelligent bot to play a game.

This year, the game is Cy-Fi 🪙!

---


>#### Refer to GAMERULES.md FOR change log
>
> Best regards and good luck!

---

We have made it as simple as possible to get started. Just follow the steps below.

## Step 1 - Download
Download the starter pack zip file and extract it to the folder you want to work.

## Step 2 - Run
The application can be run using docker via the following commands:

```sh
# Windows
.\run.cmd
```

```sh
# Linux/MacOS/Unix
./run.sh
```

## Step 3 - Upload
Sign up to the player portal [here](https://challenge.entelect.co.za/login), and follow the steps to upload your bot and participate in tournaments!

## Step 4 - Improve
Customize the provided logic or create your own to enhance one of the given starter bots, and then upload it to the player portal to see how it stacks up against the competition!

## WIN!!!
For more information, visit one of the following:

[Our website](https://challenge.entelect.co.za)

[Our forum](https://forum.entelect.co.za)

Or read the rules in the [game-rules.md](./2023-CyFi/GAMERULES.md) file.

## Project Structure

In this project you will find everything we use to build a starter pack that you can use to run a bot on your local machine. This project contains the following:

1. **2023-CyFi** - This base project contains the following files:
    * **engine** - The engine enforces the game's rules by applying the bot commands to the game state if they are valid.
    * **runner** - The runner runs matches between players, calling the appropriate commands as given by the bots and handing them to the engine to execute.
    * **logger** - The logger captures all changes to game state, as well as any exceptions, and writes them to a log file at the end of the game.
4. **starter-bots** - Starter bots with limited logic that can be used as a starting point for your bot. This folder also contains a bot called `ReferenceBot (Coming soon)`, which you can use to test your bot against!

This project can be used to get a better understanding of the rules and to help debug your bot.

Improvements and enhancements will be made to the game engine code over time.

The game engine is available to the community for peer review and bug fixes. If you find any bugs or have any concerns, please [e-mail us](mailto:challenge@entelect.co.za) or discuss it with us on the [forum](http://forum.entelect.co.za/). Alternatively submit a pull request on Github, and we will review it.

## Starter Pack - Coming Soon
The starter pack will provide everything you need to run your first bot and compete in this year's challenge. COMING SOON!

## Submission Process
We have automate submissions through GitHub!
For more information, sign up for the player portal [here](https://challenge.entelect.co.za/login), and follow the steps!
