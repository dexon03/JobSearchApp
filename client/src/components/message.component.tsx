import { ListItem, ListItemAvatar, Avatar, ListItemText } from "@mui/material";
import { MessageDto } from "../models/chat/message.dto";

const MessageComponent = ({ message, userId }: { message: MessageDto, userId: string }) => {

    const time = message.timeStamp.split('T')[1].split('.')[0] + ' ' + message.timeStamp.split('T')[0];
    return (
        message &&
        <ListItem alignItems="flex-start">
            <ListItemAvatar>
                <Avatar>{message.sender.userName.charAt(0)}</Avatar>
            </ListItemAvatar>
            <ListItemText
                primary={message.sender.id === userId ? "You" : message.sender.userName}
                secondary={
                    <>
                        <h5 style={{ whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}>{message.content}</h5>
                        <p>{time}</p>
                        {message.isRead ? <span>Read</span> : <span>Unread</span>}
                    </>
                }
            />
        </ListItem>
    );
};

export default MessageComponent;