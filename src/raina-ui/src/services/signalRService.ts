import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import type { DebugLog, EntityInfo } from "../models/models";

// Copy these types from your Models.cs or create a types file
interface SummariesLoadedEvent {
  summaryData: Record<
    number,
    {
      currentTokens: number;
      maxTokens: number;
      items: SummaryInfo[];
    }
  >;
  timestamp: string;
}

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

interface IntentClassifiedEvent {
  intentType: string;
  confidence: number;
  userInput: string;
  entities: EntityInfo[];
  timestamp: string;
}

interface EntitiesExtractedEvent {
  intentEntities: EntityInfo[];
  extractedEntities: EntityInfo[];
  userInput: string;
  timestamp: string;
}

interface ResponseGeneratedEvent {
  responseText: string;
  chunkId: string;
  chunkName: string;
  timestamp: string;
}

interface SummaryInfo {
  id: string;
  text: string;
  depth: number;
  tokenCount: number;
  isSummary: boolean;
  createdAt: string;
}

interface SummaryCreatedEvent {
  newSummaries: SummaryInfo[];
  userInput: string;
  timestamp: string;
}
export class SignalRService {
  private connection: HubConnection;
  private isListenersSetup = false;

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5299/rainahub") // Adjust to your backend URL
      .withAutomaticReconnect() // Add this
      .configureLogging("Debug") // Add this for more logging
      .build();

    // const origOn = this.connection.on.bind(this.connection);

    // this.connection.on = (methodName, newCallback) => {
    //   const wrappedCallback = (...args: unknown[]) => {
    //     console.log(`[SignalR] Event: ${methodName}`, ...args);
    //     newCallback(...args);
    //   };
    //   origOn(methodName, wrappedCallback);
    //   return this.connection;
    // };
    this.connection.onreconnecting(() => {
      console.log("🔄 SignalR: Reconnecting...");
      this.isListenersSetup = false;
    });
    this.connection.onreconnected(() => {
      console.log("✅ SignalR: Reconnected");
      this.setupAllListeners(); // Re-setup listeners on reconnect
    });

    this.connection.onclose((error) => {
      console.log("❌ SignalR: Connection closed", error);
      this.isListenersSetup = false;
    });
    this.connection.on = new Proxy(this.connection.on.bind(this.connection), {
      apply: (target, thisArg, args) => {
        const [methodName, callback] = args;
        const wrappedCallback = (...eventArgs: unknown[]) => {
          console.log(`📨 [SignalR Event] ${methodName}:`, eventArgs);
          callback(...eventArgs);
        };
        return target.call(thisArg, methodName, wrappedCallback);
      },
    });
  }
  private setupAllListeners(): void {
    if (this.isListenersSetup) {
      console.log("⚠️ Listeners already setup, skipping");
      return;
    }

    console.log("🎧 Setting up SignalR event listeners...");

    // Clear any existing listeners first
    this.removeAllListeners();

    // List all the events we're expecting
    const expectedEvents = [
      "WorkingMemoryChanged",
      "ContextUpdated",
      "IntentClassified",
      "EntitiesExtracted",
      "ResponseGenerated",
      "SummaryCreated",
      "GraphUpdated",
      "SystemStatus",
      "Test",
      "SummariesLoaded",
      "ReceiveLogMessage",
    ];

    expectedEvents.forEach((eventName) => {
      console.log(`🎧 Registering listener for: ${eventName}`);
    });

    this.isListenersSetup = true;
    console.log("✅ All SignalR listeners registered");
  }
  async start(): Promise<void> {
    if (this.connection.state === "Disconnected") {
      try {
        console.log("🚀 Starting SignalR connection...");
        await this.connection.start();

        console.log("✅ SignalR Connected!");
        console.log("   Connection ID:", this.connection.connectionId);
        console.log("   Connection State:", this.connection.state);

        // Wait a moment for connection to stabilize
        await new Promise((resolve) => setTimeout(resolve, 100));

        this.setupAllListeners();

        // Test the connection with a ping if your backend supports it
        try {
          await this.connection.invoke("Ping");
          console.log("✅ SignalR: Ping successful");
        } catch (pingError) {
          console.log("⚠️ SignalR: Ping failed (method might not exist):", pingError);
        }
      } catch (err) {
        console.error("❌ SignalR Connection Error:", err);
        throw err;
      }
    } else {
      console.log("ℹ️ SignalR already connected, state:", this.connection.state);
    }
  }

  async stop(): Promise<void> {
    if (this.connection.state === "Connected") {
      this.removeAllListeners(); // Clean up listeners first
      await this.connection.stop();
    }
  }
  async requestSummaries(): Promise<void> {
    if (this.connection.state === "Connected") {
      try {
        await this.connection.invoke("RequestSummaries");
      } catch (err) {
        console.error("Error requesting summaries:", err);
      }
    }
  }
  onLogMessage(callback: (data: DebugLog) => void): void {
    this.connection.on("ReceiveLogMessage", callback);
  }
  onSummariesLoaded(callback: (data: SummariesLoadedEvent) => void): void {
    this.connection.on("SummariesLoaded", callback);
  }
  onMessageReceived(callback: (data: MessageReceivedEvent) => void): void {
    console.log("Message Received callback registered?");

    this.connection.on("MessageReceived", callback);
  }

  onWorkingMemoryChanged(callback: (data: WorkingMemoryChangedEvent) => void): void {
    this.connection.on("WorkingMemoryChanged", callback);
  }

  onContextUpdated(callback: (data: ContextUpdatedEvent) => void): void {
    this.connection.on("ContextUpdated", callback);
  }
  onIntentClassified(callback: (data: IntentClassifiedEvent) => void): void {
    this.connection.on("IntentClassified", callback);
  }

  onEntitiesExtracted(callback: (data: EntitiesExtractedEvent) => void): void {
    console.log("Entities Extracted: ");
    this.connection.on("EntitiesExtracted", callback);
  }

  onResponseGenerated(callback: (data: ResponseGeneratedEvent) => void): void {
    this.connection.on("ResponseGenerated", callback);
  }

  onSummaryCreated(callback: (data: SummaryCreatedEvent) => void): void {
    this.connection.on("SummaryCreated", callback);
  }
  onGraphUpdated(callback: (graphJson: string) => void): void {
    this.connection.on("GraphUpdated", callback);
  }
  onSystemStatus(callback: (data: string) => void): void {
    this.connection.on("SystemStatus", callback);
  }
  onTest(callback: (data: string) => void): void {
    this.connection.on("Test", callback);
  }
  removeAllListeners(): void {
    console.log("🧹 Removing all SignalR listeners");
    this.connection.off("WorkingMemoryChanged");
    this.connection.off("ContextUpdated");
    this.connection.off("IntentClassified");
    this.connection.off("EntitiesExtracted");
    this.connection.off("ResponseGenerated");
    this.connection.off("SummaryCreated");
    this.connection.off("GraphUpdated");
    this.connection.off("SystemStatus");
    this.connection.off("Test");
    this.connection.off("SummariesLoaded");
    this.connection.off("ReceiveLogMessage");
    this.isListenersSetup = false;
  }
}
