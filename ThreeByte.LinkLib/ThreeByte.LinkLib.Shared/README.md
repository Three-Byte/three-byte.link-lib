# 🧩 ThreeByte.LinkLib.Shared

**Shared logging infrastructure for all ThreeByte.LinkLib packages — consistent, standardized console logging via `Microsoft.Extensions.Logging`.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.Shared?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.Shared)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🧩 **Shared Foundation** | Common utilities used by all LinkLib packages |
| 📋 **Structured Logging** | Built on `Microsoft.Extensions.Logging` for standardized output |
| 🖥️ **Console Output** | Pre-configured console logging provider |
| 🏭 **Logger Factory** | Simple `LogFactory.Create<T>()` API for creating typed loggers |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.Shared
```

> **Note:** This package is automatically included as a dependency when you install any other ThreeByte.LinkLib package (TcpLink, UdpLink, SerialLink, ProjectorLink, or NetBooter).

---

## 🚀 Quick Start

```csharp
using ThreeByte.LinkLib.Shared;

// Create a typed logger for your class
ILogger logger = LogFactory.Create<MyService>();

logger.LogInformation("Service started on port {Port}", 8080);
logger.LogWarning("Connection attempt timed out");
logger.LogError("Failed to send data: {Error}", ex.Message);
```

---

## 📖 API Reference

### `LogFactory`

A static factory that creates pre-configured `ILogger` instances with console output.

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Create<T>()` | `ILogger` | Creates a console logger with the category name of type `T` |

#### Example Output

```
info: MyApp.DeviceController[0]
      Connected to projector at 192.168.1.200
warn: MyApp.DeviceController[0]
      Reconnection attempt #3
```

---

## 🏗️ How It Fits Together

```
┌──────────────────────────────────────────────────┐
│              Your Application                     │
└────────┬──────────┬──────────┬──────────┬────────┘
         │          │          │          │
         ▼          ▼          ▼          ▼
    ┌─────────┐ ┌────────┐ ┌────────┐ ┌─────────┐
    │ TcpLink │ │UdpLink │ │Serial  │ │Projector│
    │         │ │        │ │ Link   │ │  Link   │
    └────┬────┘ └───┬────┘ └───┬────┘ └────┬────┘
         │          │          │           │
         └──────────┴──────┬───┴───────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │   ThreeByte.LinkLib     │
              │       .Shared          │
              │                        │
              │  LogFactory.Create<T>()│
              │         │              │
              │         ▼              │
              │  Microsoft.Extensions  │
              │     .Logging.Console   │
              └─────────────────────────┘
```

---

## 📚 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Extensions.Logging` | 9.0.2 | Core logging abstractions |
| `Microsoft.Extensions.Logging.Abstractions` | 9.0.2 | `ILogger` interface |
| `Microsoft.Extensions.Logging.Console` | 9.0.2 | Console output provider |

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

Part of the [ThreeByte.LinkLib](https://github.com/Three-Byte/three-byte.link-lib) family of communication libraries.
