export interface ChatCreateDto {
    senderId: string;
    senderName: string;
    receiverId: string;
    receiverName: string;
    message: string;
}