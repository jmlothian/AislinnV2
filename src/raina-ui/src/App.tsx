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
} from "./models/models";
import { ChatTab } from "./ChatTab";

const RainaUI = () => {
  // Mock data
  const mockCurrentIntent: Intent = {
    type: "PlanningAssistance",
    confidence: 0.89,
  };

  const mockIntentEntities: Entity[] = [
    { name: "meeting", type: "event" },
    { name: "report", type: "task" },
    { name: "3PM", type: "time" },
  ];

  const mockExtractedEntities: Entity[] = [
    { name: "John", type: "entity.person.instance" },
    { name: "day planning", type: "entity.abstract.goal" },
    { name: "time management", type: "entity.abstract.concept" },
    { name: "office work", type: "entity.abstract.category" },
  ];

  const mockSummaryData: Record<number, SummaryDepthData> = {
    3: {
      currentTokens: 245,
      maxTokens: 8000,
      items: [
        {
          id: "s3-1",
          text: "Comprehensive discussion covering daily planning strategies, time management techniques, and productivity optimization for professional environments.",
          tokens: 245,
          timestamp: "2:30 PM",
          chunkId: "sum-3-1",
        },
      ],
    },
    2: {
      currentTokens: 1456,
      maxTokens: 8000,
      items: [
        {
          id: "s2-1",
          text: "User John requested help with day planning. Discussion included upcoming 3 PM meeting and report completion task. Focus on time management and prioritization.",
          tokens: 567,
          timestamp: "2:32 PM",
          chunkId: "sum-2-1",
        },
        {
          id: "s2-2",
          text: "Conversation about productivity strategies and task organization. Assistant provided guidance on managing concurrent deadlines and meeting preparations.",
          tokens: 445,
          timestamp: "2:33 PM",
          chunkId: "sum-2-2",
        },
        {
          id: "s2-3",
          text: "Follow-up discussion on specific time allocation and task breakdown approaches for optimal workflow management.",
          tokens: 444,
          timestamp: "2:34 PM",
          chunkId: "sum-2-3",
        },
      ],
    },
    1: {
      currentTokens: 3247,
      maxTokens: 8000,
      items: [
        {
          id: "s1-1",
          text: "User John asked for help planning his day, mentioning a 3 PM meeting and report task.",
          tokens: 234,
          timestamp: "2:31 PM",
          chunkId: "sum-1-1",
        },
        {
          id: "s1-2",
          text: "Assistant offered to help with day planning and asked about upcoming activities.",
          tokens: 187,
          timestamp: "2:31 PM",
          chunkId: "sum-1-2",
        },
        {
          id: "s1-3",
          text: "Discussion about 25 minutes available before meeting and report prioritization.",
          tokens: 156,
          timestamp: "2:32 PM",
          chunkId: "sum-1-3",
        },
        {
          id: "s1-4",
          text: "User provided details about meeting timing and report completion requirements.",
          tokens: 198,
          timestamp: "2:32 PM",
          chunkId: "sum-1-4",
        },
        {
          id: "s1-5",
          text: "Assistant suggested time management strategies for report completion before meeting.",
          tokens: 223,
          timestamp: "2:33 PM",
          chunkId: "sum-1-5",
        },
        {
          id: "s1-6",
          text: "Follow-up questions about report complexity and meeting preparation needs.",
          tokens: 178,
          timestamp: "2:33 PM",
          chunkId: "sum-1-6",
        },
        {
          id: "s1-7",
          text: "Discussion of task breakdown and priority assignment methodologies.",
          tokens: 165,
          timestamp: "2:34 PM",
          chunkId: "sum-1-7",
        },
        {
          id: "s1-8",
          text: "User confirmed understanding of suggested approach and timeline feasibility.",
          tokens: 201,
          timestamp: "2:34 PM",
          chunkId: "sum-1-8",
        },
      ],
    },
    0: {
      currentTokens: 5234,
      maxTokens: 8000,
      items: [
        { id: "s0-1", text: "[2:34 PM] John: Hi Raina, can you help me plan my day?", tokens: 134, timestamp: "2:34 PM", chunkId: "utt-1" },
        {
          id: "s0-2",
          text: "[2:34 PM] Raina: Of course! I would be happy to help you plan your day. What do you have coming up?",
          tokens: 178,
          timestamp: "2:34 PM",
          chunkId: "utt-2",
        },
        {
          id: "s0-3",
          text: "[2:35 PM] John: I have a meeting at 3 PM and need to finish a report",
          tokens: 156,
          timestamp: "2:35 PM",
          chunkId: "utt-3",
        },
        {
          id: "s0-4",
          text: "[2:35 PM] Raina: Great! Let me help you organize that. Since it is currently 2:35 PM, you have about 25 minutes before your meeting. Would you like me to help prioritize what you can accomplish with the report in that time?",
          tokens: 287,
          timestamp: "2:35 PM",
          chunkId: "utt-4",
        },
        {
          id: "s0-5",
          text: "[2:31 PM] John: Actually, let me back up - what's the best way to approach this?",
          tokens: 167,
          timestamp: "2:31 PM",
          chunkId: "utt-5",
        },
        {
          id: "s0-6",
          text: "[2:31 PM] Raina: Good question! Let us start by understanding your priorities and constraints.",
          tokens: 156,
          timestamp: "2:31 PM",
          chunkId: "utt-6",
        },
        {
          id: "s0-7",
          text: "[2:32 PM] John: The report is about 70% done, but I need to add conclusions and proofread.",
          tokens: 189,
          timestamp: "2:32 PM",
          chunkId: "utt-7",
        },
        {
          id: "s0-8",
          text: "[2:32 PM] Raina: Perfect! That gives us a clear scope. With 25 minutes, I would suggest focusing on the conclusions first.",
          tokens: 201,
          timestamp: "2:32 PM",
          chunkId: "utt-8",
        },
      ],
    },
  };

  const mockMessages: Message[] = [
    { id: 1, type: "user", text: "Hi Raina, can you help me plan my day?", timestamp: "2:34 PM" },
    {
      id: 2,
      type: "assistant",
      text: "Of course! I'd be happy to help you plan your day. What do you have coming up?",
      timestamp: "2:34 PM",
    },
    { id: 3, type: "user", text: "I have a meeting at 3 PM and need to finish a report", timestamp: "2:35 PM" },
    {
      id: 4,
      type: "assistant",
      text: "Great! Let me help you organize that. Since it's currently 2:35 PM, you have about 25 minutes before your meeting. Would you like me to help prioritize what you can accomplish with the report in that time?",
      timestamp: "2:35 PM",
    },
  ];

  const mockWorkingMemory: WorkingMemoryChunk[] = [
    { id: "chunk-1", name: "Current Conversation", type: "Utterance", activation: 0.95, subsystem: "Episodic" },
    { id: "chunk-2", name: "User Profile: John", type: "Person", activation: 0.87, subsystem: "Semantic" },
    { id: "chunk-3", name: "Task: Plan Day", type: "Goal", activation: 0.82, subsystem: "Procedural" },
    { id: "chunk-4", name: "Meeting at 3 PM", type: "Event", activation: 0.78, subsystem: "Episodic" },
    { id: "chunk-5", name: "Report Task", type: "Task", activation: 0.75, subsystem: "Procedural" },
  ];

  const mockContext: ContextData = {
    environment: {
      currentTime: "Tuesday, 2:35 PM",
      location: "Home Office",
    },
    social: {
      currentSpeaker: "John",
      conversationTopic: "Day Planning",
      relationshipType: "User-Assistant",
    },
    task: {
      primaryActivity: "Conversation",
      currentGoal: "Plan Day",
      urgency: "Medium",
    },
    temporal: {
      timeOfDay: "Afternoon",
      upcomingEvents: "3 PM Meeting",
    },
  };

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
  const [intentEntities, setIntentEntities] = useState<Entity[]>(mockIntentEntities);
  const [extractedEntities, setExtractedEntities] = useState<Entity[]>(mockExtractedEntities);
  const [workingMemory, setWorkingMemory] = useState<WorkingMemoryChunk[]>(mockWorkingMemory);
  const [context, setContext] = useState<ContextData>(mockContext);
  const [summaryData, setSummaryData] = useState<Record<number, SummaryDepthData>>(mockSummaryData);

  // Add this useEffect to establish connection and set up listeners
  useEffect(() => {
    const connectSignalR = async () => {
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
          id: messageCount, // Simple ID generation, +1 to avoid collision
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
    };

    connectSignalR();

    // Cleanup on unmount
    return () => {
      signalRService.stop();
    };
  }, []);

  const renderMemoryTab = () => (
    <div className="h-full overflow-y-auto p-4">
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Working Memory</h3>
        <p className="text-sm text-gray-500">{mockWorkingMemory.length}/7 slots</p>
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
                style={{ width: `${chunk.activation * 100}%` }}
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
        <div>
          <h4 className="font-semibold text-base text-gray-800 mb-3 border-b border-gray-200 pb-2">Environment</h4>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Time:</span>
              <span className="text-sm font-medium text-gray-900">{context.environment.currentTime}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Location:</span>
              <span className="text-sm font-medium text-gray-900">{context.environment.location}</span>
            </div>
          </div>
        </div>

        <div>
          <h4 className="font-semibold text-base text-gray-800 mb-3 border-b border-gray-200 pb-2">Social</h4>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Speaker:</span>
              <span className="text-sm font-medium text-gray-900">{context.social.currentSpeaker}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Topic:</span>
              <span className="text-sm font-medium text-gray-900">{context.social.conversationTopic}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Relationship:</span>
              <span className="text-sm font-medium text-gray-900">{context.social.relationshipType}</span>
            </div>
          </div>
        </div>

        <div>
          <h4 className="font-semibold text-base text-gray-800 mb-3 border-b border-gray-200 pb-2">Task</h4>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Activity:</span>
              <span className="text-sm font-medium text-gray-900">{context.task.primaryActivity}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Goal:</span>
              <span className="text-sm font-medium text-gray-900">{context.task.currentGoal}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Urgency:</span>
              <span className="text-sm font-medium text-gray-900">{context.task.urgency}</span>
            </div>
          </div>
        </div>

        <div>
          <h4 className="font-semibold text-base text-gray-800 mb-3 border-b border-gray-200 pb-2">Temporal</h4>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Time of Day:</span>
              <span className="text-sm font-medium text-gray-900">{context.temporal.timeOfDay}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Upcoming:</span>
              <span className="text-sm font-medium text-gray-900">{context.temporal.upcomingEvents}</span>
            </div>
          </div>
        </div>
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

  const renderSummariesTab = () => (
    <div className="h-full overflow-y-auto p-4">
      <div className="space-y-4">
        {/* Render depths from highest to lowest (3, 2, 1, 0) */}
        {Object.keys(summaryData)
          .map(Number)
          .sort((a, b) => b - a)
          .map((depth) => {
            const depthData = mockSummaryData[depth];
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
          />
        );
    }
  };

  return (
    <div className="h-screen bg-gray-50 flex flex-col max-w-full">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex-shrink-0">
        <div className="flex items-center justify-between">
          <h1 className="text-xl sm:text-2xl font-bold text-gray-900">RAINA</h1>
          <div className="flex items-center space-x-2 text-xs sm:text-sm">
            <span className="text-gray-500 hidden sm:inline">Cognitive Steps: 1,247,893</span>
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span className="text-gray-500">Active</span>
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
