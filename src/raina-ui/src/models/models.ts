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
export interface EntityInfo {
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
  id: string;
  timestamp: string;
  level: "INFO" | "DEBUG" | "ERROR" | "WARNING";
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

// Define types for the graph data
export interface SlotValue {
  value: string | number | boolean | object;
}
export interface ActivationHistoryItem {
  sequenceNumber?: number;
  formattedDate?: string;
  previousValue?: number;
  newValue?: number;
  change?: number;
  activationReason?: string;
  activationSource?: string;
  activatedByChunk?: string;
  activatedBy?: string[];
}
export interface GraphNode {
  id: string;
  label?: string;
  chunkType?: string;
  cognitiveCategory?: string;
  semanticType?: string;
  activationLevel?: number;
  isWorkingMemory?: boolean;
  slots?: Record<string, SlotValue>;
  activationHistory?: ActivationHistoryItem[];
  x?: number;
  y?: number;
  size?: number;
  color?: string;
  // For animation and layout
  originalSize?: number;
  originalColor?: string;
  [key: string]: unknown;
}

export interface GraphEdge {
  id?: string;
  source: string;
  target: string;
  label?: string;
  relationAtoB?: string;
  relationBtoA?: string;
  weightAtoB?: number;
  weightBtoA?: number;
  // For animation and layout
  size?: number;
  color?: string;
  originalSize?: number;
  originalColor?: string;
  [key: string]: unknown;
}

export interface GraphData {
  nodes: GraphNode[];
  edges: GraphEdge[];
}
