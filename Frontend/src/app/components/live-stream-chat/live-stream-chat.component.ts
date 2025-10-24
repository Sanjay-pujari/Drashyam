import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignalRService, ChatMessage, ChatReaction } from '../../services/signalr.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-live-stream-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './live-stream-chat.component.html',
  styleUrls: ['./live-stream-chat.component.scss']
})
export class LiveStreamChatComponent implements OnInit, OnDestroy {
  @ViewChild('chatContainer', { static: false }) chatContainer!: ElementRef;
  @ViewChild('messageInput', { static: false }) messageInput!: ElementRef;
  
  @Input() streamId: string = '';
  @Input() userId: string = '';
  @Input() userName: string = '';
  @Input() isModerator: boolean = false;
  @Input() isSubscriber: boolean = false;
  @Input() maxMessages: number = 100;
  @Input() autoScroll: boolean = true;
  @Input() showTimestamps: boolean = true;
  @Input() enableReactions: boolean = true;
  @Input() enableMentions: boolean = true;
  
  @Output() messageSent = new EventEmitter<ChatMessage>();
  @Output() reactionSent = new EventEmitter<ChatReaction>();
  @Output() userMentioned = new EventEmitter<string>();

  // Chat state
  messages: ChatMessage[] = [];
  newMessage: string = '';
  isConnected = false;
  isTyping = false;
  typingUsers: string[] = [];
  
  // UI state
  showChat = true;
  showEmojiPicker = false;
  showUserList = false;
  showSettings = false;
  viewerCount = 0;
  chatSettings = {
    fontSize: 'medium',
    showTimestamps: true,
    showUserColors: true,
    enableSound: true,
    enableNotifications: true
  };
  
  // Moderation
  moderationMode = false;
  selectedMessage: ChatMessage | null = null;
  
  // Reactions
  availableReactions = ['❤️', '😂', '😮', '😢', '😡', '👍', '👎', '🔥', '💯', '🎉'];
  
