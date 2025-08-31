FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ENOC.dashboard.csproj", "ENOC.dashboard/"]

RUN dotnet restore "ENOC.dashboard/ENOC.dashboard.csproj"

COPY [".", "ENOC.dashboard/"]

WORKDIR "/src/ENOC.dashboard"
RUN dotnet build "ENOC.dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ENOC.dashboard.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "ENOC.dashboard.dll"]