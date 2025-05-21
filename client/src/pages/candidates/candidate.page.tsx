import { Button, Card, CardContent, Chip, TextField, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { Document, Page } from 'react-pdf';
import { useParams } from "react-router-dom";
import { useCreateChatMutation } from "../../app/features/chat/chat.api";
import { showErrorToast } from "../../app/features/common/popup";
import { useLazyDownloadResumeQuery } from "../../app/features/profile/candidateResume.api";
import { useGetCandidateProfileQuery } from "../../app/features/profile/profile.api";
import { useAppSelector } from "../../hooks/redux.hooks";
import useRole from "../../hooks/useRole";
import { ChatCreateDto } from "../../models/chat/chat.create.dto";
import { Role } from "../../models/common/role.enum";
import ReactMarkdown from "react-markdown";
import remarkGfm from 'remark-gfm';

export function CandidatePage() {
    const { id } = useParams();
    const { data: profile, isLoading, isError, error } = useGetCandidateProfileQuery(id);
    const [createChat] = useCreateChatMutation();
    const [downloadResume] = useLazyDownloadResumeQuery();
    const [message, setMessage] = useState('');
    const [isMessageSent, setIsMessageSent] = useState(false);
    const { role } = useRole();
    const [resumePdf, setResumePdf] = useState<File | null>(null);
    const [numPages, setNumPages] = useState<number>();
    const recruiter = useAppSelector(state => state.profile.recruiterProfile);

    const processedDescription = useMemo(() => {
        if (!profile?.description) return '';

        let desc = profile.description;
        if (desc.startsWith('```markdown') || desc.startsWith('```markdawn')) {
            desc = desc.substring(desc.indexOf('\n') + 1);
        } else if (desc.startsWith('```')) {
            desc = desc.substring(3);
        }

        if (desc.endsWith('```')) {
            desc = desc.substring(0, desc.length - 3);
        }

        return desc.trim();
    }, [profile?.description]);

    useEffect(() => {
        downloadResume(id).then((response) => {
            const data = response.data;
            const file = new Blob([data], { type: 'application/pdf' });
            setResumePdf(file)
        });
    }, [id])

    const handleSendMessage = async () => {
        if (!message) return;

        if (recruiter) {
            const request = {
                senderName: recruiter?.name + ' ' + recruiter?.surname,
                receiverId: profile.userId,
                receiverName: profile.name + ' ' + profile.surname,
                message: message,
            } as ChatCreateDto;
            const result = await createChat(request);
            if (!result.error) {
                setIsMessageSent(true);
                setMessage('');
            } else {
                showErrorToast('Failed to send message');
            }
        } else {
            showErrorToast('Something went wrong')
        }
    };

    if (isLoading) {
        return <div>Loading...</div>;
    }

    if (isError) {
        showErrorToast(`Error: ${JSON.stringify(error.data)}`);
    }

    const locationString = [...new Set(profile?.locations.map(location => `${location.city}, ${location.country}`))].join(', ');

    const attendanceModes = ['OnSite', 'Remote', 'Mixed', 'OnSiteOrRemote'];


    function onDocumentLoadSuccess({ numPages }: { numPages: number }): void {
        setNumPages(numPages);
    }

    return (
        profile &&
        <>
            <div style={{ display: 'flex' }}>
                <Card style={{ flex: 1, marginRight: '1rem', padding: '1em' }}>
                    <CardContent>
                        <Typography variant="h5">{profile?.positionTitle}</Typography>
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
                        {profile.skills && profile.skills.map((skill) => (
                            <Chip label={skill.name} variant="outlined" style={{ margin: '0.5rem 0' }} />
                        ))}
                    </CardContent>
                </Card>
                <Card >
                    <CardContent>
                        <Typography variant="h6">Locations:</Typography>
                        <Typography variant="body1">{locationString.length > 0 ? locationString : "No locations"}</Typography>
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Attendance Mode</Typography>
                        <Typography variant="body1">{attendanceModes[profile.attendance]}</Typography>
                        {profile.desiredSalary ?
                            <>
                                <Typography variant="h6" style={{ marginTop: '1rem' }}>Desired salary</Typography>
                                <Typography variant="body1" style={{ color: 'green' }}>{profile.desiredSalary} USD</Typography>
                            </>
                            : null
                        }
                        <Typography variant="h6" style={{ marginTop: '1rem' }}>Email: </Typography>
                        <Typography variant="body1">{profile.email}</Typography>
                    </CardContent>
                </Card>
            </div >
            {resumePdf !== null && resumePdf.size > 0 ?
                <div className='my-2' style={{ height: '700px', overflowY: 'scroll', border: '1px solid #ccc', marginBottom: '20px', borderRadius: '7px' }}>
                    <Document file={resumePdf} onLoadSuccess={onDocumentLoadSuccess}>
                        {Array.apply(null, Array(numPages)).map((x, i) => i + 1).map(page => (
                            <Page key={page} pageNumber={page} renderTextLayer={false} renderAnnotationLayer={false} />
                        ))}
                    </Document>
                </div>
                : null
            }
            {
                role == Role[Role.Recruiter] ? (!isMessageSent ?
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
    );
}