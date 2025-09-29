import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Conversation,
  StreamChatRequest,
  CreateConversationRequest,
  MessageFeedbackRequest,
  ConversationStatus,
} from '../models/conversation.model';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

type SSEType =
  | {
      type: 'messageId';
      messageId: number;
    }
  | {
      type: 'v';
      content: string;
    };

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  private authService = inject(AuthService);

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.baseUrl}/conversation`);
  }

  getConversation(id: number): Observable<Conversation> {
    return this.http.get<Conversation>(`${this.baseUrl}/conversation/${id}`);
  }

  createConversation(request: CreateConversationRequest): Observable<Conversation> {
    return this.http.post<Conversation>(`${this.baseUrl}/conversation`, request);
  }

  updateConversation(id: number, title: string): Observable<Conversation> {
    return this.http.put<Conversation>(`${this.baseUrl}/conversation/${id}`, { title });
  }

  deleteConversation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/conversation/${id}`);
  }

  streamChat(request: StreamChatRequest): Observable<SSEType> {
    return this.createEventSource(`${this.baseUrl}/chat/stream`, request);
  }

  stopConversation(conversationId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/chat/stop_conversation`, { conversationId });
  }

  addMessageFeedback(request: MessageFeedbackRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/conversation/message_feedback`, request);
  }

  private createEventSource(url: string, body: StreamChatRequest): Observable<SSEType> {
    return new Observable((observer) => {
      const authHeader = this.authService.authHeader ?? '';

      fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'text/event-stream',
          Authorization: authHeader,
        },
        body: JSON.stringify(body),
      })
        .then(async (response) => {
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }

          const reader = response.body?.getReader();
          if (!reader) {
            throw new Error('No reader available');
          }

          const decoder = new TextDecoder();

          try {
            while (true) {
              const { done, value } = await reader.read();

              if (done) {
                observer.complete();
                break;
              }

              const chunk = decoder.decode(value, { stream: true });
              const lines = chunk.split('\n');

              for (const line of lines) {
                if (line.startsWith('data: ')) {
                  const data = line.slice(6);
                  if (data === '[DONE]') {
                    observer.complete();
                    return;
                  }
                  if (data.trim()) {
                    const parsedData = JSON.parse(data) as SSEType;
                    observer.next(parsedData);
                  }
                }
              }
            }
          } catch (error) {
            observer.error(error);
          }
        })
        .catch((error) => {
          observer.error(error);
        });
      return () => {};
    });
  }
}
