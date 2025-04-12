/* eslint-disable @typescript-eslint/no-unused-vars */
import { Button, Card, CardContent, Chip, Stack, Typography } from '@mui/material';
import { useState, useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { useNavigate } from 'react-router-dom';
import { useActivateDisactivateVacancyMutation } from '../app/features/vacancy/vacancy.api';
import { showSuccessToast } from '../app/features/common/popup';



export function VacancyTile({ vacancy, isRecruiterList }) {
    const [expanded, setExpanded] = useState(false);
    const navigate = useNavigate();
    const [activateDeactivate] = useActivateDisactivateVacancyMutation();

    const MAX_CHARS = 200;

    // Process the description to remove markdown code block syntax if present
    const processedDescription = useMemo(() => {
        if (!vacancy?.description) return '';

        let desc = vacancy.description;
        // Check if the description starts with ```markdown or ```
        if (desc.startsWith('```markdown') || desc.startsWith('```markdawn')) {
            desc = desc.substring(desc.indexOf('\n') + 1);
        } else if (desc.startsWith('```')) {
            desc = desc.substring(3);
        }

        // Remove closing backticks if present
        if (desc.endsWith('```')) {
            desc = desc.substring(0, desc.length - 3);
        }

        return desc.trim();
    }, [vacancy?.description]);

    const isTruncated = processedDescription.length > MAX_CHARS;
    const displayText = expanded || !isTruncated
        ? processedDescription
        : processedDescription.slice(0, MAX_CHARS) + '...';

    const handleVacancyClicked = () => {
        navigate(`/vacancy/${vacancy.id}`);
    };

    const handleEditClicked = (e) => {
        e.stopPropagation();
        navigate(`/vacancy/edit/${vacancy.id}`);
    };

    const handleActivateDeactivate = async (e) => {
        e.stopPropagation();
        await activateDeactivate(vacancy.id);
        showSuccessToast(`Vacancy ${vacancy.isActive ? 'deactivated' : 'activated'} successfully`);
    };

    return (
        <Card className="m-2">
            <CardContent>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <div>
                        <Typography variant="h5">{vacancy.title}</Typography>
                        <Typography variant="subtitle1" color="text.secondary">
                            {vacancy.company?.name}
                        </Typography>
                    </div>
                    <Typography variant="h6">
                        {vacancy.salary}$
                    </Typography>
                </div>

                <div className="mb-2">
                    <ReactMarkdown
                        remarkPlugins={[remarkGfm]}
                        components={{
                            p: ({ node, ...props }) => <p style={{ margin: '0.5rem 0' }} {...props} />,
                            ul: ({ node, ...props }) => <ul style={{ marginLeft: '1.5rem', listStyleType: 'disc' }} {...props} />,
                            ol: ({ node, ...props }) => <ol style={{ marginLeft: '1.5rem' }} {...props} />,
                            li: ({ node, ...props }) => <li style={{ margin: '0.25rem 0' }} {...props} />,
                            strong: ({ node, ...props }) => <strong style={{ fontWeight: 'bold' }} {...props} />
                        }}
                    >
                        {displayText}
                    </ReactMarkdown>

                    {isTruncated && (
                        <Button
                            onClick={() => setExpanded(!expanded)}
                            size="small"
                            color="primary"
                        >
                            {expanded ? 'Show less' : 'Read more'}
                        </Button>
                    )}
                </div>

                <Stack direction="row" spacing={1} className="my-2">
                    {vacancy.locations && vacancy.locations.map(location => (
                        <Chip
                            key={location.id}
                            label={`${location.city}, ${location.country}`}
                            variant="outlined"
                            size="small"
                        />
                    ))}
                </Stack>

                <Stack direction="row" spacing={1} className="my-2">
                    {vacancy.skills && vacancy.skills.map(skill => (
                        <Chip
                            key={skill.id}
                            label={skill.name}
                            variant="outlined"
                            size="small"
                            color="primary"
                        />
                    ))}
                </Stack>

                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                        <Chip
                            label={vacancy.attendanceMode}
                            color="secondary"
                            size="small"
                        />
                    </div>
                    <div>
                        {isRecruiterList && (
                            <>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={handleEditClicked}
                                    style={{ marginRight: '0.5rem' }}
                                >
                                    Edit
                                </Button>
                                <Button
                                    variant="contained"
                                    color={vacancy.isActive ? 'secondary' : 'primary'}
                                    onClick={handleActivateDeactivate}
                                >
                                    {vacancy.isActive ? 'Deactivate' : 'Activate'}
                                </Button>
                            </>
                        )}
                        <Button variant="contained" onClick={handleVacancyClicked}>
                            View Details
                        </Button>
                    </div>
                </div>
            </CardContent>
        </Card >
    );
}