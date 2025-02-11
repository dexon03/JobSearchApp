import { useParams } from "react-router-dom"
import { useGetCandidateProfileQuery, useLazyGetCandidateProfileQuery } from "../../app/features/profile/profile.api"
import { Card, CardContent, Typography, Chip, Button, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../hooks/redux.hooks";
import useToken from "../../hooks/useToken";
import { showErrorToast } from "../../app/features/common/popup";
import { ChatCreateDto } from "../../models/chat/chat.create.dto";
import { useCreateChatMutation } from "../../app/features/chat/chat.api";
import { Role } from "../../models/common/role.enum";
import { Document, Page } from 'react-pdf';
import { useLazyDownloadResumeQuery } from "../../app/features/profile/candidateResume.api";

export function CandidatePage() {
    const { id } = useParams();
    const { data: profile, isLoading, isError, error } = useGetCandidateProfileQuery(id);
    const [createChat] = useCreateChatMutation();
    const [downloadResume] = useLazyDownloadResumeQuery();
    const [message, setMessage] = useState('');
    const [isMessageSent, setIsMessageSent] = useState(false);
    const { token } = useToken();
    const [resumePdf, setResumePdf] = useState<File | null>(null);
    const [numPages, setNumPages] = useState<number>();
    const recruiter = useAppSelector(state => state.profile.recruiterProfile);


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
                senderId: token?.userId,
                senderName: recruiter?.name + ' ' + recruiter?.surname,
                receiverId: profile.userId,
                receiverName: profile.name + ' ' + profile.surname,
                message: message,
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
        return <div>Loading...</div>;
    }

    if (isError) {
        return <p>Error: {JSON.stringify(error.data)}</p>;
    }

    // Create a string of unique locations separated by commas
    const locationString = [...new Set(profile?.locations.map(location => `${location.city}, ${location.country}`))].join(', ');

    // Map attendance modes to their string representations
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
                        <Typography variant="body1" style={{ whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}>{profile.description}</Typography>
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
                token?.role == Role[Role.Recruiter] ? (!isMessageSent ?
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