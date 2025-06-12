import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

// Copy these types from your Models.cs or create a types file
interface WorkingMemoryItem {
  id: string;
  name: string;
  chunkType: string;
  activationLevel: number;
  subsystem: string;
}

interface WorkingMemoryChangedEvent {
  workingMemoryItems: WorkingMemoryItem[];
  primedChunks: WorkingMemoryItem[];
  timestamp: string;
}

interface MessageReceivedEvent {
  userInput: string;
  chunkId: string;
  chunkName: string;
  intentType: string;
  timestamp: string;
}

interface ContextUpdatedEvent {
  contextSnapshot: Record<string, Record<string, unknown>>;
  contextSummary: string;
  categoryCount: number;
  timestamp: string;
}

export class SignalRService {
  private connection: HubConnection;

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/rainahub") // Adjust to your backend URL
      .build();
  }

  async start(): Promise<void> {
    try {
      await this.connection.start();
      console.log("SignalR Connected");
    } catch (err) {
      console.error("SignalR Connection Error: ", err);
    }
  }

  onMessageReceived(callback: (data: MessageReceivedEvent) => void): void {
    this.connection.on("MessageReceived", callback);
  }

  onWorkingMemoryChanged(callback: (data: WorkingMemoryChangedEvent) => void): void {
    this.connection.on("WorkingMemoryChanged", callback);
  }

  onContextUpdated(callback: (data: ContextUpdatedEvent) => void): void {
    this.connection.on("ContextUpdated", callback);
  }

  async stop(): Promise<void> {
    await this.connection.stop();
  }
}
