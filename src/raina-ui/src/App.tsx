import { useState, useEffect } from "react";
import {
  Search,
  Play,
  Pause,
  Trash2,
  Settings,
  MessageCircle,
  Brain,
  BarChart,
  Terminal,
  Eye,
  Users,
  FileText,
  ChevronDown,
  ChevronRight,
  LogOut,
  RefreshCw,
} from "lucide-react";
import { SignalRService } from "./services/signalRService";
import type {
  ContextData,
  DebugLog,
  Entity,
  Intent,
  Message,
  SummaryDepthData,
  SummaryItem,
  Tab,
  WorkingMemoryChunk,
  AuthState,
  LoadingState,
} from "./models/models";
import { ChatTab } from "./ChatTab";
import { LoginScreen } from "./LoginScreen";
import { LoadingSpinner } from "./LoadingSpinner"; // or wherever you put it

const RainaUI = () => {
  const generateId = (): string => {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0;
      const v = c == "x" ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  };
  // Mock data
  const mockCurrentIntent: Intent = {
    type: "PlanningAssistance",
    confidence: 0.89,
  };

  const mockMessages: Message[] = [];

  const mockDebugLogs: DebugLog[] = [
    {
      id: 1,
      timestamp: "14:35:23.456",
      level: "INFO",
      category: "WorkingMemory",
      message: "Added chunk: Current Conversation (activation: 0.95)",
    },
    {
      id: 2,
      timestamp: "14:35:23.467",
      level: "DEBUG",
      category: "ContextContainer",
      message: "Updated context factor: Task.PrimaryActivity = Conversation",
    },
    {
      id: 3,
      timestamp: "14:35:23.478",
      level: "INFO",
      category: "SpreadingActivation",
      message: "Activated chunk: User Profile: John (boost: 0.12)",
    },
    {
      id: 4,
      timestamp: "14:35:23.489",
      level: "DEBUG",
      category: "IntentProcessor",
      message: "Classified intent: PlanningAssistance (confidence: 0.89)",
    },
    { id: 5, timestamp: "14:35:23.501", level: "INFO", category: "ConversationManager", message: "Generated response (token count: 156)" },
  ];

  const tabs: Tab[] = [
    { id: "chat", name: "Chat", icon: MessageCircle },
    { id: "memory", name: "Memory", icon: Brain },
    { id: "context", name: "Context", icon: BarChart },
    { id: "entities", name: "Entities", icon: Users },
    { id: "summaries", name: "Summaries", icon: FileText },
    { id: "query", name: "Query", icon: Search },
    { id: "debug", name: "Debug", icon: Terminal },
    { id: "viz", name: "Viz", icon: Eye },
  ];

  const [summariesLoading, setSummariesLoading] = useState<LoadingState>({ isLoading: false });
  const [summariesRequested, setSummariesRequested] = useState<boolean>(false);
  const [chatInput, setChatInput] = useState<string>("");
  const [queryInput, setQueryInput] = useState<string>("");
  const [isDebugPaused, setIsDebugPaused] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<string>("chat");
  const [expandedDepths, setExpandedDepths] = useState<Record<number, boolean>>({ 0: true, 1: false, 2: false, 3: false });
  // Inside your RainaUI component, add these state variables and useEffect:
  const [signalRService] = useState(() => new SignalRService());
  const [isConnected, setIsConnected] = useState<boolean>(false);
  const [messageCount, setMessageCount] = useState<number>(1);
  const [messages, setMessages] = useState<Message[]>(mockMessages);
  const [currentIntent, setCurrentIntent] = useState<Intent>(mockCurrentIntent);
  const [intentEntities, setIntentEntities] = useState<Entity[]>([]);
  const [extractedEntities, setExtractedEntities] = useState<Entity[]>([]);
  const [workingMemory, setWorkingMemory] = useState<WorkingMemoryChunk[]>([]);
  const [context, setContext] = useState<ContextData>({
    environment: {},
    social: {},
    task: {},
  });
  const [summaryData, setSummaryData] = useState<Record<number, SummaryDepthData>>({});
  const [authState, setAuthState] = useState<AuthState | null>(null);

  // Chat history persistence functions
  const saveChatHistory = (messages: Message[]) => {
    try {
      localStorage.setItem("raina_chat_history", JSON.stringify(messages));
    } catch (error) {
      console.error("Error saving chat history:", error);
    }
  };

  const loadChatHistory = (): Message[] => {
    try {
      const saved = localStorage.getItem("raina_chat_history");
      return saved ? JSON.parse(saved) : [];
    } catch (error) {
      console.error("Error loading chat history:", error);
      return [];
    }
  };

  const parseSummaryToMessages = (summaryData: Record<number, SummaryDepthData>): Message[] => {
    const messages: Message[] = [];

    Object.keys(summaryData)
      .map(Number)
      .sort((a, b) => a - b)
      .forEach((depth) => {
        summaryData[depth].items.forEach((item) => {
          const parts = item.text.split(":");
          if (parts.length > 3) {
            // Get everything after the 3rd colon, rejoin with colons
            const cleanText = parts.slice(3).join(":").trim();

            // Check if the username (before 3rd colon) contains "Raina"
            const usernameSection = parts.slice(0, 3).join(":");
            const isAssistant = usernameSection.includes("Raina");
            if (item.chunkId == "00000000-0000-0000-0000-000000000000") {
              item.chunkId = generateId();
            }

            messages.push({
              id: item.chunkId,
              type: isAssistant ? "assistant" : "user",
              text: cleanText,
              timestamp: item.timestamp,
            });
          }
        });
      });

    return messages;
  };
  useEffect(() => {
    if (messages.length > 0) {
      saveChatHistory(messages);
    }
  }, [messages]);
  console.log(summariesLoading);
  // Add this useEffect to establish connection and set up listeners
  useEffect(() => {
    const savedAuth = sessionStorage.getItem("raina_auth");
    if (savedAuth) {
      try {
        const auth = JSON.parse(savedAuth) as AuthState;
        if (auth.authenticated) {
          setAuthState(auth);
        }
      } catch (error) {
        console.error("Error parsing saved auth:", error);
        sessionStorage.removeItem("raina_auth");
      }
    }
  }, [authState?.authenticated]);
  useEffect(() => {
    const connectSignalR = async () => {
      const savedMessages = loadChatHistory();
      if (savedMessages.length > 0) {
        setMessages(savedMessages);
        console.log(`Loaded ${savedMessages.length} messages from local storage`);
      }

      await signalRService.start();
      setIsConnected(true);
      console.log(isConnected);
      // Set up event listeners
      signalRService.onWorkingMemoryChanged((data) => {
        console.log("Working Memory Updated:", data);
        const newWorkingMemory: WorkingMemoryChunk[] = data.workingMemoryItems.map((item) => ({
          id: item.id,
          name: item.name,
          type: item.chunkType,
          activation: item.activationLevel,
          subsystem: item.subsystem,
        }));
        setWorkingMemory(newWorkingMemory);
      });

      // Context Updates
      signalRService.onContextUpdated((data) => {
        console.log("Context Updated:", data);
        // Convert the context snapshot to your ContextData format
        // This is a simplified conversion - you may need to adjust based on your actual data structure
        const newContext: ContextData = {
          environment: {
            currentTime: (data.contextSnapshot.Temporal?.currentTime as string) || context.environment.currentTime,
            location: (data.contextSnapshot.Environment?.location as string) || context.environment.location,
          },
          social: {
            currentSpeaker: (data.contextSnapshot.Social?.currentSpeaker as string) || context.social.currentSpeaker,
            conversationTopic: (data.contextSnapshot.Social?.conversationTopic as string) || context.social.conversationTopic,
            relationshipType: (data.contextSnapshot.Social?.relationshipType as string) || context.social.relationshipType,
          },
          task: {
            primaryActivity: (data.contextSnapshot.Task?.primaryActivity as string) || context.task.primaryActivity,
            currentGoal: (data.contextSnapshot.Task?.currentGoal as string) || context.task.currentGoal,
            urgency: (data.contextSnapshot.Task?.urgency as string) || context.task.urgency,
          },
          temporal: {
            timeOfDay: (data.contextSnapshot.Temporal?.timeOfDay as string) || context.temporal.timeOfDay,
            upcomingEvents: (data.contextSnapshot.Temporal?.upcomingEvents as string) || context.temporal.upcomingEvents,
          },
        };
        setContext(newContext);
      });

      // Message Received
      //we can use this to update the processing status of a message to mark it as "seen" essentially
      // signalRService.onMessageReceived((data) => {
      //   console.log("Message Received:", data);
      //   const newMessage: Message = {
      //     id: Date.now(), // Simple ID generation
      //     type: "user",
      //     text: data.userInput,
      //     timestamp: new Date(data.timestamp).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
      //   };
      //   setMessages((prev) => [...prev, newMessage]);
      // });

      // Intent Classification
      signalRService.onIntentClassified((data) => {
        console.log("Intent Classified:", data);
        console.log(`Intent: ${data.intentType} (${(data.confidence * 100).toFixed(1)}%)`);

        setCurrentIntent({
          type: data.intentType,
          confidence: data.confidence,
        });

        const newIntentEntities: Entity[] = data.entities.map((entity) => ({
          name: entity.name,
          type: entity.type,
        }));
        setIntentEntities(newIntentEntities);
      });

      // Entities Extracted
      signalRService.onEntitiesExtracted((data) => {
        console.log("Entities Extracted:", data);
        console.log(`Intent entities: ${data.intentEntities.length}, Extracted: ${data.extractedEntities.length}`);

        const newExtractedEntities: Entity[] = data.extractedEntities.map((entity) => ({
          name: entity.name,
          type: entity.type,
        }));
        setExtractedEntities(newExtractedEntities);
      });

      // Response Generated
      signalRService.onResponseGenerated((data) => {
        console.log("Response Generated:", data);
        console.log(`Response: ${data.responseText}`);
        setMessageCount(messageCount + 1);
        const newMessage: Message = {
          id: data.chunkId, // Simple ID generation, +1 to avoid collision
          type: "assistant",
          text: data.responseText,
          timestamp: new Date(data.timestamp).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
        };
        setMessages((prev) => [...prev, newMessage]);
      });
      signalRService.onSystemStatus((data) => {
        console.log("SYSTEMSTATUS");
        console.log(data);
      });
      signalRService.onTest((data) => {
        console.log("Test");
        console.log(data);
      });
      // Summary Created
      signalRService.onSummaryCreated((data) => {
        console.log("Summary Created:", data);
        console.log(`Created ${data.newSummaries.length} new summaries`);

        // Update summary data with new summaries
        setSummaryData((prev) => {
          const newSummaryData = { ...prev };

          data.newSummaries.forEach((summary) => {
            const depth = summary.depth;

            if (!newSummaryData[depth]) {
              newSummaryData[depth] = {
                currentTokens: 0,
                maxTokens: 8000,
                items: [],
              };
            }

            const newSummaryItem: SummaryItem = {
              id: summary.id,
              text: summary.text,
              tokens: summary.tokenCount,
              timestamp: new Date(summary.createdAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
              chunkId: summary.id,
            };

            newSummaryData[depth].items.push(newSummaryItem);
            newSummaryData[depth].currentTokens += summary.tokenCount;
          });

          return newSummaryData;
        });
      });
      signalRService.onSummariesLoaded((data) => {
        console.log("Summaries Loaded:", data);
        setSummariesLoading({ isLoading: false });

        const convertedSummaryData: Record<number, SummaryDepthData> = {};
        Object.entries(data.summaryData).forEach(([depth, depthData]) => {
          convertedSummaryData[Number(depth)] = {
            currentTokens: depthData.currentTokens,
            maxTokens: depthData.maxTokens,
            items: depthData.items.map((item) => ({
              id: item.id,
              text: item.text,
              tokens: item.tokenCount,
              timestamp: new Date(item.createdAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
              chunkId: item.id,
            })),
          };
        });
        setSummaryData(convertedSummaryData);

        if (messages.length === 0) {
          const messagesFromSummary = parseSummaryToMessages(convertedSummaryData);
          setMessages(messagesFromSummary);
          console.log(`Built ${messagesFromSummary.length} messages from summaries`);
        }
      });
    };

    connectSignalR();

    // Cleanup on unmount
    return () => {
      signalRService.stop();
    };
  }, []);

  const handleRequestSummaries = async () => {
    setSummariesLoading({ isLoading: true, message: "Loading summaries..." });
    setSummariesRequested(true);
    await signalRService.requestSummaries();
  };

  useEffect(() => {
    if (activeTab === "summaries" && !summariesRequested && Object.keys(summaryData).length === 0) {
      handleRequestSummaries();
    }
  }, [activeTab, summariesRequested, summaryData]);
  const handleLogin = (username: string) => {
    const auth: AuthState = {
      username,
      authenticated: true,
      loginTime: new Date().toISOString(),
    };
    setAuthState(auth);
  };

  const handleSignOut = () => {
    sessionStorage.removeItem("raina_auth");
    setAuthState(null);
    signalRService.stop();
  };

  // Show login screen if not authenticated
  if (!authState?.authenticated) {
    return <LoginScreen onLogin={handleLogin} />;
  }
  const renderMemoryTab = () => (
    <div className="h-full overflow-y-auto p-4">
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Working Memory</h3>
        <p className="text-sm text-gray-500">{workingMemory.length}/20 slots</p>
      </div>
      <div className="space-y-3">
        {workingMemory.map((chunk) => (
          <div key={chunk.id} className="bg-white border border-gray-200 rounded-lg p-4">
            <div className="flex justify-between items-start mb-2">
              <div className="flex-1 min-w-0">
                <h4 className="font-medium text-sm text-gray-900 truncate">{chunk.name}</h4>
                <p className="text-xs text-gray-500">
                  {chunk.type} • {chunk.subsystem}
                </p>
              </div>
              <div className="text-right ml-3 flex-shrink-0">
                <div className="text-xs font-mono text-gray-600">{chunk.activation.toFixed(2)}</div>
              </div>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${(Math.min(chunk.activation, 60) / 60) * 100}%` }}
              ></div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  const renderContextTab = () => (
    <div className="h-full overflow-y-auto p-4">
      <div className="space-y-6">
        {Object.entries(context).map(([sectionKey, sectionValue]) => {
          if (!sectionValue) return null; // skip null section objects

          return (
            <div key={sectionKey}>
              <h4 className="font-semibold text-base text-gray-800 mb-3 border-b border-gray-200 pb-2 capitalize">{sectionKey}</h4>
              <div className="space-y-2">
                {Object.entries(sectionValue).map(([label, value]) => {
                  if (value == null) return null; // skip null or undefined values
                  return (
                    <div key={label} className="flex justify-between">
                      <span className="text-sm text-gray-600">{label}:</span>
                      <span className="text-sm font-medium text-gray-900">{value}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );

  const renderEntitiesTab = () => (
    <div className="h-full overflow-y-auto p-4">
      <div className="space-y-6">
        {/* Intent Entities */}
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-3 border-b border-gray-200 pb-2">Intent Entities</h3>
          <div className="space-y-2">
            {intentEntities.map((entity, index) => (
              <div key={index} className="bg-white border border-gray-200 rounded-lg p-3">
                <div className="flex justify-between items-center">
                  <span className="font-medium text-gray-900">{entity.name}</span>
                  <span className="text-sm text-gray-500 bg-gray-100 px-2 py-1 rounded">{entity.type}</span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Extracted Entities */}
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-3 border-b border-gray-200 pb-2">Extracted Entities</h3>
          <div className="space-y-2">
            {extractedEntities.map((entity, index) => (
              <div key={index} className="bg-white border border-gray-200 rounded-lg p-3">
                <div className="flex justify-between items-center">
                  <span className="font-medium text-gray-900">{entity.name}</span>
                  <span className="text-sm text-gray-500 bg-gray-100 px-2 py-1 rounded">{entity.type}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );

  const toggleDepth = (depth: number): void => {
    setExpandedDepths((prev) => ({
      ...prev,
      [depth]: !prev[depth],
    }));
  };

  const renderSummariesTab = () => {
    // Show loading spinner if summaries are loading and no data exists
    if (summariesLoading.isLoading && Object.keys(summaryData).length === 0) {
      return (
        <div className="h-full flex items-center justify-center">
          <LoadingSpinner message={summariesLoading.message} size="lg" />
        </div>
      );
    }

    return (
      <div className="h-full overflow-y-auto p-4">
        {/* Header with refresh button */}
        <div className="mb-4 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-gray-900">Conversation Summaries</h3>
          <button
            onClick={handleRequestSummaries}
            disabled={summariesLoading.isLoading}
            className="flex items-center space-x-2 px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <RefreshCw size={16} className={summariesLoading.isLoading ? "animate-spin" : ""} />
            <span>{summariesLoading.isLoading ? "Loading..." : "Refresh"}</span>
          </button>
        </div>

        {/* Show message if no summaries */}
        {Object.keys(summaryData).length === 0 && !summariesLoading.isLoading && (
          <div className="text-center py-8">
            <FileText className="mx-auto mb-4 text-gray-400" size={48} />
            <div className="text-lg font-medium text-gray-600 mb-2">No summaries available</div>
            <div className="text-sm text-gray-400">Click refresh to load current summaries</div>
          </div>
        )}

        {/* Render depths from highest to lowest (3, 2, 1, 0) */}
        <div className="space-y-4">
          {Object.keys(summaryData)
            .map(Number)
            .sort((a, b) => b - a)
            .map((depth) => {
              const depthData = summaryData[depth];
              const isExpanded = expandedDepths[depth];
              const progressPercent = (depthData.currentTokens / depthData.maxTokens) * 100;

              return (
                <div key={depth} className="space-y-2">
                  {/* Depth Header */}
                  <div
                    className="bg-white border border-gray-200 rounded-lg p-4 cursor-pointer hover:bg-gray-50 transition-colors"
                    onClick={() => toggleDepth(depth)}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-3">
                        {isExpanded ? <ChevronDown size={20} /> : <ChevronRight size={20} />}
                        <h3 className="text-lg font-semibold text-gray-900">Depth {depth}</h3>
                        <span className="text-sm text-gray-500">
                          ({depthData.currentTokens} / {depthData.maxTokens} tokens)
                        </span>
                      </div>
                      <div className="text-sm text-gray-500">{depthData.items.length} items</div>
                    </div>

                    {/* Progress Bar */}
                    <div className="mt-3">
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className={`h-2 rounded-full transition-all duration-300 ${
                            progressPercent > 75 ? "bg-red-500" : progressPercent > 50 ? "bg-yellow-500" : "bg-green-500"
                          }`}
                          style={{ width: `${progressPercent}%` }}
                        ></div>
                      </div>
                    </div>
                  </div>

                  {/* Expanded Items */}
                  {isExpanded && (
                    <div className="ml-6 border-l-2 border-gray-200 pl-4 space-y-3">
                      {depthData.items.map((item) => (
                        <div key={item.id} className="relative">
                          {/* Dot connector */}
                          <div className="absolute -left-6 top-3 w-3 h-3 bg-blue-500 rounded-full border-2 border-white"></div>

                          {/* Item content */}
                          <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
                            <div className="text-sm text-gray-900 mb-2 leading-relaxed">{item.text}</div>
                            <div className="flex justify-between items-center text-xs text-gray-500">
                              <span>{item.tokens} tokens</span>
                              <span>{item.timestamp}</span>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
        </div>
      </div>
    );
  };
  const renderQueryTab = () => (
    <div className="h-full flex flex-col p-4">
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-gray-900 mb-2">Memory Query</h3>
        <div className="flex flex-col sm:flex-row space-y-2 sm:space-y-0 sm:space-x-2">
          <input
            type="text"
            value={queryInput}
            onChange={(e) => setQueryInput(e.target.value)}
            placeholder="ChunkType == 'Goal' AND ActivationLevel > 0.5"
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button className="bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700 flex items-center justify-center">
            <Search size={16} />
          </button>
        </div>
        <div className="text-xs text-gray-500 mt-2">Query syntax: field comparisons with AND/OR operators</div>
      </div>

      <div className="flex-1 bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div className="text-sm text-gray-600">Results will appear here...</div>
      </div>
    </div>
  );

  const renderDebugTab = () => (
    <div className="h-full flex flex-col">
      <div className="border-b border-gray-200 px-4 py-3 flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Debug Logs</h3>
        <div className="flex space-x-2">
          <button
            onClick={() => setIsDebugPaused(!isDebugPaused)}
            className={`p-2 rounded-lg ${isDebugPaused ? "bg-green-100 text-green-600" : "bg-red-100 text-red-600"}`}
          >
            {isDebugPaused ? <Play size={16} /> : <Pause size={16} />}
          </button>
          <button className="p-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200">
            <Trash2 size={16} />
          </button>
          <button className="p-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200">
            <Settings size={16} />
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-4 font-mono text-xs bg-gray-900 text-gray-100">
        <div className="space-y-1">
          {mockDebugLogs.map((log) => (
            <div key={log.id} className="flex flex-wrap gap-2 leading-relaxed">
              <span className="text-gray-400 flex-shrink-0">{log.timestamp}</span>
              <span
                className={`font-semibold flex-shrink-0 ${
                  log.level === "INFO" ? "text-blue-400" : log.level === "DEBUG" ? "text-gray-400" : "text-red-400"
                }`}
              >
                {log.level}
              </span>
              <span className="text-purple-400 flex-shrink-0">[{log.category}]</span>
              <span className="text-gray-100 min-w-0">{log.message}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  const renderVizTab = () => (
    <div className="h-full flex items-center justify-center p-4">
      <div className="text-center">
        <Eye className="mx-auto mb-4 text-gray-400" size={48} />
        <div className="text-lg font-medium text-gray-600 mb-2">Activation Visualization</div>
        <div className="text-sm text-gray-400">To be implemented</div>
      </div>
    </div>
  );

  const renderActiveTab = () => {
    switch (activeTab) {
      case "chat":
        return (
          <ChatTab
            chatInput={chatInput}
            setChatInput={setChatInput}
            messages={messages}
            setMessages={setMessages}
            messageCount={messageCount}
            setMessageCount={setMessageCount}
            currentIntent={currentIntent}
            authState={authState}
          />
        );
      case "memory":
        return renderMemoryTab();
      case "context":
        return renderContextTab();
      case "entities":
        return renderEntitiesTab();
      case "summaries":
        return renderSummariesTab();
      case "query":
        return renderQueryTab();
      case "debug":
        return renderDebugTab();
      case "viz":
        return renderVizTab();
      default:
        return (
          <ChatTab
            chatInput={chatInput}
            setChatInput={setChatInput}
            messages={messages}
            setMessages={setMessages}
            messageCount={messageCount}
            setMessageCount={setMessageCount}
            currentIntent={currentIntent}
            authState={authState}
          />
        );
    }
  };

  return (
    <div className="h-screen bg-gray-50 flex flex-col max-w-full">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl sm:text-2xl font-bold text-gray-900">RAINA</h1>
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-2 text-xs sm:text-sm">
            <span className="text-gray-500 hidden sm:inline">Cognitive Steps: 1,247,893</span>
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span className="text-gray-500">Active</span>
          </div>
          <div className="flex items-center space-x-2">
            <span className="text-sm text-gray-600 hidden sm:inline">Welcome, {authState.username}</span>
            <button
              onClick={handleSignOut}
              className="flex items-center space-x-1 px-3 py-1 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
              title="Sign Out"
            >
              <LogOut size={16} />
              <span className="hidden sm:inline">Sign Out</span>
            </button>
          </div>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="bg-white border-b border-gray-200 px-2 py-2 flex-shrink-0">
        <div className="flex space-x-1 overflow-x-auto">
          {tabs.map((tab) => {
            const IconComponent = tab.icon;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center space-x-2 px-3 py-2 rounded-lg text-sm font-medium whitespace-nowrap transition-colors ${
                  activeTab === tab.id
                    ? "bg-blue-100 text-blue-700 border border-blue-200"
                    : "text-gray-600 hover:text-gray-900 hover:bg-gray-100"
                }`}
              >
                <IconComponent size={16} />
                <span className="hidden sm:inline">{tab.name}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 overflow-hidden">{renderActiveTab()}</div>
    </div>
  );
};

export default RainaUI;
