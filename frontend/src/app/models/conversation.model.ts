export interface Conversation {
  id: number;
  title: string;
  messages: Message[];
  userId: string;
  timestamp: string;
  status: ConversationStatus;
}

export interface Message {
  id: number;
  role: Role;
  content: string;
  timestamp: string;
  ratings: MessageRating[];
  conversationId: number;
}

export interface MessageRating {
  id: number;
  messageId: number;
  value: Rating;
  userId: string;
}

export enum ConversationStatus {
  Streaming = 0,
  Completed = 1,
  Stopped = 2,
}

export enum Role {
  User = 0,
  AI = 1,
}

export enum Rating {
  ThumbsUp = 0,
  ThumbsDown = 1,
}

export interface StreamChatRequest {
  message: string;
  conversationId?: number;
}

export interface CreateConversationRequest {
  title: string;
}

export interface MessageFeedbackRequest {
  conversationId: number;
  messageId: number;
  rating: Rating;
}

export interface LoginRequest {
  email: string;
}

export interface AuthResponse {
  message: string;
  userId: string;
  email: string;
  isAuthenticated: boolean;
  authHeader: string;
}
