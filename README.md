# Concurrent Blazor Upload Simulation

A Blazor WebAssembly application that simulates concurrent file uploads to multiple disk queues, demonstrating priority-based queue management and reactive programming patterns.

## Features

- **Real-time visualization** of upload progress across multiple disk queues
- **Priority-based client queue** that automatically adjusts based on wait time
- **Reactive programming** using System.Reactive (Rx.NET)
- **Concurrent processing** simulation with multiple upload queues
- **Dark-themed UI** with smooth animations and progress tracking

## Technology Stack

- .NET 9.0
- Blazor WebAssembly
- System.Reactive (Rx.NET) 6.1.0
- C# with nullable reference types enabled
- Tailwind

## How It Works

The application simulates a system where:
- Multiple clients are generated at random intervals
- Each client has a random number of files with varying sizes
- Clients are queued based on a priority score (considering wait time and file count)
- Multiple disk queues process uploads concurrently
- Real-time updates show progress and queue status

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later

### Running the Application

1. Clone the repository:
```bash
git clone <repository-url>
cd Współbieżne-Blazor
```

2. Restore dependencies:
```bash
cd Współbieżne-Blazor
dotnet restore
```

3. Run the application:
```bash
dotnet run
```

4. Open your browser and navigate to the URL shown in the console (typically `https://localhost:5001` or `http://localhost:5000`)

### Usage

- Click **"Rozpocznij symulację"** (Start Simulation) to begin generating clients and processing uploads
- Click **"Zatrzymaj symulację"** (Stop Simulation) to pause client generation
- Watch the real-time updates of:
  - Disk queue progress bars
  - Active client ID per disk
  - Client queue with wait times and priority scores

## Project Structure

- `Simulation.cs` - Manages client generation
- `UploadServer.cs` - Coordinates upload queues and client processing
- `UploadQueue.cs` - Represents individual disk upload queues
- `Client.cs` - Client model with files and priority calculation
- `ClientComparer.cs` - Priority comparison logic for queue ordering
- `Pages/Index.razor` - Main UI component

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
