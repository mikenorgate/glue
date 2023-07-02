# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build

WORKDIR /src

COPY src/. ./
WORKDIR /src/
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "TT.Deliveries.Web.Api.dll"]