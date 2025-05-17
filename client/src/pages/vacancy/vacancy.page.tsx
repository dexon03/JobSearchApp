import { useParams } from "react-router-dom"
import { useGetVacancyQuery } from "../../app/features/vacancy/vacancy.api";
import { Card, CardContent, Typography, Chip, Box, TextField, Button } from "@mui/material";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { useState, useMemo } from "react";
import { useLazyGetRecruiterProfileQuery } from "../../app/features/profile/profile.api";
import { ChatCreateDto } from "../../models/chat/chat.create.dto";
import { useAppSelector } from "../../hooks/redux.hooks";
import { Role } from "../../models/common/role.enum";
import { useCreateChatMutation } from "../../app/features/chat/chat.api";
import { showErrorToast } from "../../app/features/common/popup";
import useRole from "../../hooks/useRole";
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

export function VacancyPage() {
    const { id } = useParams();
    const { data: vacancy } = useGetVacancyQuery(id!);
    const [getRecruiter] = useLazyGetRecruiterProfileQuery();
    const [createChat] = useCreateChatMutation();
    const [message, setMessage] = useState('');
    const [isMessageSent, setIsMessageSent] = useState(false);
    const candidate = useAppSelector(state => state.profile.candidateProfile);

    const { role } = useRole();

    const handleSendMessage = async () => {
        if (!message) return;

        const recruiter = await getRecruiter(vacancy!.recruiterId);
        const currentPageUrl = window.location.href;
        const messageToSend = message + `\n\n${currentPageUrl}`;
        if (candidate && !recruiter.isError) {
            const request = {
                vacancyId: id,
                receiverId: recruiter?.data?.userId,
                receiverName: recruiter?.data?.name + ' ' + recruiter?.data?.surname,
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


    const locationString = [...new Set(vacancy?.locations?.map(location => `${location.city}, ${location.country}`))].join(', ');

    const processedDescription = useMemo(() => {
        if (!vacancy?.description) return '';

        let desc = vacancy.description;
        if (desc.startsWith('```markdown') || desc.startsWith('```markdawn')) {
            desc = desc.substring(desc.indexOf('\n') + 1);
        } else if (desc.startsWith('```')) {
            desc = desc.substring(3);
        }

        if (desc.endsWith('```')) {
            desc = desc.substring(0, desc.length - 3);
        }

        return desc.trim();
    }, [vacancy?.description]);

    return (
        vacancy &&
        <>
            <div style={{ display: 'flex' }}>
                <Card style={{ flex: 1, marginRight: '1rem', padding: '1em' }}>
                    <CardContent>
                        <Typography variant="h5">{vacancy?.title}</Typography>
                        <Typography variant="h6">{vacancy?.positionTitle}</Typography>
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Description</Typography>
                        <div className="markdown-content" style={{ margin: '1rem 0' }}>
                            <ReactMarkdown
                                remarkPlugins={[remarkGfm]}
                                components={{
                                    p: ({ node, ...props }) => <p style={{ margin: '0.5rem 0' }} {...props} />,
                                    ul: ({ node, ...props }) => <ul style={{ marginLeft: '1.5rem', listStyleType: 'disc' }} {...props} />,
                                    ol: ({ node, ...props }) => <ol style={{ marginLeft: '1.5rem' }} {...props} />,
                                    li: ({ node, ...props }) => <li style={{ margin: '0.25rem 0' }} {...props} />,
                                    h1: ({ node, ...props }) => <h1 style={{ fontSize: '1.8rem', fontWeight: 'bold', margin: '1rem 0' }} {...props} />,
                                    h2: ({ node, ...props }) => <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', margin: '1rem 0' }} {...props} />,
                                    h3: ({ node, ...props }) => <h3 style={{ fontSize: '1.3rem', fontWeight: 'bold', margin: '0.75rem 0' }} {...props} />,
                                    strong: (props) => <strong style={{ fontWeight: 'bold' }} {...props} />
                                }}
                            >
                                {processedDescription}
                            </ReactMarkdown>
                        </div>
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
            {role == Role.Candidate ? (!isMessageSent ?
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