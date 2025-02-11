import { useParams } from "react-router-dom"
import { useGetVacancyQuery } from "../../app/features/vacancy/vacancy.api";
import { Card, CardContent, Typography, Chip, Box, TextField, Button } from "@mui/material";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { useState } from "react";
import { useLazyGetRecruiterProfileQuery } from "../../app/features/profile/profile.api";
import { ChatCreateDto } from "../../models/chat/chat.create.dto";
import useToken from "../../hooks/useToken";
import { useAppSelector } from "../../hooks/redux.hooks";
import { Role } from "../../models/common/role.enum";
import { useCreateChatMutation } from "../../app/features/chat/chat.api";
import { showErrorToast } from "../../app/features/common/popup";

export function VacancyPage() {
    const { id } = useParams();
    const { data: vacancy, isError, isLoading, error } = useGetVacancyQuery(id);
    const [getRecruiter] = useLazyGetRecruiterProfileQuery();
    const [createChat] = useCreateChatMutation();
    const [message, setMessage] = useState('');
    const [isMessageSent, setIsMessageSent] = useState(false);
    const { token } = useToken();
    const candidate = useAppSelector(state => state.profile.candidateProfile);

    const handleSendMessage = async () => {
        if (!message) return;

        const recruiter = await getRecruiter(vacancy?.recruiterId);
        const currentPageUrl = window.location.href;
        const messageToSend = message + `\n\n${currentPageUrl}`;
        if (candidate && !recruiter.isError) {
            const request = {
                senderId: token?.userId,
                senderName: candidate?.name + ' ' + candidate?.surname,
                receiverId: recruiter?.data.userId,
                receiverName: recruiter?.data.name + ' ' + recruiter?.data.surname,
                message: messageToSend,
            } as ChatCreateDto
            const result = await createChat(request);
            if (!result.error) {
                setIsMessageSent(true);
                setMessage('');
            }
        } else {
            showErrorToast('Something went wrong')
        }
    };

    if (isLoading) {
        return <p>Loading...</p>;
    }

    if (isError) {
        return <p>Error: {JSON.stringify(error.data)}</p>;
    }

    const locationString = [...new Set(vacancy?.locations.map(location => `${location.city}, ${location.country}`))].join(', ');

    return (
        vacancy &&
        <>
            <div style={{ display: 'flex' }}>
                <Card style={{ flex: 1, marginRight: '1rem', padding: '1em' }}>
                    <CardContent>
                        <Typography variant="h5">{vacancy?.title}</Typography>
                        <Typography variant="h6">{vacancy?.positionTitle}</Typography>
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Description</Typography>
                        <Typography variant="body1" style={{ whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}>{vacancy.description}</Typography>
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Skills</Typography>
                        {vacancy.skills && vacancy.skills.map((skill) => (
                            <Chip label={skill.name} variant="outlined" style={{ margin: '0.5rem 0' }} />
                        ))}
                        <Box m={2} />
                        <Typography variant="h6">About company:</Typography>
                        <Typography variant="body1" style={{ whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}>{vacancy.company.description}</Typography>
                    </CardContent>
                </Card>
                <Card >
                    <CardContent>
                        <Typography variant="h6">Locations</Typography>
                        <Typography variant="body1">{locationString}</Typography>
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Attendance Mode</Typography>
                        <Typography variant="body1">{AttendanceMode[vacancy.attendanceMode]}</Typography>
                        {vacancy.salary ?
                            <>
                                <Typography variant="h6" style={{ marginTop: '1rem' }}>Salary</Typography>
                                <Typography variant="body1" style={{ color: 'green' }}>{vacancy.salary} USD</Typography>
                            </>
                            : null
                        }
                    </CardContent>
                </Card>
            </div >
            {token?.role == Role[Role.Candidate] ? (!isMessageSent ?
                <div style={{ marginTop: '1rem', width: '100%' }}>
                    <TextField
                        label="Type your message"
                        variant="outlined"
                        fullWidth
                        multiline
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                    />
                    <Button
                        variant="contained"
                        color="primary"
                        style={{ marginTop: '0.5rem' }}
                        onClick={handleSendMessage}
                    >
                        Send Application
                    </Button>
                </div> :
                <Typography variant="h6" style={{ marginTop: '1rem', color: 'green' }}>Your application has been sent</Typography>
            )
                : null
            }
        </>
    )
}