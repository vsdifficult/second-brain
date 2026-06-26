FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/SecondBrain.Gateway/SecondBrain.Gateway.csproj
RUN dotnet publish src/SecondBrain.Gateway/SecondBrain.Gateway.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SecondBrain.Gateway.dll"]
