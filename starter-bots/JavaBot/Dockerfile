FROM eclipse-temurin:11-jdk-jammy as build

WORKDIR /app
COPY .mvn/ .mvn
COPY mvnw pom.xml ./

RUN ./mvnw dependency:resolve

COPY src ./src
RUN ./mvnw package

# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/java:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY --from=build /app/target/JavaBot-jar-with-dependencies.jar ./JavaBot.jar

# The entrypoint to run the bot
ENTRYPOINT ["java", "-jar", "JavaBot.jar"]