  private destroy$ = new Subject<void>();
  private typingTimeout: any;
  private scrollTimeout: any;

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    this.setupSignalRConnection();
    this.loadChatHistory();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.disconnectFromChat();
  }

  private setupSignalRConnection(): void {
    // Connect to SignalR
    this.signalRService.connect();
    
    // Listen for chat messages
    this.signalRService.onChatMessage()
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        this.handleNewMessage(message);
      });

    // Listen for reactions
    this.signalRService.onChatReaction()
      .pipe(takeUntil(this.destroy$))
      .subscribe(reaction => {
        this.handleReaction(reaction);
      });

    // Listen for typing indicators
    this.signalRService.onUserTyping()
      .pipe(takeUntil(this.destroy$))
      .subscribe(typingData => {
        this.handleTypingIndicator(typingData);
      });

    // Connect to chat
    this.connectToChat();
  }

  private connectToChat(): void {
    if (this.streamId) {
      this.signalRService.joinChat(this.streamId);
      this.isConnected = true;
    }
  }

  private disconnectFromChat(): void {
    if (this.streamId) {
      this.signalRService.leaveChat(this.streamId);
      this.isConnected = false;
    }
  }

  private async loadChatHistory(): Promise<void> {
    // In a real implementation, you would load chat history from API
    // For now, we'll start with an empty chat
    this.messages = [];
  }

  private handleNewMessage(message: ChatMessage): void {
    this.messages.push(message);
    
    // Limit message count
    if (this.messages.length > this.maxMessages) {
      this.messages = this.messages.slice(-this.maxMessages);
    }
    
    // Auto-scroll to bottom
    if (this.autoScroll) {
      this.scrollToBottom();
    }
    
    // Play sound if enabled
    if (this.chatSettings.enableSound) {
      this.playNotificationSound();
    }
    
    // Check for mentions
    if (this.enableMentions && this.isUserMentioned(message.message)) {
      this.userMentioned.emit(message.userName);
    }
  }

  private handleReaction(reaction: ChatReaction): void {
    // Find the message and add reaction
    const message = this.messages.find(m => m.id === reaction.messageId);
    if (message) {
      if (!message.reactions) {
        message.reactions = [];
      }
      message.reactions.push(reaction);
    }
  }

  private handleTypingIndicator(typingData: any): void {
    if (typingData.isTyping) {
      if (!this.typingUsers.includes(typingData.userName)) {
        this.typingUsers.push(typingData.userName);
      }
    } else {
      this.typingUsers = this.typingUsers.filter(user => user !== typingData.userName);
    }
  }

  private isUserMentioned(message: string): boolean {
    return message.toLowerCase().includes(`@${this.userName.toLowerCase()}`);
  }

  private playNotificationSound(): void {
    // Play a subtle notification sound
    const audio = new Audio('assets/sounds/notification.mp3');
    audio.volume = 0.3;
    audio.play().catch(() => {
      // Ignore audio play errors
    });
  }

  private scrollToBottom(): void {
    if (this.chatContainer) {
      this.scrollTimeout = setTimeout(() => {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      }, 100);
    }
  }

  // Message sending
  sendMessage(): void {
    if (!this.newMessage.trim() || !this.isConnected) return;

    const message: ChatMessage = {
      id: Date.now(),
      userId: this.userId,
      userName: this.userName,
      message: this.newMessage.trim(),
      timestamp: new Date(),
      isModerator: this.isModerator,
      isSubscriber: this.isSubscriber,
      reactions: []
    };

    // Send via SignalR
    this.signalRService.sendMessage(this.streamId, this.userName, this.newMessage.trim());
    
    // Emit event
    this.messageSent.emit(message);
    
    // Clear input
    this.newMessage = '';
    this.stopTyping();
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    } else {
      this.startTyping();
    }
  }

  private startTyping(): void {
    if (!this.isTyping) {
      this.isTyping = true;
      this.signalRService.sendTypingIndicator(this.streamId, this.userName, true);
    }
    
    // Clear existing timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
    
    // Set new timeout
    this.typingTimeout = setTimeout(() => {
      this.stopTyping();
    }, 3000);
  }

  private stopTyping(): void {
    if (this.isTyping) {
      this.isTyping = false;
      this.signalRService.sendTypingIndicator(this.streamId, this.userName, false);
    }
    
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
  }

  // Reactions
  addReaction(message: ChatMessage, reactionType: string): void {
    const reaction: ChatReaction = {
      id: Date.now(),
      reactionType,
      userId: this.userId,
      timestamp: new Date()
    };

    // Send via SignalR
    this.signalRService.sendReaction(this.streamId, this.userName, reactionType);
    
    // Add to message
    if (!message.reactions) {
      message.reactions = [];
    }
    message.reactions.push(reaction);
    
    // Emit event
    this.reactionSent.emit(reaction);
  }

  getReactionCount(message: ChatMessage, reactionType: string): number {
    if (!message.reactions) return 0;
    return message.reactions.filter(r => r.reactionType === reactionType).length;
  }

  hasUserReacted(message: ChatMessage, reactionType: string): boolean {
    if (!message.reactions) return false;
    return message.reactions.some(r => r.reactionType === reactionType && r.userId === this.userId);
  }

  // Moderation
  toggleModerationMode(): void {
    this.moderationMode = !this.moderationMode;
  }

  selectMessage(message: ChatMessage): void {
    if (this.moderationMode) {
      this.selectedMessage = message;
    }
  }

  deleteMessage(message: ChatMessage): void {
    if (this.isModerator) {
      // Remove from local array
      this.messages = this.messages.filter(m => m.id !== message.id);
      
      // Send moderation action via SignalR
      this.signalRService.sendModerationAction(this.streamId, 'delete', message.id);
    }
  }

  timeoutUser(message: ChatMessage, duration: number): void {
    if (this.isModerator) {
      // Send timeout action via SignalR
      this.signalRService.sendModerationAction(this.streamId, 'timeout', message.userId, duration);
    }
  }

  banUser(message: ChatMessage): void {
    if (this.isModerator) {
      // Send ban action via SignalR
      this.signalRService.sendModerationAction(this.streamId, 'ban', message.userId);
    }
  }

  // UI Controls
  toggleChat(): void {
    this.showChat = !this.showChat;
  }

  toggleEmojiPicker(): void {
    this.showEmojiPicker = !this.showEmojiPicker;
  }

  toggleUserList(): void {
    this.showUserList = !this.showUserList;
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
  }

  addEmoji(emoji: string): void {
    this.newMessage += emoji;
    this.showEmojiPicker = false;
    this.messageInput.nativeElement.focus();
  }

  // Utility methods
  formatTimestamp(timestamp: Date): string {
    return timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  getUserColor(userId: string): string {
    // Generate a consistent color for each user
    const colors = [
      '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7',
      '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E9'
    ];
    const index = userId.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0) % colors.length;
    return colors[index];
  }

  getMessageClass(message: ChatMessage): string {
    let classes = 'message';
    
    if (message.isModerator) classes += ' moderator';
    if (message.isSubscriber) classes += ' subscriber';
    if (this.isUserMentioned(message.message)) classes += ' mentioned';
    
    return classes;
  }

  getTypingText(): string {
    if (this.typingUsers.length === 0) return '';
    if (this.typingUsers.length === 1) return `${this.typingUsers[0]} is typing...`;
    if (this.typingUsers.length === 2) return `${this.typingUsers[0]} and ${this.typingUsers[1]} are typing...`;
    return `${this.typingUsers[0]} and ${this.typingUsers.length - 1} others are typing...`;
  }

  clearChat(): void {
    this.messages = [];
  }

  exportChat(): void {
    const chatData = this.messages.map(msg => ({
      timestamp: msg.timestamp.toISOString(),
      user: msg.userName,
      message: msg.message,
      isModerator: msg.isModerator,
      isSubscriber: msg.isSubscriber
    }));
    
    const blob = new Blob([JSON.stringify(chatData, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `chat-${this.streamId}-${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }

  // Track by function for ngFor
  trackByMessageId(index: number, message: ChatMessage): number {
    return message.id;
  }

  // Get unique reactions for display
  getUniqueReactions(reactions: ChatReaction[]): ChatReaction[] {
    const unique = new Map<string, ChatReaction>();
    reactions.forEach(reaction => {
      if (!unique.has(reaction.reactionType)) {
        unique.set(reaction.reactionType, reaction);
      }
    });
    return Array.from(unique.values());
  }
}
