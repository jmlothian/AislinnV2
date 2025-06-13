import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

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
interface EntityInfo {
  name: string;
  type: string;
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

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5299/rainahub") // Adjust to your backend URL
      .withAutomaticReconnect() // Add this
      .configureLogging("Information") // Add this for more logging
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
  }

  async start(): Promise<void> {
    if (this.connection.state === "Disconnected") {
      try {
        await this.connection.start();
        console.log("SignalR Connected");
      } catch (err) {
        console.error("SignalR Connection Error: ", err);
      }
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
  onSystemStatus(callback: (data: string) => void): void {
    this.connection.on("SystemStatus", callback);
  }
  onTest(callback: (data: string) => void): void {
    this.connection.on("Test", callback);
  }
  removeAllListeners(): void {
    this.connection.off("WorkingMemoryChanged");
    this.connection.off("ContextUpdated");
    this.connection.off("IntentClassified");
    this.connection.off("EntitiesExtracted");
    this.connection.off("ResponseGenerated");
    this.connection.off("SummaryCreated");
    this.connection.off("SystemStatus");
    this.connection.off("Test");
    this.connection.off("SummariesLoaded");
  }
}
