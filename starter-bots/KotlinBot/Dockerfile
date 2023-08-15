FROM maven:3.9.2-eclipse-temurin-11 AS build

COPY ./ ./

RUN ./gradlew clean build

# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/java:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY --from=build build/libs/KotlinBot-1.0-SNAPSHOT-standalone.jar ./KotlinBot.jar

# The entrypoint to run the bot
ENTRYPOINT ["java", "-jar", "KotlinBot.jar"]
