FROM registry.nexdev.net:5050/docker/dotnet/core/sdk:3.1 AS builder

WORKDIR /src
COPY ./GameUser/GameUser.csproj ./GameUser/GameUser.csproj
COPY ./GameUser.Web/GameUser.Web.csproj ./GameUser.Web/GameUser.Web.csproj

WORKDIR /src/GameUser.Web/
RUN dotnet restore -v diag

WORKDIR /src
COPY ./GameUser/ ./GameUser/
COPY ./GameUser.Web/ ./GameUser.Web/

WORKDIR /src/GameUser.Web/
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM registry.nexdev.net:5050/docker/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "GameUser.Web.dll"]
