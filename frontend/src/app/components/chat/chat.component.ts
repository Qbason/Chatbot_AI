import { Component, signal, effect, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AuthService } from '../../services/auth.service';
import { ConversationService } from '../../services/conversation.service';
import {
  Conversation,
  Message,
  Role,
  Rating,
  ConversationStatus,
  StreamChatRequest,
} from '../../models/conversation.model';
import { catchError, of, Subscription, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss'],
  imports: [
    CommonModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule,
  ],
})
export class ChatComponent implements AfterViewInit {
  @ViewChild('messageContainer') messageContainer!: ElementRef;
  @ViewChild('messageInput') messageInput!: ElementRef;

  conversations = signal<Conversation[]>([]);
  conversations$ = toObservable(this.conversations);
  selectedConversationId = signal<number | null>(null);
  selectedConversation = signal<Conversation | null>(null);
  currentMessageValue = '';
  isStreaming = signal(false);
  streamingMessageContent = signal('');
  streamSubscription: Subscription | null = null;

  editingConversationId = signal<number | null>(null);
  editingTitle = '';

  Role = Role;
  Rating = Rating;

  constructor(
    private authService: AuthService,
    private conversationService: ConversationService,
    private snackBar: MatSnackBar
  ) {
    this.loadConversations();

    effect(() => {
      if (this.streamingMessageContent()) {
        setTimeout(() => this.scrollToBottom(), 0);
      }
    });

    effect(() => {
      const conversationId = this.selectedConversationId();
      if (conversationId) {
        this.conversationService.getConversation(conversationId).subscribe({
          next: (conversation) => {
            this.selectedConversation.set(conversation);
          },
          error: (error) => {
            console.error('Error loading conversation:', error);
            this.snackBar.open('Error loading conversation', 'Close', { duration: 3000 });
          },
        });
      }
    });
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  loadConversations(): void {
    this.conversationService
      .getConversations()
      .pipe(
        catchError((error) => {
          console.error('Error loading conversations:', error);
          this.snackBar.open('Error loading conversations', 'Close', { duration: 3000 });
          return of([]);
        }),
        switchMap((conversations) => {
          if (conversations.length > 0 && this.selectedConversation() == null) {
            this.conversations.set(conversations);
            const firstConversationId = conversations[0].id;
            return this.conversationService.getConversation(firstConversationId);
          }
          return of(null);
        })
      )
      .subscribe((conversation) => {
        if (conversation) {
          this.selectedConversation.set(conversation);
        }
      });
  }

  selectConversation(conversationId: Conversation['id']): void {
    this.selectedConversationId.set(conversationId);
    setTimeout(() => this.scrollToBottom(), 0);
  }

  newConversation(): void {
    const title = 'New Chat';
    this.conversationService.createConversation({ title }).subscribe({
      next: (conversation) => {
        this.conversations.update((convs) => [...convs, conversation]);
        this.selectedConversationId.set(conversation.id);
      },
      error: (error) => {
        console.error('Error creating conversation:', error);
        this.snackBar.open('Error creating conversation', 'Close', { duration: 3000 });
      },
    });
  }

  sendMessage(): void {
    const message = this.currentMessageValue.trim();
    if (!message || this.isStreaming()) return;

    const conversationId = this.selectedConversation()?.id;

    if (!conversationId) {
      this.newConversation();
      setTimeout(() => this.sendMessage(), 100);
      return;
    }

    this.currentMessageValue = '';
    this.isStreaming.set(true);
    this.streamingMessageContent.set('');

    this.addMessageToConversation(conversationId, {
      id: Date.now(),
      role: Role.User,
      content: message,
      timestamp: new Date().toISOString(),
      ratings: [],
      conversationId,
    });

    const streamRequest: StreamChatRequest = {
      message,
      conversationId,
    };

    this.streamSubscription = this.conversationService.streamChat(streamRequest).subscribe({
      next: (chunk) => {
        this.streamingMessageContent.update((content) => content + chunk);
      },
      complete: () => {
        this.finalizeStreamingMessage(conversationId);
      },
      error: (error) => {
        console.error('Streaming error:', error);
        this.isStreaming.set(false);
        this.streamingMessageContent.set('');
        this.snackBar.open('Error sending message', 'Close', { duration: 3000 });
      },
    });
  }

  stopStreaming(): void {
    if (this.streamSubscription) {
      this.streamSubscription.unsubscribe();
      this.streamSubscription = null;
    }

    const conversationId = this.selectedConversation()?.id;
    if (conversationId) {
      this.conversationService.stopConversation(conversationId).subscribe({
        next: () => {
          this.finalizeStreamingMessage(conversationId);
        },
        error: (error) => {
          console.error('Error stopping conversation:', error);
          this.finalizeStreamingMessage(conversationId);
        },
      });
    }
  }

  private finalizeStreamingMessage(conversationId: number): void {
    const content = this.streamingMessageContent();
    if (content) {
      this.addMessageToConversation(conversationId, {
        //TODO: Replace with real ID from server
        id: Date.now() + 1,
        role: Role.AI,
        content,
        timestamp: new Date().toISOString(),
        ratings: [],
        conversationId,
      });
    }

    this.isStreaming.set(false);
    this.streamingMessageContent.set('');

    this.conversationService.getConversation(conversationId).subscribe({
      next: (updatedConversation) => {
        this.conversations.update((convs) =>
          convs.map((c) => (c.id === conversationId ? updatedConversation : c))
        );
        this.selectedConversation.set(updatedConversation);
      },
      error: (error) => {
        console.error('Error refreshing conversation:', error);
      },
    });
  }

  private addMessageToConversation(conversationId: number, message: Message): void {
    this.conversations.update((convs) =>
      convs.map((c) => (c.id === conversationId ? { ...c, messages: [...c.messages, message] } : c))
    );

    const currentConv = this.selectedConversation();
    if (currentConv && currentConv.id === conversationId) {
      this.selectedConversation.set({
        ...currentConv,
        messages: [...currentConv.messages, message],
      });
    }

    setTimeout(() => this.scrollToBottom(), 0);
  }

  rateMessage(messageId: number, rating: Rating): void {
    const conversationId = this.selectedConversation()?.id;
    if (!conversationId) return;

    this.conversationService
      .addMessageFeedback({
        conversationId,
        messageId,
        rating,
      })
      .subscribe({
        next: () => {
          this.updateMessageRating(messageId, rating);
          this.snackBar.open('Feedback submitted', 'Close', { duration: 2000 });
        },
        error: (error) => {
          console.error('Error submitting feedback:', error);
          this.snackBar.open('Error submitting feedback', 'Close', { duration: 3000 });
        },
      });
  }

  private updateMessageRating(messageId: number, rating: Rating): void {
    const conversation = this.selectedConversation();
    if (!conversation) return;

    const updatedMessages = conversation.messages.map((message) => {
      if (message.id === messageId) {
        return {
          ...message,
          ratings: [{ id: Date.now(), messageId, value: rating, userId: '' }],
        };
      }
      return message;
    });

    this.selectedConversation.set({
      ...conversation,
      messages: updatedMessages,
    });
  }

  deleteConversation(conversationId: number): void {
    this.conversationService.deleteConversation(conversationId).subscribe({
      next: () => {
        this.conversations.update((convs) => convs.filter((c) => c.id !== conversationId));

        const remainingConversations = this.conversations();
        if (remainingConversations.length > 0) {
          this.selectedConversationId.set(remainingConversations[0].id);
        } else {
          this.selectedConversationId.set(null);
        }

        this.snackBar.open('Conversation deleted', 'Close', { duration: 2000 });
      },
      error: (error) => {
        console.error('Error deleting conversation:', error);
        this.snackBar.open('Error deleting conversation', 'Close', { duration: 3000 });
      },
    });
  }

  startEditTitle(conversationId: number, currentTitle: string): void {
    this.editingConversationId.set(conversationId);
    this.editingTitle = currentTitle;

    setTimeout(() => {
      //TODO: do not use querySelector
      const input = document.querySelector('.edit-title-field input') as HTMLInputElement;
      if (input) {
        input.focus();
        input.select();
      }
    }, 100);
  }

  saveConversationTitle(conversationId: number): void {
    if (!this.editingTitle.trim()) {
      this.snackBar.open('Title cannot be empty', 'Close', { duration: 3000 });
      return;
    }

    this.conversationService
      .updateConversation(conversationId, this.editingTitle.trim())
      .subscribe({
        next: () => {
          this.conversations.update((convs) =>
            convs.map((c) =>
              c.id === conversationId ? { ...c, title: this.editingTitle.trim() } : c
            )
          );

          const selected = this.selectedConversation();
          if (selected && selected.id === conversationId) {
            this.selectedConversation.set({ ...selected, title: this.editingTitle.trim() });
          }

          this.editingConversationId.set(null);
          this.editingTitle = '';
          this.snackBar.open('Title updated successfully', 'Close', { duration: 2000 });
        },
        error: (error) => {
          console.error('Error updating conversation title:', error);
          this.snackBar.open('Error updating title', 'Close', { duration: 3000 });
        },
      });
  }

  cancelEditTitle(): void {
    this.editingConversationId.set(null);
    this.editingTitle = '';
  }

  logout(): void {
    this.authService.logout();
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom(): void {
    if (this.messageContainer) {
      const element = this.messageContainer.nativeElement;
      element.scrollTop = element.scrollHeight;
    }
  }

  getMessageRating(message: Message): Rating | null {
    if (message.ratings.length > 0) {
      return message.ratings[0].value;
    }
    return null;
  }

  isRated(message: Message, rating: Rating): boolean {
    const messageRating = this.getMessageRating(message);
    return messageRating === rating;
  }
}
