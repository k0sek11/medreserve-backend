FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /src

COPY Medreserve.csproj ./
RUN dotnet restore Medreserve.csproj
COPY . .

FROM base AS dev
RUN dotnet tool restore
ENV PATH="${PATH}:/root/.dotnet/tools"
EXPOSE 8080
CMD ["dotnet", "watch", "--project", "Medreserve.csproj", "run", "--urls", "http://0.0.0.0:8080"]

FROM base AS build
RUN dotnet publish Medreserve.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS prod
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Medreserve.dll"]
