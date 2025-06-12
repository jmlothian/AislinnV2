export interface Message {
  id: number;
  type: "user" | "assistant";
  text: string;
  timestamp: string;
}

export interface WorkingMemoryChunk {
  id: string;
  name: string;
  type: string;
  activation: number;
  subsystem: string;
}

export interface ContextData {
  environment: {
    currentTime: string;
    location: string;
  };
  social: {
    currentSpeaker: string;
    conversationTopic: string;
    relationshipType: string;
  };
  task: {
    primaryActivity: string;
    currentGoal: string;
    urgency: string;
  };
  temporal: {
    timeOfDay: string;
    upcomingEvents: string;
  };
}

export interface Entity {
  name: string;
  type: string;
}

export interface SummaryItem {
  id: string;
  text: string;
  tokens: number;
  timestamp: string;
  chunkId: string;
}

export interface SummaryDepthData {
  currentTokens: number;
  maxTokens: number;
  items: SummaryItem[];
}

export interface DebugLog {
  id: number;
  timestamp: string;
  level: "INFO" | "DEBUG" | "ERROR";
  category: string;
  message: string;
}

export interface Tab {
  id: string;
  name: string;
  icon: React.ComponentType<{ size?: number }>;
}

export interface Intent {
  type: string;
  confidence: number;
}
