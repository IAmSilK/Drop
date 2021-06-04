FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY "Drop/*.csproj" "Drop/"
RUN dotnet restore Drop

COPY . .
RUN dotnet publish Drop -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /data
COPY --from=build /app/publish /app
ENTRYPOINT ["dotnet", "/app/Drop.dll"]