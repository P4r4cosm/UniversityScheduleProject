FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["University Schedule Lab1/University Schedule Lab1.csproj", "University Schedule Lab1/"]
RUN dotnet restore "University Schedule Lab1/University Schedule Lab1.csproj"
COPY . .
WORKDIR "/src/University Schedule Lab1"
RUN dotnet build "University Schedule Lab1.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "University Schedule Lab1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "University Schedule Lab1.dll"]
