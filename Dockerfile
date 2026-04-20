FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /src

COPY studia.csproj ./
RUN dotnet restore studia.csproj
COPY . .

FROM base AS dev
EXPOSE 8080
CMD ["dotnet", "watch", "--project", "studia.csproj", "run", "--urls", "http://0.0.0.0:8080"]

FROM base AS build
RUN dotnet publish studia.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS prod
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "studia.dll"]
