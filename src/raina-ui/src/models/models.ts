export interface Message {
  id: string;
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
  [key: string]: { [key: string]: string };
}

export interface Entity {
  name: string;
  type: string;
  formal: string;
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
export interface LoadingState {
  isLoading: boolean;
  message?: string;
}
export interface Intent {
  type: string;
  confidence: number;
}
export interface ChatTabProps {
  chatInput: string;
  setChatInput: (value: string) => void;
  messages: Message[];
  setMessages: (fn: (prev: Message[]) => Message[]) => void;
  messageCount: number;
  setMessageCount: (n: number) => void;
  currentIntent: Intent;
  username: string;
}
export interface AuthState {
  username: string;
  authenticated: boolean;
  loginTime: string;
}
