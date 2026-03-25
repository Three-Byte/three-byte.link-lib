# 🔋 ThreeByte.LinkLib.NetBooter

**Client library for controlling Synaccess NetBooter networked power controllers — toggle outlets, poll state, and monitor power remotely.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.NetBooter?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.NetBooter)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔋 **Outlet Control** | Turn individual power outlets on and off programmatically |
| 📊 **State Polling** | Query all outlet states in a single call |
| 🔔 **Property Change Notifications** | Implements `INotifyPropertyChanged` for UI data binding |
| 🌐 **HTTP-Based** | Communicates via the NetBooter's built-in web API |
| ⚠️ **Error Events** | Event-driven error reporting |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.NetBooter
```

or via the NuGet Package Manager:

```
Install-Package ThreeByte.LinkLib.NetBooter
```

---

## 🚀 Quick Start

```csharp
using ThreeByte.LinkLib.NetBooter;

// Connect to a NetBooter device
var netBooter = new NetBooterLink("192.168.1.10");

// Subscribe to errors
netBooter.ErrorOccurred += (s, ex) =>
    Console.WriteLine($"Error: {ex.Message}");

// Turn on outlet 1
netBooter.Power(outlet: 1, state: true);

// Turn off outlet 3
netBooter.Power(outlet: 3, state: false);

// Poll all outlet states from the device
netBooter.PollState();

// Check individual outlet states
bool outlet1 = netBooter[1];  // true = ON
bool outlet2 = netBooter[2];  // false = OFF
Console.WriteLine($"Outlet 1: {(outlet1 ? "ON" : "OFF")}");
Console.WriteLine($"Outlet 2: {(outlet2 ? "ON" : "OFF")}");
```

### WPF / MVVM Data Binding

`NetBooterLink` implements `INotifyPropertyChanged`, making it easy to bind to UI:

```xml
<!-- XAML -->
<StackPanel DataContext="{Binding NetBooter}">
    <TextBlock Text="{Binding [1], StringFormat='Outlet 1: {0}'}" />
    <TextBlock Text="{Binding [2], StringFormat='Outlet 2: {0}'}" />
    <Button Content="Refresh" Command="{Binding PollCommand}" />
</StackPanel>
```

---

## 📖 API Reference

### `NetBooterLink`

#### Constructor

```csharp
NetBooterLink(string ipAddress)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `this[int port]` | `bool` | Indexer — get the power state of outlet N (`true` = ON) |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Power(int outlet, bool state)` | `void` | Set outlet power: `true` = ON, `false` = OFF |
| `PollState()` | `void` | Query all outlet states from the device |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `PropertyChanged` | `PropertyChangedEventArgs` | Fires when an outlet state changes (INotifyPropertyChanged) |
| `ErrorOccurred` | `Exception` | Fires on communication errors |

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────┐
│              NetBooterLink                    │
│                                              │
│  Power(outlet, state) ─────┐                 │
│                             ▼                │
│                  ┌─────────────────────┐     │
│                  │  HTTP Commands      │     │
│                  │                     │     │
│                  │  SET:  $A3 {o} {s}  │     │
│                  │  POLL: $A5          │     │
│                  └────────┬────────────┘     │
│                           │                  │
│                           ▼                  │
│                  ┌─────────────────────┐     │
│                  │  NetBooter Device   │     │
│                  │  http://{ip}/       │     │
│                  │  cmd.cgi            │     │
│                  └─────────────────────┘     │
│                           │                  │
│  PollState() ─────────────┘                  │
│       │                                      │
│       ▼                                      │
│  ┌─────────────────────────────────────────┐ │
│  │  Outlet State Dictionary                │ │
│  │  { 1: ON, 2: OFF, 3: ON, ... }         │ │
│  └─────────────────────────────────────────┘ │
│       │                                      │
│       ▼                                      │
│  PropertyChanged events → UI binding         │
└──────────────────────────────────────────────┘
```

---

## 🔧 How It Works

The library communicates with Synaccess NetBooter devices via their HTTP CGI interface:

| Operation | HTTP Request | Example |
|-----------|-------------|---------|
| **Power On** outlet 1 | `GET /cmd.cgi?$A3 1 1` | Turns on outlet 1 |
| **Power Off** outlet 2 | `GET /cmd.cgi?$A3 2 0` | Turns off outlet 2 |
| **Poll State** | `GET /cmd.cgi?$A5` | Returns binary string of all outlet states |

### State Response Format

The device returns a binary string where each bit (right-to-left) represents an outlet:

```
Response: "1101"
           ||||
           |||└─ Outlet 1: ON
           ||└── Outlet 2: OFF
           |└─── Outlet 3: ON
           └──── Outlet 4: ON
```

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
