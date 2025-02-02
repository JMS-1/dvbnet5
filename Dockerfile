# Build Frontend
FROM node:20-alpine AS web-client

WORKDIR /usr/src/app

COPY . .

WORKDIR /usr/src/app/WebClient

RUN yarn && yarn build

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS web-server

WORKDIR /usr/src/app

COPY . .

RUN dotnet restore

WORKDIR /usr/src/app/WebServer

RUN dotnet publish -c Release

# Create Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /usr/src/app

COPY --from=web-server /usr/src/app/WebServer/bin/Release/net9.0/publish .
COPY --from=web-client /usr/src/app/WebClient/dist WebClient

ENTRYPOINT ["dotnet", "JMS.VCR.NET.dll"]
