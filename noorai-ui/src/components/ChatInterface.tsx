"use client";

import React, { useState, useRef, ChangeEvent } from "react";
import { PaperClipIcon, PaperAirplaneIcon } from "@heroicons/react/24/outline";
import { API_ENDPOINTS } from "../config/api";
import clsx from 'clsx';

interface Message {
  content: string;
  isUser: boolean;
}

interface InterviewResponse {
  id: number;
  question: string;
  feedback?: string;
}

export default function ChatInterface() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputMessage, setInputMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentInterviewId, setCurrentInterviewId] = useState<number | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  React.useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const formatTranscript = (transcript: string): Message[] => {
    const lines = transcript.split("\n");
    const formattedMessages: Message[] = [];

    lines.forEach((line) => {
      if (line.trim() === "") return;

      if (line.startsWith("AI:")) {
        formattedMessages.push({
          content: line.replace("AI:", "").trim(),
          isUser: false,
        });
      } else if (line.startsWith("You:")) {
        formattedMessages.push({
          content: line.replace("You:", "").trim(),
          isUser: true,
        });
      } else {
        formattedMessages.push({
          content: line.trim(),
          isUser: false,
        });
      }
    });

    return formattedMessages;
  };

  const handleFileUpload = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      setError("File size must be less than 10MB");
      return;
    }

    setError(null);
    setIsLoading(true);

    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(API_ENDPOINTS.UPLOAD, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        throw new Error("Failed to upload file");
      }

      const data: InterviewResponse = await response.json();
      setCurrentInterviewId(data.id);

      const formattedMessages = formatTranscript(data.question);
      setMessages(formattedMessages);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload file");
    } finally {
      setIsLoading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleSendMessage = async () => {
    if (!inputMessage.trim() || !currentInterviewId) return;

    const userMessage = inputMessage;
    setInputMessage("");
    setMessages((prev) => [...prev, { content: userMessage, isUser: true }]);
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch(API_ENDPOINTS.INTERVIEW_RESPOND, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ interviewId: currentInterviewId }),
      });

      if (!response.ok) {
        throw new Error("Failed to get response");
      }

      const data = await response.json();

      if (data.feedback) {
        const formattedMessages = formatTranscript(data.question);
        setMessages((prev) => [
          ...prev,
          ...formattedMessages,
          {
            content: "Here's your feedback:",
            isUser: false,
          },
          { content: data.feedback, isUser: false },
        ]);
        setCurrentInterviewId(null);
      } else {
        const formattedMessages = formatTranscript(data.question);
        setMessages((prev) => [...prev, ...formattedMessages]);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to get response");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-6 py-4 shadow-sm">
        <div className="max-w-5xl mx-auto">
          <h1 className="text-3xl font-bold text-gray-900">NoorAI Interview Assistant</h1>
          <p className="text-gray-600 mt-2">Upload your resume (PDF) to start the interview process</p>
        </div>
      </header>

      {/* Chat Container */}
      <div 
        ref={chatContainerRef}
        className="flex-1 overflow-y-auto px-6 py-8"
      >
        <div className="max-w-5xl mx-auto space-y-6">
          {messages.map((message, index) => (
            <div
              key={index}
              className={clsx(
                "flex",
                message.isUser ? "justify-end" : "justify-start"
              )}
            >
              <div
                className={clsx(
                  "flex flex-col max-w-[80%]",
                  message.isUser ? "items-end" : "items-start"
                )}
              >
                <div
                  className={clsx(
                    "text-sm font-medium mb-1",
                    message.isUser ? "text-blue-600" : "text-emerald-600"
                  )}
                >
                  {message.isUser ? "You" : "NoorAI"}
                </div>
                <div
                  className={clsx(
                    "rounded-2xl px-6 py-4 shadow-sm",
                    message.isUser
                      ? "bg-blue-600 text-white"
                      : "bg-white text-gray-800 border border-gray-200"
                  )}
                >
                  {message.content}
                </div>
              </div>
            </div>
          ))}

          {isLoading && (
            <div className="flex justify-start">
              <div className="flex flex-col max-w-[80%]">
                <div className="text-sm font-medium mb-1 text-emerald-600">
                  NoorAI
                </div>
                <div className="bg-white rounded-2xl px-6 py-4 text-gray-800 border border-gray-200 shadow-sm">
                  <div className="flex items-center space-x-2">
                    <div className="w-2 h-2 bg-emerald-600 rounded-full animate-bounce"></div>
                    <div className="w-2 h-2 bg-emerald-600 rounded-full animate-bounce [animation-delay:0.2s]"></div>
                    <div className="w-2 h-2 bg-emerald-600 rounded-full animate-bounce [animation-delay:0.4s]"></div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {error && (
            <div className="flex justify-center">
              <div className="bg-red-50 text-red-800 rounded-lg px-6 py-4 border border-red-200 max-w-[80%]">
                {error}
              </div>
            </div>
          )}
          <div ref={messagesEndRef} />
        </div>
      </div>

      {/* Input Area */}
      <div className="border-t border-gray-200 bg-white px-6 py-4">
        <div className="max-w-5xl mx-auto flex items-center space-x-4">
          <button
            onClick={() => fileInputRef.current?.click()}
            className="p-3 text-gray-500 hover:text-gray-700 transition-colors rounded-full hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-gray-200"
            disabled={isLoading}
            title="Upload Resume (PDF)"
          >
            <PaperClipIcon className="h-6 w-6" />
          </button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileUpload}
            className="hidden"
            accept=".pdf"
          />
          <input
            type="text"
            value={inputMessage}
            onChange={(e) => setInputMessage(e.target.value)}
            onKeyPress={(e) => e.key === "Enter" && handleSendMessage()}
            placeholder="Type your message..."
            className="flex-1 px-6 py-3 border border-gray-200 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-gray-800 placeholder-gray-400"
            disabled={isLoading}
          />
          <button
            onClick={handleSendMessage}
            disabled={isLoading || !inputMessage.trim()}
            className="p-3 text-blue-600 hover:text-blue-700 transition-colors rounded-full hover:bg-blue-50 disabled:text-gray-400 disabled:hover:bg-transparent focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <PaperAirplaneIcon className="h-6 w-6" />
          </button>
        </div>
      </div>
    </div>
  );
}