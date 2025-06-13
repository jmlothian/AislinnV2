import React, { useEffect, useRef } from "react";
import { Send } from "lucide-react";
import type { AuthState, Intent, Message } from "./models/models";

interface ChatTabProps {
  chatInput: string;
  setChatInput: (value: string) => void;
  messages: Message[];
  setMessages: (fn: (prev: Message[]) => Message[]) => void;
  messageCount: number;
  setMessageCount: (n: number) => void;
  currentIntent: Intent;
  authState: AuthState;
}
//quick and dirty
const generateId = (): string => {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0;
    const v = c == "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};
export const ChatTab: React.FC<ChatTabProps> = ({
  chatInput,
  setChatInput,
  messages,
  setMessages,
  messageCount,
  setMessageCount,
  currentIntent,
  authState,
}) => {
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" });
  }, [messages]);

  const sendMessage = () => {
    if (!chatInput.trim()) return;

    const chatInputBuffer = chatInput;
    const timestamp = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

    setMessageCount(messageCount + 1);
    const userMessage: Message = {
      id: generateId(),
      type: "user",
      text: chatInputBuffer,
      timestamp,
    };

    setMessages((prev) => [...prev, userMessage]);
    setChatInput("");

    fetch("http://localhost:5299/api/raina/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message: chatInputBuffer, sessionId: authState.username }),
    })
      .then((response) => {
        if (!response.ok) console.log("Bad Response");
        return response.json();
      })
      .then((result) => {
        console.log("Message sent successfully:", result);
      })
      .catch((error) => {
        console.error("Error sending message:", error);
      });
  };

  return (
    <div className="flex flex-col h-full">
      <div ref={scrollRef} className="flex-1 overflow-y-auto p-4 space-y-4">
        {messages.map((message) => (
          <div key={message.id} id={message.id} className={`flex ${message.type === "user" ? "justify-end" : "justify-start"}`}>
            <div
              className={`max-w-[85%] sm:max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
                message.type === "user" ? "bg-blue-600 text-white" : "bg-white border border-gray-200 text-gray-900"
              }`}
            >
              <p className="text-sm">{message.text}</p>
              <p className={`text-xs mt-1 ${message.type === "user" ? "text-blue-100" : "text-gray-500"}`}>{message.timestamp}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="border-t border-gray-200 p-4">
        <div className="mb-3 p-2 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex justify-between items-center text-sm">
            <span className="text-blue-700 font-medium">Intent: {currentIntent.type}</span>
            <span className="text-blue-600">({(currentIntent.confidence * 100).toFixed(0)}%)</span>
          </div>
        </div>

        <div className="flex space-x-2">
          <input
            type="text"
            value={chatInput}
            onChange={(e) => setChatInput(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && sendMessage()}
            placeholder="Type your message..."
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button
            onClick={sendMessage}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 flex-shrink-0"
          >
            <Send size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};
