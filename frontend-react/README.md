# Frontend Notes

This React app is built with Vite, but production builds are emitted into `../VirtualPet/wwwroot` so the ASP.NET backend can serve the static files directly.

## Local Frontend Development

Run the backend first in another terminal:

```powershell
Set-Location ..\VirtualPet
dotnet run --launch-profile http
```

Then run the frontend dev server from this folder:

```powershell
npm.cmd install
$env:VITE_API_URL="http://localhost:5058"
npm.cmd run dev
```

Open:

```text
http://localhost:5173
```

## Production Build

Create a production build that the backend serves from `wwwroot`:

```powershell
npm.cmd run build
```

For Docker deployment, LAN access, and the full local workflow, see the repository README in the workspace root.
