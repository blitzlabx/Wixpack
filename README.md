# Wixpack by Blitz

**Creator:** Blitz · **Social:** [@blitzlabx](https://t.me/blitzlabx)

Modular C# / .NET 9 toolkit combining:

- **Telegram** bot (private + groups, colored inline buttons)
- **Floket** human verification & group security
- Games, social downloader, developer tools, API, desktop overlay (in progress)
- Experimental features (isolated)

---

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Telegram bot token from [@BotFather](https://t.me/BotFather)

---

## Configuration

Edit `config/settings.json` (or set environment variables with prefix `WIXPACK_`):

```json
{
  "LogoUrl": "",
  "DonationUrl": "",
  "Telegram": {
    "BotToken": "YOUR_BOT_TOKEN",
    "EnablePolling": true,
    "AdminUserIds": [],
    "DefaultLanguage": "en"
  },
  "Floket": {
    "VerificationTimeoutSeconds": 120,
    "MaxAttempts": 3,
    "RestrictUntilVerified": true,
    "EnabledByDefault": true
  }
}
```

**Important:** Leave `LogoUrl` and `DonationUrl` empty until you set your own URLs. Do not commit secrets; use Render env vars for `WIXPACK_Telegram__BotToken`.

Environment variable examples:

| Variable | Purpose |
|----------|---------|
| `WIXPACK_Telegram__BotToken` | Bot token |
| `WIXPACK_Telegram__EnablePolling` | `true` / `false` |
| `PORT` | HTTP port (Render sets this automatically) |

---

## Local run

```bash
# Restore & build
dotnet restore
dotnet build Wixpack.sln -c Release

# Run host (HTTP + Telegram polling)
dotnet run --project Wixpack.Host
```

Then open:

- http://localhost:5080/
- http://localhost:5080/health
- http://localhost:5080/ping

---

## Keep-alive route (Render 24/7)

Free web services on Render sleep after inactivity. Use this URL in a **Cron Job** (every 5–10 minutes):

```text
GET https://YOUR-SERVICE-NAME.onrender.com/health
```

Alternate:

```text
GET https://YOUR-SERVICE-NAME.onrender.com/ping
```

Response example (`/health`):

```json
{
  "status": "healthy",
  "product": "Wixpack by Blitz",
  "creator": "Blitz",
  "handle": "blitzlabx",
  "utc": "2026-08-25T08:00:00+00:00"
}
```

Suggested external pingers if you do not use Render Cron:

- [cron-job.org](https://cron-job.org)
- [UptimeRobot](https://uptimerobot.com)

---

## Deploy on Render

### Option A — Blueprint (`render.yaml`)

1. Push this repo to GitHub/GitLab.
2. On Render: **New → Blueprint** → select the repo.
3. Set secret env var `WIXPACK_Telegram__BotToken`.
4. Deploy.
5. Create a **Cron Job** (or external ping) hitting `https://<your-service>.onrender.com/health` every 10 minutes.

### Option B — Manual Web Service (Docker)

| Field | Value |
|-------|--------|
| **Runtime** | Docker |
| **Dockerfile path** | `./Dockerfile` |
| **Docker context** | `.` |
| **Health check path** | `/health` |

### Option C — Manual Web Service (native .NET)

If your Render instance supports the .NET native runtime:

| Field | Value |
|-------|--------|
| **Build command** | `dotnet publish Wixpack.Host/Wixpack.Host.csproj -c Release -o ./publish` |
| **Start command** | `dotnet ./publish/Wixpack.Host.dll` |
| **Health check path** | `/health` |

> **Recommended on Render today:** use **Docker** (`Dockerfile` + `render.yaml`) so .NET 9 is consistent.

### Required environment variables on Render

```text
ASPNETCORE_ENVIRONMENT=Production
WIXPACK_Telegram__BotToken=<from BotFather>
WIXPACK_Telegram__EnablePolling=true
```

Optional:

```text
WIXPACK_Logging__MinimumLevel=Information
WIXPACK_Floket__VerificationTimeoutSeconds=120
WIXPACK_Floket__MaxAttempts=3
```

---

## Project structure

```text
Wixpack/
├── config/settings.json
├── Wixpack.Host/          # Entry: HTTP + hosted Telegram
├── Wixpack.Core/          # Branding, settings, logging, DI
├── Wixpack.Telegram/      # Bot, commands, colored keyboards
├── Wixpack.Floket/        # Human verification engine
├── Wixpack.Games/
├── Wixpack.Downloader/
├── Wixpack.DevTools/
├── Wixpack.Api/
├── Wixpack.Experimental/
├── Dockerfile
├── render.yaml
└── README.md
```

---

## Floket verification

Floket is the security component for protected Telegram groups:

- Session IDs + one-time challenge tokens
- Expiry windows
- Attempt limits
- Abuse / failure handling
- In-memory store (single instance; swap for Redis/DB for multi-instance)

UI always shows **Powered by Floket**.

---

## Telegram buttons

Uses Bot API free styles (not Premium):

| Style | Color | Use |
|-------|-------|-----|
| `primary` | Blue | Main actions |
| `success` | Green | Confirm / games |
| `danger` | Red | Cancel / close |
| *(default)* | Neutral / transparent | Secondary |

Custom emoji **icons** on buttons require Premium — not enabled by default.

---

## Branding

- **Product:** Wixpack by Blitz  
- **Creator:** Blitz  
- **Handle:** blitzlabx  

---

## License / notes

Provide your own logo and donation URLs in `settings.json`.  
Do not commit bot tokens. Use Render environment variables for secrets.

---

## HTTP API (Host)

Base: your service URL (local `http://localhost:5080`).

| Method | Path | Notes |
|--------|------|-------|
| GET | `/health` | **Keep-alive for Render cron** |
| GET | `/ping` | Simple pong |
| GET | `/api` | Endpoint index |
| POST | `/api/download` | Body `{"url":"..."}` — Prexzy-backed resolver |
| GET | `/api/download/ytinfo?url=` | YouTube metadata via Prexzy |
| POST | `/api/tools/json/format` | Raw JSON body |
| POST | `/api/tools/json/minify` | Raw JSON body |
| POST | `/api/tools/base64/encode` | Text body |
| POST | `/api/tools/base64/decode` | Text body |
| GET | `/api/tools/uuid` | New GUID |
| POST | `/api/tools/hash` | `{"text":"...","algorithm":"SHA256"}` |
| POST | `/api/tools/regex` | `{"pattern":"...","input":"..."}` |
| POST | `/api/tools/jwt` | JWT string body |
| GET/POST | `/api/tools/timestamp` | Now / convert |
| POST | `/api/tools/qr` | Text → PNG |
| GET | `/api/experimental/coin-flip` | Isolated experimental |

Downloader uses [Prexzy APIs](https://docs.prexzyapis.com/) (`https://prexzyapis.com`) — free, no key. Platform routes: YouTube, TikTok, Instagram, Twitter/X, Facebook, Spotify, SoundCloud, etc.
