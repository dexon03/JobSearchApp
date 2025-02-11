export interface ChatDto {
    id: string;
    name: string;
    senderOfLastMessage: string;
    lastMessage: string;
    isLastMessageRead: boolean;
}