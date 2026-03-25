# 🔌 ThreeByte.LinkLib.SerialLink

**Serial port (RS-232) communication with optional frame-based protocol support, auto-reconnection, and message queuing.**

[![NuGet](https://img.shields.io/nuget/v/ThreeByte.LinkLib.SerialLink?color=blue&logo=nuget)](https://www.nuget.org/packages/ThreeByte.LinkLib.SerialLink)
![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0%20%7C%20Standard%202.1-purple?logo=dotnet)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔌 **RS-232 Support** | Full serial port configuration — baud rate, data bits, parity |
| 📐 **Frame Protocol** | Optional header/footer framing via `FramedSerialLink` |
| 🔄 **Auto-Reconnect** | Automatically recovers from serial port errors |
| 📦 **Message Queuing** | Incoming data queued FIFO (up to 100 messages) |
| 🔒 **Thread-Safe** | All operations protected with locks |
| 🔔 **Event-Driven** | Connection changes, data arrival, and error events |
| 🎛️ **Enable/Disable** | Pause and resume without closing the port |

---

## 📦 Installation

```
dotnet add package ThreeByte.LinkLib.SerialLink
```

or via the NuGet Package Manager:

```
Install-Package ThreeByte.LinkLib.SerialLink
```

---

## 🚀 Quick Start

### Raw Serial Communication

```csharp
using ThreeByte.LinkLib.SerialLink;

// Open a serial port at 9600 baud
var serial = new SerialLink("COM3", baudRate: 9600);

// Subscribe to events
serial.IsConnectedChanged += (s, connected) =>
    Console.WriteLine(connected ? "✅ Port open" : "❌ Port closed");

serial.DataReceived += (s, e) =>
{
    byte[]? data = serial.GetMessage();
    if (data != null)
        Console.WriteLine($"Received {data.Length} bytes");
};

// Send raw bytes
byte[] command = new byte[] { 0x01, 0x02, 0x03 };
serial.SendData(command);

// Check for queued data
while (serial.HasData)
{
    byte[]? response = serial.GetMessage();
}

serial.Dispose();
```

### Framed Serial Communication

Use `FramedSerialLink` when your device protocol uses header/footer delimiters:

```csharp
using ThreeByte.LinkLib.SerialLink;

var framed = new FramedSerialLink("COM4", baudRate: 115200);

// Configure frame delimiters
framed.SendFrame = new SerialFrame
{
    Header = new byte[] { 0x02 },  // STX
    Footer = new byte[] { 0x03 }   // ETX
};

framed.ReceiveFrame = new SerialFrame
{
    Header = new byte[] { 0x02 },
    Footer = new byte[] { 0x03 }
};

// Send a framed message — header/footer added automatically
framed.SendMessage("STATUS?");
// Wire: [0x02] S T A T U S ? [0x03]

// Receive complete framed messages
framed.DataReceived += (s, e) =>
{
    string? response = framed.GetMessage();
    Console.WriteLine($"Device says: {response}");
};

framed.Dispose();
```

---

## 📖 API Reference

### `SerialLink` — Raw Serial Port

#### Constructors

```csharp
SerialLink(string comPort, int baudRate = 9600, int dataBits = 8,
           Parity parity = Parity.None, bool enabled = true)
SerialLink(SerialLinkSettings settings, bool enabled = true)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | `bool` | Whether the serial port is open and active |
| `IsEnabled` | `bool` | Whether messaging is active |
| `IsOpen` | `bool` | Whether the underlying port is open |
| `HasData` | `bool` | Whether there are queued messages |
| `ComPort` | `string` | The COM port name (e.g., `"COM3"`) |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendData(byte[])` | `void` | Sends raw bytes over the serial port |
| `GetMessage()` | `byte[]?` | Dequeues the next incoming byte array (FIFO) |
| `SetEnabled(bool)` | `void` | Enables or disables communication |
| `Dispose()` | `void` | Closes the port and releases resources |

---

### `FramedSerialLink` — Framed Protocol

#### Constructors

```csharp
FramedSerialLink(string comPort, int baudRate = 9600, int dataBits = 8,
                 Parity parity = Parity.None, bool enabled = true)
FramedSerialLink(SerialLinkSettings settings, bool enabled = true)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `SendFrame` | `SerialFrame` | Header/footer config for outgoing messages |
| `ReceiveFrame` | `SerialFrame` | Header/footer config for incoming messages |
| `IsConnected` | `bool` | Whether the underlying serial port is connected |
| `IsEnabled` | `bool` | Whether messaging is active |
| `HasData` | `bool` | Whether there are complete framed messages queued |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `SendMessage(string)` | `void` | Sends a string message wrapped in the configured frame |
| `GetMessage()` | `string?` | Dequeues the next complete framed message (FIFO) |
| `SetEnabled(bool)` | `void` | Enables or disables communication |
| `Dispose()` | `void` | Closes the port and releases resources |

---

### `SerialFrame` — Frame Configuration

```csharp
var frame = new SerialFrame
{
    Header = new byte[] { 0x02 },  // Start-of-text
    Footer = new byte[] { 0x0D, 0x0A }  // CR+LF
};
```

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `byte[]` | Bytes prepended to every outgoing message / expected at start of incoming |
| `Footer` | `byte[]` | Bytes appended to every outgoing message / expected at end of incoming |

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────┐
│                  FramedSerialLink                 │
│  ┌────────────┐           ┌────────────────────┐ │
│  │ SendFrame  │           │   ReceiveFrame     │ │
│  │ [HDR][MSG] │           │ Detect [HDR]..     │ │
│  │     [FTR]  │           │         ..[FTR]    │ │
│  └─────┬──────┘           └────────┬───────────┘ │
│        │                           │             │
│        ▼                           ▼             │
│  ┌──────────────────────────────────────────────┐│
│  │               SerialLink (Raw)               ││
│  │                                              ││
│  │  COM Port ←→ Read/Write ←→ Message Queue     ││
│  │                   ↕                          ││
│  │           Auto-Reconnect Timer               ││
│  └──────────────────────────────────────────────┘│
└──────────────────────────────────────────────────┘
```

---

## 🔧 Configuration

Use `SerialLinkSettings` for structured configuration:

```csharp
var settings = new SerialLinkSettings("COM3", 9600, 8, Parity.None);
var serial = new SerialLink(settings);
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ComPort` | `string` | — | Serial port name (e.g., `"COM3"`, `"/dev/ttyUSB0"`) |
| `BaudRate` | `int` | `9600` | Communication speed |
| `DataBits` | `int` | `8` | Data bits per byte |
| `Parity` | `Parity` | `None` | Parity checking mode |

---

## 🎯 Platform Support

| Platform | Supported |
|----------|-----------|
| .NET 10.0 | ✅ |
| .NET Standard 2.1 | ✅ |
| .NET Standard 2.0 | ✅ |
| Windows (`COM1`..`COM256`) | ✅ |
| Linux (`/dev/ttyUSB0`, `/dev/ttyS0`) | ✅ |
| macOS (`/dev/tty.usbserial`) | ✅ |

---

## 📄 License

Part of the [ThreeByte.LinkLib](https://github.com/Three-Byte/three-byte.link-lib) family of communication libraries.
