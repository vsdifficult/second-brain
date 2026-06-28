FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore src/SecondBrain.Services/SecondBrain.Search/SecondBrain.Search.csproj

RUN dotnet publish \
    src/SecondBrain.Services/SecondBrain.Search/SecondBrain.Search.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 7001

ENTRYPOINT ["dotnet", "SecondBrain.Search.dll"]