FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish --configuration Debug --output ./publish

# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/dotnetcore:2022

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY --from=build /app/publish/ .

# The entrypoint to run the bot
ENTRYPOINT ["dotnet", "NETCoreBot.dll"]
