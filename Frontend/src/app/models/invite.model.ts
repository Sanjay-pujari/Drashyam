export interface UserInvite {
  id: number;
  inviterId: string;
  inviterName: string;
  inviteeEmail: string;
  inviteeFirstName?: string;
  inviteeLastName?: string;
  inviteToken: string;
  status: InviteStatus;
  createdAt: string;
  acceptedAt?: string;
  expiresAt?: string;
  personalMessage?: string;
  type: InviteType;
  acceptedUserId?: string;
  acceptedUserName?: string;
}

export interface CreateInvite {
  inviteeEmail: string;
  inviteeFirstName?: string;
  inviteeLastName?: string;
  personalMessage?: string;
  type: InviteType;
  expirationDays?: number;
}

export interface AcceptInvite {
  inviteToken: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface InviteStats {
  totalInvites: number;
  pendingInvites: number;
  acceptedInvites: number;
  expiredInvites: number;
  conversionRate: number;
}

export interface BulkInvite {
  invites: CreateInvite[];
}

export interface InviteLink {
  inviteLink: string;
  inviteToken: string;
  expiresAt: string;
  usageCount: number;
  maxUsage: number;
}

export enum InviteStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

export enum InviteType {
  Email = 'Email',
  Social = 'Social',
  DirectLink = 'DirectLink'
}
