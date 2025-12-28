# Build Frontend
FROM node:24-alpine AS web-client

WORKDIR /usr/src/app

COPY . .

WORKDIR /usr/src/app/WebClient

RUN yarn && yarn build

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:10 AS web-server

WORKDIR /usr/src/app

COPY . .

RUN dotnet restore

WORKDIR /usr/src/app/WebServer

RUN dotnet publish -c Release

# Create Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /usr/src/app

RUN apt-get update && apt-get install -y ffmpeg && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=web-server /usr/src/app/WebServer/bin/Release/net10.0/publish .
COPY --from=web-client /usr/src/app/WebClient/dist WebClient

ENTRYPOINT ["dotnet", "JMS.VCR.NET.dll"]
