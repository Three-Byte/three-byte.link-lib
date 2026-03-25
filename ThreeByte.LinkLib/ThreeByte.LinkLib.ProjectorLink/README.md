# 🎬 ThreeByte.LinkLib.ProjectorLink

**PJLink protocol client for controlling projectors over TCP — power on/off, state queries, and device info.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.ProjectorLink?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.ProjectorLink)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🎬 **PJLink 1.0** | Full implementation of the PJLink Class 1 protocol |
| 🔐 **Authentication** | MD5 password authentication when required by projector |
| ⚡ **Power Control** | Turn projectors on and off programmatically |
| 📊 **State Queries** | Query power state: Off, On, Cooling, Warming Up |
| ℹ️ **Device Info** | Retrieve manufacturer, product name, and projector name |
| 🔔 **Error Events** | Event-driven error reporting |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.ProjectorLink
```

or via the NuGet Package Manager:

```
Install-Package ThreeByte.LinkLib.ProjectorLink
```

---

## 🚀 Quick Start

```csharp
using ThreeByte.LinkLib.ProjectorLink;

// Connect to a projector (default PJLink port 4352)
using var projector = new Projector("192.168.1.200");

// Power on
bool success = projector.TurnOn();
Console.WriteLine(success ? "✅ Projector turning on" : "❌ Power on failed");

// Query state
PowerStatus status = projector.GetState();
Console.WriteLine($"Status: {status}");
// Output: Status: WARMUP → ON

// Get device info
string info = projector.GetInfo();
Console.WriteLine($"Projector: {info}");
// Output: Projector: Epson EB-L1755U (Main Hall)

// Power off
projector.TurnOff();
```

### With Authentication

```csharp
// Some projectors require a password
using var projector = new Projector("192.168.1.200", password: "admin123");

projector.ErrorOccurred += (s, ex) =>
    Console.WriteLine($"Projector error: {ex.Message}");

projector.TurnOn();
```

### Custom Port

```csharp
// Non-standard PJLink port
using var projector = new Projector("10.0.0.50", port: 5000, password: "secret");
```

---

## 📖 API Reference

### `Projector`

#### Constructors

```csharp
Projector(string host)                                    // Default port 4352, no auth
Projector(string host, int port)                          // Custom port, no auth
Projector(string host, string password)                   // Default port, with auth
Projector(string host, int port, string password)         // Custom port + auth
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Address` | `string` | Formatted as `"host/port"` |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `TurnOn()` | `bool` | Sends power-on command. Returns `true` on success |
| `TurnOff()` | `bool` | Sends power-off command. Returns `true` on success |
| `GetState()` | `PowerStatus` | Queries the current power state |
| `GetInfo()` | `string` | Returns `"Manufacturer Product (Name)"` |
| `Dispose()` | `void` | Releases resources |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `ErrorOccurred` | `Exception` | Fires on communication or protocol errors |

---

### `PowerStatus` Enum

| Value | Int | Description |
|-------|-----|-------------|
| `OFF` | 0 | Projector is powered off |
| `ON` | 1 | Projector is running normally |
| `COOLING` | 2 | Projector is cooling down after power-off |
| `WARMUP` | 3 | Projector lamp is warming up after power-on |
| `UNKNOWN` | 4 | State could not be determined |

### `CommandResponse` Enum

| Value | Description |
|-------|-------------|
| `SUCCESS` | Command executed successfully |
| `UNDEFINED_CMD` | Projector does not recognize the command |
| `OUT_OF_PARAMETER` | Invalid parameter sent |
| `UNAVAILABLE_TIME` | Command not available in current state |
| `PROJECTOR_FAILURE` | Projector hardware error |
| `AUTH_FAILURE` | Authentication failed |
| `COMMUNICATION_ERROR` | Network or protocol error |

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────┐
│               Projector                  │
│                                          │
│  TurnOn() ──┐                            │
│  TurnOff()──┤  ┌──────────────────────┐  │
│  GetState()─┤  │    PJLink Commands   │  │
│  GetInfo()──┘  │                      │  │
│                │  PowerCommand        │  │
│                │  ManufacturerName     │  │
│                │  ProductName         │  │
│                │  ProjectorName       │  │
│                └──────────┬───────────┘  │
│                           │              │
│                           ▼              │
│                ┌──────────────────────┐   │
│                │  TCP Connection      │   │
│                │  (per-command)       │   │
│                │                      │   │
│                │  ┌────────────────┐  │   │
│                │  │ MD5 Auth       │  │   │
│                │  │ (if required)  │  │   │
│                │  └────────────────┘  │   │
│                └──────────────────────┘   │
└──────────────────────────────────────────┘
```

---

## 📋 PJLink Protocol Reference

This library implements the [PJLink](https://pjlink.jbmia.or.jp/english/) Class 1 specification:

| PJLink Command | Method | Description |
|----------------|--------|-------------|
| `%1POWR 1` | `TurnOn()` | Power on |
| `%1POWR 0` | `TurnOff()` | Power off |
| `%1POWR ?` | `GetState()` | Query power status |
| `%1INF1 ?` | (via `GetInfo()`) | Query manufacturer |
| `%1INF2 ?` | (via `GetInfo()`) | Query product name |
| `%1NAME ?` | (via `GetInfo()`) | Query projector name |

> **Default PJLink port:** TCP 4352

---

## 🎯 Platform Support

| Platform | Supported |
|----------|-----------|
| .NET 10.0 | ✅ |
| .NET Standard 2.1 | ✅ |
| .NET Standard 2.0 | ✅ |
| Windows | ✅ |
| Linux | ✅ |
| macOS | ✅ |

---

## 📄 License

Part of the [ThreeByte.LinkLib](https://github.com/olaafrossi/three-byte.link-lib) family of communication libraries.
