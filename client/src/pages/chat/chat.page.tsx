import { useEffect, useRef, useState } from 'react';
import { useGetChatMessagesQuery } from '../../app/features/chat/chat.api';
import useToken from '../../hooks/useToken';
import { useParams } from 'react-router-dom';
import { MessageDto } from '../../models/chat/message.dto';
import MessageComponent from '../../components/message.component';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAppSelector } from '../../hooks/redux.hooks';
import { TextField, Container, Button } from '@mui/material';
import { environment } from '../../environment/environment';
import { ApiServicesRoutes } from '../../api/api.services.routes';

const ChatPage = () => {
    const { token } = useToken();
    const { id } = useParams<{ id: string }>();
    const { data: previousMessages, isLoading } = useGetChatMessagesQuery(id);

    const chatContainerRef = useRef();
    const [connection, setConnection] = useState<any>(null);
    const [messages, setMessages] = useState<MessageDto[]>([] as MessageDto[]);
    const [newMessage, setNewMessage] = useState('');
    const [companionId, setCompanionId] = useState('');
    const sender = useAppSelector((state) => state.profile.candidateProfile || state.profile.recruiterProfile);

    useEffect(() => {
        setMessages(previousMessages);
        const chatCompanion = previousMessages && (previousMessages[0]?.sender.id === token?.userId ? previousMessages[0]?.receiver.id : previousMessages[0]?.sender.id);
        setCompanionId(chatCompanion || '');
        // const url = "http://localhost:5245/chatHub"
        const url = environment.apiUrl + ApiServicesRoutes.chatHub + '/chatHub'
        const connection = new HubConnectionBuilder()
            .withUrl(url)
            .configureLogging(LogLevel.Information)
            .build();
        try {
            connection.start().then(() => {
                connection.invoke('JoinChatGroup', id);
            });
            connection.on('ReceiveMessage', (message: MessageDto) => {
                setMessages(prevMessages => [...prevMessages, message]);
            });
            connection.on('ConnectedUser', (message: any) => {
                console.log(message);
            })


        } catch (error) {
            console.log(error);
        }


        setConnection(connection);

        return () => {
            connection.invoke('LeaveChatGroup', id);
            connection.stop();
            setConnection(null);
        };
    }, [previousMessages]);

    useEffect(() => {
        if (chatContainerRef.current) {
            chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
        }


    }, [messages]);

    const handleSendMessage = () => {
        if (!newMessage) return;
        const newMessageDto = {
            content: newMessage,
            senderId: token?.userId || '',
            receiverId: companionId,
            senderName: sender?.name + ' ' + sender?.surname,
            chatId: id,
            isRead: false,
        };

        // Send the message to the server using SignalR
        connection.invoke('SendMessage', newMessageDto);

        // Clear the input field
        setNewMessage('');
    };


    if (isLoading) {
        return <div>Loading...</div>
    }

    return (
        <Container ref={chatContainerRef} className='pb-2' maxWidth="md" style={{ maxHeight: '80vh', overflowY: 'auto', overflowX: 'hidden', }}>
            {messages && messages.map((msg) => (
                <MessageComponent key={msg.id} message={msg} userId={token?.userId} />
            ))}

            <div style={{ display: 'flex', marginTop: '8px' }}>
                <TextField
                    style={{ flex: 1 }}
                    type="text"
                    multiline
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                />
                <Button
                    variant="contained"
                    color="primary"
                    style={{ marginLeft: '8px' }}
                    onClick={handleSendMessage}
                >
                    Send
                </Button>
            </div>
        </Container>
    );
}

export default ChatPage;