# Java Readme

## Installation

To run the Java bot, download Java JDK 11

## Running

### IDE
To run the Java bot, you can either use **IntelliJ** or any IDE that is able to run Java. If you're using **VSCode** you may need to install the Java extention to run the bot. 

### Docker
Build the docker image by running `docker build -t <image_name> .` in the root directory i.e. /JavaBot  
Then run the container using `docker run --env=RUNNER_IPV4=host.docker.internal javabot`. Be sure to have the engine running before you run your bot.  
You can change the container name by adding the [`--name`](https://docs.docker.com/engine/reference/commandline/run/#name) option to the run command.  
You can also change the name of the JavaBot in the logs by adding the `--env=BOT_NICKNAME=MyBotName` option to the run command  
