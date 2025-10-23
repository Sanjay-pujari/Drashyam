import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatMessage {
  id: number;
  liveStreamId: number;
  userId: string;
  userName: string;
  userAvatar?: string;
  message: string;
  messageType: string;
  timestamp: Date;
  isModerator: boolean;
  isSubscriber: boolean;
  isHighlighted: boolean;
  isDeleted: boolean;
  replyToMessageId?: number;
  reactions: ChatReaction[];
}

export interface ChatReaction {
  id: number;
  messageId: number;
  userId: string;
  reactionType: string;
  timestamp: Date;
}

export interface SendChatMessage {
  liveStreamId: number;
  message: string;
  messageType: string;
  replyToMessageId?: number;
}

export interface ChatSettings {
  liveStreamId: number;
  allowChat: boolean;
  allowReactions: boolean;
  allowLinks: boolean;
  slowMode: boolean;
  slowModeDelay: number;
  followerOnlyMode: boolean;
  subscriberOnlyMode: boolean;
  moderatorOnlyMode: boolean;
  autoModeration: boolean;
  profanityFilter: boolean;
  spamFilter: boolean;
}

export interface ChatModeration {
  id: number;
  liveStreamId: number;
  moderatorId: string;
  action: string;
  targetUserId: string;
  reason?: string;
  duration?: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class LiveChatService {
  private apiUrl = `${environment.apiUrl}/api`;
  private chatMessagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public chatMessages$ = this.chatMessagesSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Chat Messages
  getChatMessages(liveStreamId: number, page: number = 1, pageSize: number = 50): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/messages?page=${page}&pageSize=${pageSize}`);
  }

  sendMessage(message: SendChatMessage): Observable<ChatMessage> {
    return this.http.post<ChatMessage>(`${this.apiUrl}/livestreamchat/message`, message);
  }

  deleteMessage(messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/livestreamchat/message/${messageId}`);
  }

  editMessage(messageId: number, newMessage: string): Observable<ChatMessage> {
    return this.http.put<ChatMessage>(`${this.apiUrl}/livestreamchat/message/${messageId}`, { message: newMessage });
  }

  // Chat Reactions
  addReaction(messageId: number, reactionType: string): Observable<ChatReaction> {
    return this.http.post<ChatReaction>(`${this.apiUrl}/livestreamchat/message/${messageId}/reaction`, { reactionType });
  }

  removeReaction(messageId: number, reactionType: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/livestreamchat/message/${messageId}/reaction/${reactionType}`);
  }

  getMessageReactions(messageId: number): Observable<ChatReaction[]> {
    return this.http.get<ChatReaction[]>(`${this.apiUrl}/livestreamchat/message/${messageId}/reactions`);
  }

  // Chat Settings
  getChatSettings(liveStreamId: number): Observable<ChatSettings> {
    return this.http.get<ChatSettings>(`${this.apiUrl}/livestreamchat/${liveStreamId}/settings`);
  }

  updateChatSettings(liveStreamId: number, settings: Partial<ChatSettings>): Observable<ChatSettings> {
    return this.http.put<ChatSettings>(`${this.apiUrl}/livestreamchat/${liveStreamId}/settings`, settings);
  }

  // Chat Moderation
  getChatModeration(liveStreamId: number): Observable<ChatModeration[]> {
    return this.http.get<ChatModeration[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/moderation`);
  }

  moderateUser(liveStreamId: number, targetUserId: string, action: string, reason?: string, duration?: number): Observable<ChatModeration> {
    return this.http.post<ChatModeration>(`${this.apiUrl}/livestreamchat/${liveStreamId}/moderate`, {
      targetUserId,
      action,
      reason,
      duration
    });
  }

  // Chat Analytics
  getChatAnalytics(liveStreamId: number, startTime: Date, endTime: Date): Observable<any> {
    const params = {
      startTime: startTime.toISOString(),
      endTime: endTime.toISOString()
    };
    return this.http.get(`${this.apiUrl}/livestreamchat/${liveStreamId}/analytics`, { params });
  }

  getChatStatistics(liveStreamId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/livestreamchat/${liveStreamId}/statistics`);
  }

  // Chat Bots
  getChatBots(liveStreamId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/bots`);
  }

  createChatBot(liveStreamId: number, bot: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/bots`, bot);
  }

  updateChatBot(liveStreamId: number, botId: number, bot: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/livestreamchat/${liveStreamId}/bots/${botId}`, bot);
  }

  deleteChatBot(liveStreamId: number, botId: number): Observable<void> {
    return this.http.delete(`${this.apiUrl}/livestreamchat/${liveStreamId}/bots/${botId}`);
  }

  // Chat Commands
  getChatCommands(liveStreamId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/commands`);
  }

  createChatCommand(liveStreamId: number, command: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/commands`, command);
  }

  updateChatCommand(liveStreamId: number, commandId: number, command: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/livestreamchat/${liveStreamId}/commands/${commandId}`, command);
  }

  deleteChatCommand(liveStreamId: number, commandId: number): Observable<void> {
    return this.http.delete(`${this.apiUrl}/livestreamchat/${liveStreamId}/commands/${commandId}`);
  }

  // Chat Polls
  getChatPolls(liveStreamId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/polls`);
  }

  createChatPoll(liveStreamId: number, poll: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/polls`, poll);
  }

  voteOnPoll(liveStreamId: number, pollId: number, optionId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/polls/${pollId}/vote`, { optionId });
  }

  endChatPoll(liveStreamId: number, pollId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/polls/${pollId}/end`, {});
  }

  // Chat Highlights
  getChatHighlights(liveStreamId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/highlights`);
  }

  createChatHighlight(liveStreamId: number, highlight: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/highlights`, highlight);
  }

  deleteChatHighlight(liveStreamId: number, highlightId: number): Observable<void> {
    return this.http.delete(`${this.apiUrl}/livestreamchat/${liveStreamId}/highlights/${highlightId}`);
  }

  // Chat Emotes
  getChatEmotes(liveStreamId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/livestreamchat/${liveStreamId}/emotes`);
  }

  uploadChatEmote(liveStreamId: number, emote: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/livestreamchat/${liveStreamId}/emotes`, emote);
  }

  deleteChatEmote(liveStreamId: number, emoteId: number): Observable<void> {
    return this.http.delete(`${this.apiUrl}/livestreamchat/${liveStreamId}/emotes/${emoteId}`);
  }

  // Local State Management
  addMessageToLocalState(message: ChatMessage): void {
    const currentMessages = this.chatMessagesSubject.value;
    this.chatMessagesSubject.next([...currentMessages, message]);
  }

  updateMessageInLocalState(message: ChatMessage): void {
    const currentMessages = this.chatMessagesSubject.value;
    const index = currentMessages.findIndex(m => m.id === message.id);
    if (index !== -1) {
      currentMessages[index] = message;
      this.chatMessagesSubject.next([...currentMessages]);
    }
  }

  removeMessageFromLocalState(messageId: number): void {
    const currentMessages = this.chatMessagesSubject.value;
    this.chatMessagesSubject.next(currentMessages.filter(m => m.id !== messageId));
  }

  clearLocalState(): void {
    this.chatMessagesSubject.next([]);
  }

  // Utility Methods
  formatMessage(message: ChatMessage): string {
    let formattedMessage = message.message;
    
    // Add user styling
    if (message.isModerator) {
      formattedMessage = `<span class="moderator">${message.userName}</span>: ${formattedMessage}`;
    } else if (message.isSubscriber) {
      formattedMessage = `<span class="subscriber">${message.userName}</span>: ${formattedMessage}`;
    } else {
      formattedMessage = `${message.userName}: ${formattedMessage}`;
    }

    // Add timestamp
    const timestamp = new Date(message.timestamp).toLocaleTimeString();
    formattedMessage += ` <span class="timestamp">${timestamp}</span>`;

    return formattedMessage;
  }

  parseMessageForEmotes(message: string, emotes: any[]): string {
    // Simple emote parsing - replace :emote: with emote image
    emotes.forEach(emote => {
      const regex = new RegExp(`:${emote.name}:`, 'g');
      message = message.replace(regex, `<img src="${emote.url}" alt="${emote.name}" class="chat-emote">`);
    });

    return message;
  }

  isMessageSpam(message: string, recentMessages: ChatMessage[]): boolean {
    // Simple spam detection
    const recentUserMessages = recentMessages
      .filter(m => m.timestamp > new Date(Date.now() - 30000)) // Last 30 seconds
      .slice(-5); // Last 5 messages

    const duplicateMessages = recentUserMessages.filter(m => m.message === message);
    return duplicateMessages.length >= 3;
  }
}
