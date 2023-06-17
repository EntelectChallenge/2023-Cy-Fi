FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . ./

# Install Arial for build
RUN apt update && apt install -y fonts-liberation fontconfig && fc-cache

RUN dotnet restore
RUN dotnet publish --configuration Debug --output ./publish

# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/dotnetcore:2022

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY --from=build /app/publish/ .

ENV DOCKER=1

# The entrypoint to run the bot
CMD ["dotnet", "ReferenceBot.dll"]
