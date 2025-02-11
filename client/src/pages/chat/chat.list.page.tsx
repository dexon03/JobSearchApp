import { List, ListItem, ListItemIcon, Grid, Card, CardContent, Typography, Pagination } from "@mui/material";
import { useGetChatListQuery } from "../../app/features/chat/chat.api";
import useToken from "../../hooks/useToken";
import { ChatDto } from "../../models/chat/chat.dto";
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { useNavigate } from "react-router-dom";
import { useState } from "react";

export default function ChatList() {
    const [page, setPage] = useState(1);
    const pageSize = 10; // or any number you prefer
    const { token } = useToken();
    const { data, isLoading } = useGetChatListQuery({
        userId: token?.userId ?? 'skip',
        page,
        pageSize
    });
    const navigate = useNavigate();

    const handlePageChange = (event: React.ChangeEvent<unknown>, value: number) => {
        setPage(value);
    };

    if (isLoading) {
        return <div>Loading...</div>
    }

    const onChatClick = (chatId: string) => {
        navigate(`/chat/${chatId}`);
    }

    return (
        <div>
            <h1>ChatList</h1>
            <List>
                {data?.items?.map((chat: ChatDto) => (
                    <ListItem key={chat.id} onClick={() => onChatClick(chat.id)}>
                        <Card sx={{ width: '100%' }}>
                            <CardContent>
                                <Grid container>
                                    <Grid item xs={4}>
                                        <Typography variant="h6">{chat.name}</Typography>
                                    </Grid>
                                    <Grid item xs={6}>
                                        <Typography variant="body1">{chat.senderOfLastMessage}</Typography>
                                        <Typography variant="body2">{chat.lastMessage}</Typography>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <ListItemIcon>
                                            {chat.isLastMessageRead ? <CheckCircleIcon /> : <CheckCircleOutlineIcon />}
                                        </ListItemIcon>
                                    </Grid>
                                </Grid>
                            </CardContent>
                        </Card>
                    </ListItem>
                ))}
            </List>
            <Pagination 
                count={Math.ceil((data?.totalCount ?? 0) / pageSize)} 
                page={page}
                onChange={handlePageChange}
                sx={{ mt: 2, display: 'flex', justifyContent: 'center' }}
            />
        </div>
    );
}