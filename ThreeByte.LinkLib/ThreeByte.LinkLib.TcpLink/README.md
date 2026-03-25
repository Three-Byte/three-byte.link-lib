# 🌐 ThreeByte.LinkLib.TcpLink

**Asynchronous TCP client with automatic reconnection, message queuing, and thread-safe operation.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.TcpLink?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.TcpLink)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔄 **Auto-Reconnect** | Automatically re-establishes connections with a 3-second retry on failure |
| 📦 **Message Queuing** | Incoming data is queued (FIFO, up to 100 messages) for reliable retrieval |
| 🔒 **Thread-Safe** | All operations are protected with locking for concurrent access |
| ⚡ **Fully Async** | Uses `BeginConnect` / `BeginRead` / `BeginWrite` for non-blocking I/O |
| 🔔 **Event-Driven** | Rich event model for connection changes, data arrival, and errors |
| 🎛️ **Enable/Disable** | Pause and resume communication without destroying the connection |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.TcpLink
```

or via the NuGet Package Manager:

```
Install-Package ThreeByte.LinkLib.TcpLink
```

---

## 🚀 Quick Start

```csharp
using ThreeByte.LinkLib.TcpLink;

// Create a TCP link to a remote device
var tcp = new AsyncTcpLink("192.168.1.100", 5000);

// Subscribe to events
tcp.IsConnectedChanged += (s, connected) =>
    Console.WriteLine(connected ? "✅ Connected" : "❌ Disconnected");

tcp.DataReceived += (s, e) =>
    Console.WriteLine($"Data arrived: {tcp.GetMessage()?.Length} bytes");

tcp.ErrorOccurred += (s, ex) =>
    Console.WriteLine($"Error: {ex.Message}");

// Send a message
byte[] command = System.Text.Encoding.ASCII.GetBytes("HELLO\r\n");
tcp.SendMessage(command);

// Retrieve queued incoming data
if (tcp.HasData)
{
    byte[]? response = tcp.GetMessage();
}

// Clean up
tcp.Dispose();
```

---

## 📖 API Reference

### `AsyncTcpLink`

#### Constructors

```csharp
AsyncTcpLink(string address, int port)
AsyncTcpLink(string address, int port, bool enabled = true)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | `bool` | Whether the TCP socket is currently connected |
| `IsEnabled` | `bool` | Whether messaging is active |
| `Address` | `string` | Remote host address |
| `Port` | `int` | Remote host port |
| `HasData` | `bool` | Whether there are queued incoming messages |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendMessage(byte[])` | `void` | Sends a byte array to the remote host |
| `GetMessage()` | `byte[]?` | Dequeues and returns the next incoming message (FIFO) |
| `SetEnabled(bool)` | `void` | Enables or disables communication |
| `Dispose()` | `void` | Releases all resources and closes the connection |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `IsConnectedChanged` | `bool` | Fires when connection state changes |
| `IsEnabledChanged` | `bool` | Fires when enabled state changes |
| `ErrorOccurred` | `Exception` | Fires on communication errors |
| `DataReceived` | `EventArgs` | Fires when new data arrives |

---

## 🔧 Configuration

Use `TcpLinkSettings` for structured configuration:

```csharp
var settings = new TcpLinkSettings("192.168.1.100", 5000);
// Access settings.Address, settings.Port
```

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────┐
│              AsyncTcpLink                   │
│                                             │
│  ┌───────────┐  ┌───────────┐  ┌─────────┐ │
│  │  Connect   │  │   Read    │  │  Write  │ │
│  │  (Async)   │  │  (Async)  │  │ (Async) │ │
│  └─────┬─────┘  └─────┬─────┘  └────┬────┘ │
│        │              │              │      │
│        ▼              ▼              ▼      │
│  ┌─────────────────────────────────────────┐│
│  │         Thread-Safe Message Queue       ││
│  │            (FIFO, max 100)              ││
│  └─────────────────────────────────────────┘│
│        │                                    │
│        ▼                                    │
│  ┌─────────────────────────────────────────┐│
│  │     Auto-Reconnect Timer (3 sec)        ││
│  └─────────────────────────────────────────┘│
└─────────────────────────────────────────────┘
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

Part of the [ThreeByte.LinkLib](https://github.com/olaafrossi/three-byte.link-lib) family of communication libraries.
