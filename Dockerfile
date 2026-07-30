FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Agerie365.API.csproj ./
RUN dotnet restore Agerie365.API.csproj

COPY . ./
RUN dotnet publish Agerie365.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Agerie365.API.dll"]
