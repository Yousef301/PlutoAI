FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pluto.API/Pluto.API.csproj", "Pluto.API/"]
COPY ["Pluto.Application/Pluto.Application.csproj", "Pluto.Application/"]
COPY ["Pluto.DAL/Pluto.DAL.csproj", "Pluto.DAL/"]
RUN dotnet restore "Pluto.API/Pluto.API.csproj"
COPY . .
WORKDIR "/src/Pluto.API"
RUN dotnet build "Pluto.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Pluto.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pluto.API.dll"]
