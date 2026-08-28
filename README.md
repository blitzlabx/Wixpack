<div align="center">

<img src="https://u.pone.rs/wpkliixf.jpg" alt="Wixpack by Blitz" width="180" style="border-radius: 24px;" />

<br/><br/>

# **Wixpack by Blitz**

### Modular toolkit for Telegram, security, games, media & developer power

<br/>

<img src="https://readme-typing-svg.demolab.com?font=Inter&weight=600&size=22&duration=3500&pause=900&color=3B82F6&center=true&vCenter=true&multiline=true&repeat=true&width=680&height=70&lines=Telegram+%C2%B7+Floket+Security+%C2%B7+Games;Downloader+%C2%B7+DevTools+%C2%B7+HTTP+API;Built+by+Blitz+%C2%B7+%40blitzlabx" alt="Wixpack typing banner" />

<br/>

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Telegram](https://img.shields.io/badge/Telegram-Bot-26A5E4?style=for-the-badge&logo=telegram&logoColor=white)](https://telegram.org/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?style=for-the-badge&logo=render&logoColor=white)](https://render.com/)
[![License](https://img.shields.io/badge/License-Private-0F172A?style=for-the-badge)](#)

<br/>

**Creator** · **Blitz**  
**Telegram** · [**@blitzlabx**](https://t.me/blitzlabx)

<br/>

[![Follow Blitz](https://img.shields.io/badge/Follow-@blitzlabx-26A5E4?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/blitzlabx)
[![Donate](https://img.shields.io/badge/Support-Donate-10B981?style=for-the-badge&logo=heart&logoColor=white)](https://flutterwave.com/donate/knjcdfjgzwyt)

<br/>

<img src="https://user-images.githubusercontent.com/74038190/212284100-561aa473-3905-4a80-b561-0d28506553ee.gif" width="700" alt="divider" />

</div>

---

## Table of contents

- [What is Wixpack?](#what-is-wixpack)
- [Feature map](#feature-map)
- [Architecture](#architecture)
- [Screens and menus](#screens-and-menus)
- [Telegram bot](#telegram-bot)
- [Floket human verification](#floket-human-verification)
- [Games](#games)
- [Media downloader](#media-downloader)
- [Developer tools](#developer-tools)
- [HTTP API](#http-api)
- [Configuration](#configuration)
- [Environment variables](#environment-variables)
- [Local development](#local-development)
- [Deploy on Render](#deploy-on-render)
- [Keep-alive (24/7)](#keep-alive-247)
- [Docker](#docker)
- [Project structure](#project-structure)
- [Commands cheat sheet](#commands-cheat-sheet)
- [Security notes](#security-notes)
- [Roadmap](#roadmap)
- [Support and contact](#support-and-contact)
- [Credits](#credits)

---

## What is Wixpack?

**Wixpack by Blitz** is a production-oriented **.NET 9** application that packages several useful systems into one deployable service:

| Pillar | What you get |
|--------|----------------|
| Telegram | Interactive bot with colored buttons, menus, groups and commands |
| Floket | Real human-verification for protected groups |
| Games | In-chat games with sessions and scores |
| Downloader | Resolve media links into downloadable sources |
| DevTools | UUID, hash, timestamps, Base64, QR and more |
| HTTP API | REST surface for tools, health and downloads |
| Cloud-ready | Docker + Render blueprint + keep-alive health route |

Built with a **modular C# architecture** so each domain stays maintainable: Core, Telegram, Floket, Games, Downloader, DevTools, Experimental, and Host.

---

## Feature map

```text
                         +--------------------------+
                         |      Wixpack.Host        |
                         |  HTTP + Background bot   |
                         +------------+-------------+
                                      |
        +--------------+--------------+--------------+--------------+
        v              v              v              v              v
   Telegram        Floket          Games        Downloader       DevTools
   menus/cmds      verify          RPS          media resolve    UUID/hash
   groups          sessions        registry     link pipeline    QR / JWT
   buttons         restrict        scores       multi-platform   timestamps
```

### Highlight capabilities

- **Colored inline buttons** (blue / green / red + neutral) — free Bot API styles
- **Group moderation hooks** via Floket (restrict, challenge, unrestrict / kick)
- **Long-polling Telegram bot** suitable for single-instance cloud hosts
- **Health endpoints** designed for uptime pingers
- **Settings-driven branding** — your logo URL and donation URL stay under your control

---

## Architecture

| Project | Role |
|---------|------|
| `Wixpack.Host` | Entry point — Kestrel HTTP + hosted Telegram service |
| `Wixpack.Core` | Branding, settings model, logging, `Result<T>`, DI |
| `Wixpack.Telegram` | Bot client, commands, keyboards, callbacks |
| `Wixpack.Floket` | Verification engine, challenges, session store |
| `Wixpack.Games` | Game registry, sessions, Rock–Paper–Scissors |
| `Wixpack.Downloader` | Media resolution client and platform routing |
| `Wixpack.DevTools` | Developer utilities |
| `Wixpack.Experimental` | Isolated experiments (safe to remove) |
| `Wixpack.Api` | Optional API project scaffold |

**Principles**

- Dependency injection everywhere
- Options pattern for configuration
- Serilog structured logging
- No dead placeholder handlers on core menu paths
- Secrets preferred via environment variables in production

---

## Screens and menus

After `/start`, users see the main menu:

| Button | Style | Opens |
|--------|-------|--------|
| Dev Tools | Blue | UUID, timestamp, coin flip |
| Games | Green | Game list + start RPS |
| Downloader | Blue | How to use `/dl` |
| Floket Verify | Blue | Group protection guide |
| Settings | Neutral | Logo, donation, Floket limits |
| About | Neutral | Product + creator card |
| Close | Red | Dismiss menu |

Navigation always includes a **Back** path to the main menu.

---

## Telegram bot

### Requirements

1. Create a bot with [@BotFather](https://t.me/BotFather)
2. Copy the **bot token**
3. Put the token in `settings.json` **or** (recommended on Render) in env:

```text
WIXPACK_Telegram__BotToken=YOUR_TOKEN_HERE
```

4. Optional: set your numeric Telegram user id(s) as admins

### Behaviour

- Registers bot commands with Telegram on startup
- Handles messages, callback queries, and chat-member updates
- Private chats: a bare `https://…` URL is treated as a download request
- Groups: Floket can challenge new members when the bot is admin with **Restrict members**

### Branding in chat

Menus and about text show:

- **Wixpack by Blitz**
- Creator **Blitz**
- Contact **[@blitzlabx](https://t.me/blitzlabx)**

Logo and donation links come from configuration so your brand assets stay yours.

---

## Floket human verification

Floket is the **group security** module — not a decorative placeholder.

### Flow

1. New member joins a protected group
2. Bot **restricts** send permissions (needs admin rights)
3. Member receives a **one-time challenge** with answer buttons
4. Correct → unrestrict + success message
5. Too many failures / expiry → failure handling (optional kick)

### Built-in controls

| Control | Default | Purpose |
|---------|---------|---------|
| Session ID | random | Tracks one verification attempt |
| Challenge token | random | Binds UI to the session |
| Timeout | 120s | Auto-expire stale challenges |
| Max attempts | 3 | Abuse resistance |
| Restrict until verified | on | Quiet rooms until humans pass |
| Powered by Floket | always | Clear security branding in UI |

### Admin setup checklist

- [ ] Bot added to the group
- [ ] Bot promoted with **Restrict members** (and ban if you want kick-on-fail)
- [ ] Floket enabled (default on)
- [ ] Test with a secondary account

---

## Games

Modular game engine:

- `IGame` contract
- `GameRegistry` discovery
- In-memory session store (single instance)

### Rock Paper Scissors

| Item | Detail |
|------|--------|
| Command | `/game rps` |
| Menu | Games → Rock Paper Scissors |
| Play | Players tap Rock / Paper / Scissors |
| End | First two distinct picks resolve the round |

More games can plug into the same registry without touching Host wiring.

---

## Media downloader

Resolve public media links into usable download metadata and direct file URLs where available.

### How to use

```text
/dl https://example.com/your-media-link
```

In a **private chat** with the bot, you can also paste the URL alone.

### Platform routing (built-in)

The downloader selects the correct internal pipeline from the link host, including support paths for:

- YouTube
- TikTok
- Instagram
- X / Twitter
- Facebook
- Spotify
- SoundCloud
- MediaFire / Pinterest-style hosts
- Generic **AIO** fallback for other public links

> Results depend on upstream availability and the public status of the media. Private or region-locked content may fail cleanly with an error message.

### Response UX

- Status message while resolving
- Title / quality / format when present
- **Open download** button when a direct URL is returned

---

## Developer tools

Available from the **Dev Tools** menu and the HTTP API.

| Tool | Telegram | HTTP |
|------|----------|------|
| New UUID | Button | `GET /api/tools/uuid` |
| UTC timestamp | Button | `GET /api/tools/timestamp` |
| Coin flip | Button | `GET /api/experimental/coin-flip` |
| SHA / MD5 hash | API | `POST /api/tools/hash` |
| Base64 encode/decode | API | `POST /api/tools/base64/*` |
| JSON format/minify | API | `POST /api/tools/json/*` |
| URL encode/decode | API | `POST /api/tools/url/*` |
| Regex test | API | `POST /api/tools/regex` |
| JWT inspect | API | `POST /api/tools/jwt` |
| QR PNG | API | `POST /api/tools/qr` |

---

## HTTP API

The Host listens on `PORT` (Render injects this automatically).

### Always-on routes

| Method | Path | Purpose |
|--------|------|---------|
| `GET` | `/` | Service card |
| `GET` | `/health` | **Keep-alive / health check** |
| `GET` | `/ping` | Simple `pong` |
| `GET` | `/api` | Endpoint index |

### Example: health

```bash
curl -s https://YOUR-SERVICE.onrender.com/health
```

```json
{
  "status": "healthy",
  "product": "Wixpack by Blitz",
  "creator": "Blitz",
  "handle": "blitzlabx",
  "utc": "2026-08-28T07:00:00+00:00"
}
```

### Example: download

```bash
curl -s -X POST https://YOUR-SERVICE.onrender.com/api/download \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.youtube.com/watch?v=jNQXAC9IVRw"}'
```

### Example: hash

```bash
curl -s -X POST https://YOUR-SERVICE.onrender.com/api/tools/hash \
  -H "Content-Type: application/json" \
  -d '{"text":"wixpack","algorithm":"SHA256"}'
```

---

## Configuration

Primary file: **`config/settings.json`**  
Also keep **`Wixpack.Host/config/settings.json`** in sync for local runs.

```json
{
  "LogoUrl": "https://u.pone.rs/wpkliixf.jpg",
  "DonationUrl": "https://flutterwave.com/donate/knjcdfjgzwyt",
  "Telegram": {
    "BotToken": "",
    "EnablePolling": true,
    "AdminUserIds": ["8656909561"],
    "DefaultLanguage": "en"
  },
  "Floket": {
    "VerificationTimeoutSeconds": 120,
    "MaxAttempts": 3,
    "RestrictUntilVerified": true,
    "EnabledByDefault": true
  },
  "Downloader": {
    "OutputDirectory": "downloads",
    "YtDlpPath": null,
    "FFmpegPath": null
  },
  "Api": {
    "Host": "http://localhost:5080",
    "RequireApiKey": false,
    "ApiKey": ""
  },
  "Logging": {
    "MinimumLevel": "Information",
    "LogDirectory": "logs"
  }
}
```

### Field guide

| Field | You should… |
|-------|-------------|
| `LogoUrl` | Your public logo image URL |
| `DonationUrl` | Your support / donation page |
| `Telegram.BotToken` | Prefer **env var** on Render, not git |
| `Telegram.AdminUserIds` | Array of numeric user ids as **strings** |
| `Floket.*` | Tune timeout and attempts per community strictness |
| `Api.RequireApiKey` | Leave `false` unless you add key middleware later |

**API key is not required** for the current Host routes.

### Multiple admins

```json
"AdminUserIds": ["8656909561", "111222333", "444555666"]
```

---

## Environment variables

Prefix: **`WIXPACK_`** (nested keys use `__`).

```env
WIXPACK_LogoUrl=https://u.pone.rs/wpkliixf.jpg
WIXPACK_DonationUrl=https://flutterwave.com/donate/knjcdfjgzwyt
WIXPACK_Telegram__BotToken=PUT_TOKEN_HERE
WIXPACK_Telegram__EnablePolling=true
WIXPACK_Telegram__AdminUserIds__0=8656909561
WIXPACK_Telegram__DefaultLanguage=en
WIXPACK_Api__RequireApiKey=false
ASPNETCORE_ENVIRONMENT=Production
DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE=false
```

> `DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE=false` avoids **inotify** limit crashes on small cloud containers (common on Render free).

Env vars **override** `settings.json`.

---

## Local development

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A Telegram bot token

### Run

```bash
# from repo root
dotnet restore
dotnet build Wixpack.sln -c Release
dotnet run --project Wixpack.Host
```

Open:

- http://localhost:5080/
- http://localhost:5080/health
- http://localhost:5080/api

Then message your bot: `/start`

---

## Deploy on Render

### Option A — Docker (recommended)

| Setting | Value |
|---------|--------|
| Runtime | **Docker** |
| Dockerfile path | `./Dockerfile` |
| Docker context | `.` |
| Health check path | `/health` |

### Option B — Blueprint

Connect the repo and apply `render.yaml`.

### Option C — Native publish commands

| Field | Command |
|-------|---------|
| **Build** | `dotnet publish Wixpack.Host/Wixpack.Host.csproj -c Release -o ./publish` |
| **Start** | `dotnet ./publish/Wixpack.Host.dll` |
| **Health** | `/health` |

### Required env on Render

```text
WIXPACK_Telegram__BotToken=<from BotFather>
WIXPACK_Telegram__EnablePolling=true
ASPNETCORE_ENVIRONMENT=Production
DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE=false
```

Optional branding:

```text
WIXPACK_LogoUrl=https://u.pone.rs/wpkliixf.jpg
WIXPACK_DonationUrl=https://flutterwave.com/donate/knjcdfjgzwyt
WIXPACK_Telegram__AdminUserIds__0=8656909561
```

After deploy, confirm logs show the bot online, then open:

```text
https://YOUR-SERVICE.onrender.com/health
```

---

## Keep-alive (24/7)

Free web services sleep when idle. Ping **every 5–10 minutes**:

```text
GET https://YOUR-SERVICE.onrender.com/health
```

Alternate:

```text
GET https://YOUR-SERVICE.onrender.com/ping
```

Use **Render Cron**, [cron-job.org](https://cron-job.org), or [UptimeRobot](https://uptimerobot.com).

---

## Docker

Multi-stage build ships with the repo (`Dockerfile`). Local image test:

```bash
docker build -t wixpack .
docker run --rm -p 5080:8080 \
  -e PORT=8080 \
  -e WIXPACK_Telegram__BotToken=YOUR_TOKEN \
  -e DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE=false \
  wixpack
```

---

## Project structure

```text
Wixpack/
├── config/
│   └── settings.json
├── Wixpack.Host/
│   ├── Program.cs
│   └── config/settings.json
├── Wixpack.Core/
├── Wixpack.Telegram/
│   ├── Commands/          # start, help, game, dl
│   ├── Handlers/          # callbacks
│   ├── Keyboards/         # colored menus
│   ├── Floket/            # group join wiring
│   └── Services/          # long-polling host
├── Wixpack.Floket/
├── Wixpack.Games/
├── Wixpack.Downloader/
├── Wixpack.DevTools/
├── Wixpack.Experimental/
├── Wixpack.Api/
├── Dockerfile
├── render.yaml
└── README.md
```

---

## Commands cheat sheet

| Command | Description |
|---------|-------------|
| `/start` | Open the main interactive menu |
| `/help` | Command list |
| `/game` | List registered games |
| `/game rps` | Start Rock–Paper–Scissors |
| `/dl <url>` | Resolve a media URL |

**Tip:** In private chat, send the URL by itself — same as `/dl`.

---

## Security notes

- Never commit real bot tokens to a **public** repository
- Prefer Render **secret env vars** for `WIXPACK_Telegram__BotToken`
- If a token was exposed in chat or git history, revoke it in BotFather and issue a new one
- Floket needs correct admin permissions or restrict/kick calls will fail in logs
- Treat download endpoints as utility resolvers — respect platform terms and copyright

---

## Roadmap

- [ ] Additional games (trivia, number guess, group leaderboards)
- [ ] Persistent storage (Redis / Postgres) for multi-instance Floket
- [ ] Webhook mode as an alternative to long polling
- [ ] Windows desktop overlay (separate WPF package)
- [ ] Richer admin panel commands

---

## Support and contact

<div align="center">

### Built with care by **Blitz**

**Telegram:** [**@blitzlabx**](https://t.me/blitzlabx)

[![Telegram](https://img.shields.io/badge/Message-Blitz-26A5E4?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/blitzlabx)
[![Donate](https://img.shields.io/badge/Support-Donate-F5A623?style=for-the-badge&logo=flutter&logoColor=white)](https://flutterwave.com/donate/knjcdfjgzwyt)

<img src="https://u.pone.rs/wpkliixf.jpg" width="96" style="border-radius:16px;" alt="Wixpack logo" />

</div>

---

## Credits

| | |
|--|--|
| **Product** | Wixpack by Blitz |
| **Creator** | Blitz |
| **Telegram** | [@blitzlabx](https://t.me/blitzlabx) |
| **Runtime** | .NET 9 · C# |
| **Bot stack** | Telegram Bot API |
| **Hosting target** | Render · Docker · any Kestrel host |

---

<div align="center">

<img src="https://user-images.githubusercontent.com/74038190/212284087-bbe7e430-757e-4901-90bf-4cd2ce72c786.gif" width="500" alt="footer wave" />

<br/>

**Wixpack by Blitz** · ship fast · stay modular · stay sharp

<br/>

`v0.9+` · Blitz · [@blitzlabx](https://t.me/blitzlabx)

</div>
