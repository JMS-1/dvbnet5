# Build Frontend
FROM node:20-alpine as WebClient

WORKDIR /usr/src/app

COPY . .

WORKDIR /usr/src/app/WebClient

RUN yarn && yarn build

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS WebServer

WORKDIR /usr/src/app

COPY . .

RUN dotnet restore

WORKDIR /usr/src/app/WebServer

RUN dotnet publish -c Release

# Create Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /usr/src/app

COPY --from=WebServer /usr/src/app/WebServer/bin/Release/net8.0/publish .
COPY --from=WebClient /usr/src/app/WebClient/dist WebClient

ENTRYPOINT ["dotnet", "JMS.VCR.NET.dll"]
