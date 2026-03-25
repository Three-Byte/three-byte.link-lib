# 📡 ThreeByte.LinkLib.UdpLink

**Asynchronous UDP client with message queuing, configurable endpoints, and thread-safe operation.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.UdpLink?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.UdpLink)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 📡 **Connectionless** | Low-latency UDP datagrams — no handshake overhead |
| 📦 **Message Queuing** | Incoming datagrams queued FIFO (up to 100 messages) |
| 🔒 **Thread-Safe** | All operations protected with locking |
| ⚡ **Fully Async** | Non-blocking `BeginSend` / `BeginReceive` patterns |
| 🎛️ **Dual Port Config** | Separate remote and local port configuration |
| 🔔 **Event-Driven** | Events for data arrival, errors, and enable/disable state |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.UdpLink
```

or via the NuGet Package Manager:

```
Install-Package ThreeByte.LinkLib.UdpLink
```

---

## 🚀 Quick Start

```csharp
using ThreeByte.LinkLib.UdpLink;

// Send to a remote device on port 9000, listen on local port 9001
var udp = new AsyncUdpLink("192.168.1.50", remotePort: 9000, localPort: 9001);

// Subscribe to incoming data
udp.DataReceived += (s, e) =>
{
    byte[]? data = udp.GetMessage();
    if (data != null)
        Console.WriteLine($"Received {data.Length} bytes");
};

udp.ErrorOccurred += (s, ex) =>
    Console.WriteLine($"Error: {ex.Message}");

// Send a datagram
byte[] payload = System.Text.Encoding.ASCII.GetBytes("PING");
udp.SendMessage(payload);

// Check for queued data
while (udp.HasData)
{
    byte[]? response = udp.GetMessage();
}

// Clean up
udp.Dispose();
```

### Dynamic Local Port

```csharp
// Let the OS assign a local port (default behavior)
var udp = new AsyncUdpLink("10.0.0.1", remotePort: 8080);
```

---

## 📖 API Reference

### `AsyncUdpLink`

#### Constructors

```csharp
AsyncUdpLink(string address, int remotePort, int localPort = 0, bool enabled = true)
AsyncUdpLink(UdpLinkSettings settings, bool enabled = true)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsEnabled` | `bool` | Whether the link is actively receiving |
| `Address` | `string` | Remote host address |
| `Port` | `int` | Remote port number |
| `HasData` | `bool` | Whether there are queued incoming datagrams |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendMessage(byte[])` | `void` | Sends a byte array datagram to the remote endpoint |
| `GetMessage()` | `byte[]?` | Dequeues the next incoming datagram (FIFO) |
| `SetEnabled(bool)` | `void` | Starts or stops the receive loop |
| `Dispose()` | `void` | Releases the UDP socket and all resources |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `IsEnabledChanged` | `bool` | Fires when enabled state changes |
| `ErrorOccurred` | `Exception` | Fires on communication errors |
| `DataReceived` | `EventArgs` | Fires when a datagram arrives |

---

## 🔧 Configuration

Use `UdpLinkSettings` for structured configuration:

```csharp
// With explicit local port
var settings = new UdpLinkSettings("10.0.0.1", remotePort: 8080, localPort: 8081);

// With dynamic local port (default 0)
var settings = new UdpLinkSettings("10.0.0.1", remotePort: 8080);
```

| Property | Type | Description |
|----------|------|-------------|
| `Address` | `string` | Remote host address |
| `RemotePort` | `int` | Remote port to send to |
| `LocalPort` | `int` | Local port to bind (0 = OS-assigned) |

---

## 🏗️ Architecture

```
┌────────────────────────────────────────────┐
│              AsyncUdpLink                  │
│                                            │
│  ┌──────────────┐    ┌──────────────────┐  │
│  │  Send (Async) │    │ Receive (Async)  │  │
│  │ → Remote:Port │    │ ← Local:Port     │  │
│  └──────┬───────┘    └────────┬─────────┘  │
│         │                     │            │
│         ▼                     ▼            │
│  ┌──────────────────────────────────────┐  │
│  │     Thread-Safe Message Queue        │  │
│  │          (FIFO, max 100)             │  │
│  └──────────────────────────────────────┘  │
│         │                                  │
│         ▼                                  │
│  ┌──────────────────────────────────────┐  │
│  │   Event Dispatch (Data / Error)      │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
```

---

## 💡 TCP vs UDP — When to Use This Package

| Scenario | Use |
|----------|-----|
| Reliable, ordered data transfer | `ThreeByte.LinkLib.TcpLink` |
| Low-latency, fire-and-forget messaging | **`ThreeByte.LinkLib.UdpLink`** ✅ |
| Broadcast / multicast discovery | **`ThreeByte.LinkLib.UdpLink`** ✅ |
| Streaming sensor data | **`ThreeByte.LinkLib.UdpLink`** ✅ |
| Command-response protocols | `ThreeByte.LinkLib.TcpLink` |

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
