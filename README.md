# Virtual Pet

This repo contains:

- `frontend-react`: the Vite/React UI
- `VirtualPet`: the ASP.NET backend API that also serves the built frontend from `wwwroot`

## Local Development Without Docker

You do not need Docker for normal development.

### Backend API

Start the ASP.NET backend first:

```powershell
Set-Location .\VirtualPet
dotnet run --launch-profile http
```

The backend listens on:

```text
http://localhost:4058
```

### Frontend Dev Server

In a second terminal, start the Vite dev server:

```powershell
Set-Location .\frontend-react
npm.cmd install
npm.cmd run dev
```

Open the frontend at:

```text
http://localhost:5173
```

### Notes

- `npm.cmd` is the Windows npm launcher. Use it if PowerShell blocks plain `npm`.
- Keep both terminals running while developing.
- Vite proxies `/pet` requests to `http://localhost:4058` during local development.
- Use this flow for fast frontend iteration with Vite hot reload.

## Production-Style Local Run Without Docker

If you want to test the app the same way the backend serves it in production:

```powershell
Set-Location .\frontend-react
npm.cmd run build

Set-Location ..\VirtualPet
dotnet run --launch-profile http
```

Then open:

```text
http://localhost:4058
```

This builds the frontend into `VirtualPet/wwwroot`, and the backend serves those files directly.

## Docker Deployment

The Docker setup builds the frontend, copies the generated files into `VirtualPet/wwwroot`, publishes the ASP.NET app, and runs everything in a single container.

### Requirements

- Docker Desktop running
- Port `5058` available on the host

### Build and Start

From the repository root:

```powershell
docker compose up -d --build
```

Open the app locally at:

```text
http://localhost:5058
```

### Stop

```powershell
docker compose down
```

### Rebuild After Changes

If you change frontend or backend code and want the container updated, run:

```powershell
docker compose up -d --build
```

This rebuilds the image and republishes the latest frontend and backend code into the container.

### Persistent Data

The SQLite database is stored in the Docker volume `petdata`.

If you want a completely fresh app state:

```powershell
docker compose down -v
```

## LAN Access

Other devices on your network should use:

```text
http://192.168.86.29:5058
```

If that works on your PC but not from another device, the usual cause is Windows Firewall. Allow inbound TCP traffic on port `5058`.

Example PowerShell command:

```powershell
New-NetFirewallRule -DisplayName "Virtual Pet 5058" -Direction Inbound -Protocol TCP -LocalPort 5058 -Action Allow
```

## Time Zone

The container time zone is currently set in `docker-compose.yml` via the `TZ` environment variable. Update that value if you are not in the configured time zone, since feeding windows use local server time.