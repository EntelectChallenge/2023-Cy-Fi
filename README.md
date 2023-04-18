# Entelect Challenge 2023 - Cy-Fi

The Entelect Challenge is an annual coding competition where students, professional developers, and enthusiasts develop an intelligent bot to play a game.

This year, the game is Cy-Fi 🪙!

---


>#### _NB_ - **Project is a work in progress** 🔧
>\
>The Entelect Challenge has undergone some restructuring this year.
>
>We will be releasing the **full game** at the start, meaning no more periodic feature releases or last-minute changes (fingers crossed). This. is. it. 🔥
>
>We apologize that these changes have affected our timelines, resulting in us not being able to deliver everything we planned for this release. 😭😭😭
>
>Rather than delaying the release for a week, we have decided to open the project up to our AWESOME community (that's you!) to give you a head start. This way, you can catch a few bugs that we missed while building your bots, and we can patch our game while you all see what we have in store for you 🤭.
>
>Keep and eye on our [forum](http://forum.entelect.co.za/) for any incoming updates, patches, and bug fixes 🔜. There may be a shiny visualiser in your future!
>
>\
>As always thank you for your continued support and lovely feedback 😊
>
>\
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
    * **logger** (WIP 🔧) - The logger captures all changes to game state, as well as any exceptions, and writes them to a log file at the end of the game.
4. **starter-bots** (WIP 🔧) - Starter bots with limited logic that can be used as a starting point for your bot. This folder also contains a bot called `ReferenceBot (Coming soon)`, which you can use to test your bot against!

This project can be used to get a better understanding of the rules and to help debug your bot.

Improvements and enhancements will be made to the game engine code over time.

The game engine is available to the community for peer review and bug fixes. If you find any bugs or have any concerns, please [e-mail us](mailto:challenge@entelect.co.za) or discuss it with us on the [forum](http://forum.entelect.co.za/). Alternatively submit a pull request on Github, and we will review it.

## Starter Pack - Coming Soon
The starter pack will provide everything you need to run your first bot and compete in this year's challenge. COMING SOON!

## Submission Process
We have automate submissions through GitHub!
For more information, sign up for the player portal [here](https://challenge.entelect.co.za/login), and follow the steps!
