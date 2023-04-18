# Prevent .NET CLI from sending telemetry data
ARG DOTNET_CLI_TELEMETRY_OPTOUT=1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet publish --configuration Debug --output ./publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000

CMD ["dotnet", "CyFi.dll"]