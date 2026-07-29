FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the API project file specifically
COPY Agerie365.API/Agerie365.API.csproj Agerie365.API/
RUN dotnet restore Agerie365.API/Agerie365.API.csproj

# Copy everything else
COPY . .
WORKDIR /src/Agerie365.API
RUN dotnet publish Agerie365.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Agerie365.API.dll"]
