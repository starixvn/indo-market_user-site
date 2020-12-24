FROM registry.nexdev.net:5050/docker/dotnet/core/sdk:3.1 AS builder

WORKDIR /src

COPY ./GameUser ./GGameUser
COPY ./GameUser.Web/GameUser.Web.csproj ./GameUser.Web/GameUser.Web.csproj

WORKDIR /src/GameUser.Web/
RUN dotnet restore -v diag

COPY ./GameUser/GameUser.Web/ .

RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM registry.nexdev.net:5050/docker/dotnet/core/aspnet:3.1 AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "GameUser.Web.dll"]