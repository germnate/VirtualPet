FROM node:22-alpine AS frontend-build
WORKDIR /src

COPY frontend-react/package.json frontend-react/package-lock.json ./frontend-react/
WORKDIR /src/frontend-react
RUN npm ci

WORKDIR /src
COPY frontend-react/ ./frontend-react/
RUN cd frontend-react && npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

COPY VirtualPet/ ./VirtualPet/
COPY --from=frontend-build /src/VirtualPet/wwwroot ./VirtualPet/wwwroot

WORKDIR /src/VirtualPet
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=backend-build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "VirtualPet.dll"]