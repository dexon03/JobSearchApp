import { Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, InputLabel, MenuItem, OutlinedInput, Select, TextField, Paper, Typography } from "@mui/material";
import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { showErrorToast, showSuccessToast, showWarningToast } from "../../app/features/common/popup";
import { useCreateVacancyMutation, useLazyGenerateVacancyDesciprtionQuery, useLazyGetVacancyCategoriesQuery, useLazyGetVacancyLocationQuery, useLazyGetVacancySkillsQuery } from "../../app/features/vacancy/vacancy.api";
import { useAppSelector } from "../../hooks/redux.hooks";
import useRole from "../../hooks/useRole";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { Role } from "../../models/common/role.enum";
import { RecruiterProfile } from "../../models/profile/recruiter.profile.model";
import { Experience } from "../../models/vacancy/experience.enum";
import { VacancyCreate } from "../../models/vacancy/vacancy.create.dto";
import SmartToyIcon from '@mui/icons-material/SmartToy';
import CircularProgress from '@mui/material/CircularProgress';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

export function VacancyCreatePage() {
    const [createVacancy] = useCreateVacancyMutation();
    const [getVacancySkills, { data: skills }] = useLazyGetVacancySkillsQuery();
    const [getVacancyLocations, { data: locations }] = useLazyGetVacancyLocationQuery();
    const [getVacancyCategories, { data: categories }] = useLazyGetVacancyCategoriesQuery();
    const [generateDescription, { isLoading: isGenerating }] = useLazyGenerateVacancyDesciprtionQuery();
    const { role } = useRole();
    const navigate = useNavigate();

    const recruiterProfile: RecruiterProfile = useAppSelector(state => state.profile.recruiterProfile!)

    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [salary, setSalary] = useState(0);
    const [experience, setExperience] = useState<Experience>(0);
    const [attendanceMode, setAttendanceMode] = useState<AttendanceMode>(0);
    const [selectedCategory, setSelectedCategory] = useState('');
    const [selectedLocations, setSelectedLocations] = useState<string[]>([]);
    const [selectedSkills, setSelectedSkills] = useState<string[]>([]);

    // AI dialog state
    const [aiDialogOpen, setAiDialogOpen] = useState(false);
    const [aiVacancyTitle, setAiVacancyTitle] = useState('');
    const [aiShortDescription, setAiShortDescription] = useState('');
    const [aiGeneratedDescription, setAiGeneratedDescription] = useState('');

    useEffect(() => {
        if (!skills) {
            getVacancySkills();
        }
        if (!locations) {
            getVacancyLocations();
        }
        if (!categories) {
            getVacancyCategories();
        }
    }, [skills, locations, categories, getVacancySkills, getVacancyLocations, getVacancyCategories])

    if (role == Role.Candidate) {
        return <p>Access denied</p>
    }


    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (recruiterProfile.company == null) {
            showWarningToast('You must be registered in company')
            return;
        }
        const result = await createVacancy({
            title,
            description,
            salary,
            experience,
            attendanceMode,
            locations: locations && locations.filter(location => selectedLocations.includes(location.id)),
            skills: skills && skills.filter(skill => selectedSkills.includes(skill.id)),
            categoryId: categories && categories.find(category => category.id === selectedCategory)?.id,
            companyId: recruiterProfile?.company?.id,
            recruiterId: recruiterProfile.id,
        } as VacancyCreate)
        if (result.error) {
            showErrorToast('Error creating vacancy')
            return;
        }
        showSuccessToast('Vacancy created successfully')
        navigate('/vacancy')
    }

    const handleGenerateWithAI = async () => {
        if (!aiVacancyTitle || !aiShortDescription) {
            showWarningToast('Title and short description must be filled');
            return;
        }

        if (recruiterProfile.company == null) {
            showWarningToast('You must be registered in company');
            return;
        }

        const result = await generateDescription({
            experience: experience,
            position: aiVacancyTitle,
            description: aiShortDescription,
        });

        if (result.data) {
            setAiGeneratedDescription(result.data.description);
        }
    }

    const handleUseAiDescription = () => {
        setDescription(aiGeneratedDescription);
        setAiDialogOpen(false);
        // Reset AI dialog fields
        setAiGeneratedDescription('');
    };

    const handleOpenAiDialog = () => {
        setAiVacancyTitle(title);
        setAiShortDescription(description);
        setAiGeneratedDescription('');
        setAiDialogOpen(true);
    };

    const handleCloseAiDialog = () => {
        setAiDialogOpen(false);
    };

    const processedDescription = useMemo(() => {
        if (!aiGeneratedDescription) return '';

        let desc = aiGeneratedDescription;
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
    }, [aiGeneratedDescription]);

    return (
        role == Role.Candidate
            ? <p>Access denied</p>
            : <Container component="main" maxWidth="sm">
                <form onSubmit={handleSubmit}>
                    <TextField
                        label="Title"
                        margin="normal"
                        name="title"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        fullWidth
                        required
                    />
                    <TextField
                        label="Salary"
                        margin="normal"
                        name="salary"
                        type="number"
                        value={salary}
                        onChange={(e) => setSalary(e.target.value)}
                        fullWidth
                        required
                    />
                    <TextField
                        select
                        label="Work Experience"
                        margin="normal"
                        fullWidth
                        defaultValue={Experience.NoExperience}
                        value={experience}
                        onChange={(e) => {
                            setExperience(Number(e.target.value))
                        }}
                    >
                        {Object.values(Experience).filter((v) => isNaN(Number(v))).map((value) => (
                            <MenuItem key={value} value={Experience[value]}>
                                {value}
                            </MenuItem>
                        ))}
                    </TextField>
                    <TextField
                        select
                        label="Attendance Mode"
                        margin="normal"
                        fullWidth
                        defaultValue={AttendanceMode.Remote}
                        value={attendanceMode}
                        onChange={(e) => {
                            setAttendanceMode(Number(e.target.value))
                        }}
                    >
                        {Object.values(AttendanceMode).filter((v) => isNaN(Number(v))).map((value) => (
                            <MenuItem key={value} value={AttendanceMode[value]}>
                                {value}
                            </MenuItem>
                        ))}
                    </TextField>
                    <InputLabel>Category</InputLabel>
                    <Select
                        fullWidth
                        value={selectedCategory}
                        onChange={(e) => setSelectedCategory(e.target.value)}
                        input={<OutlinedInput label="Category" />}
                    >
                        {categories && categories.map((category) => (
                            <MenuItem key={category.id} value={category.id}>
                                {category.name}
                            </MenuItem>
                        ))}
                    </Select>
                    <InputLabel>Locations</InputLabel>
                    <Select
                        multiple
                        fullWidth
                        value={selectedLocations}
                        onChange={(e) => setSelectedLocations(e.target.value)}
                        input={<OutlinedInput label="Locations" />}
                    >
                        {locations && locations.map((location) => (
                            <MenuItem key={location.id} value={location.id}>
                                {location.city}, {location.country}
                            </MenuItem>
                        ))}
                    </Select>
                    <InputLabel>Skills</InputLabel>
                    <Select
                        multiple
                        fullWidth
                        value={selectedSkills}
                        onChange={(e) => setSelectedSkills(e.target.value)}
                        input={<OutlinedInput label="Skills" />}
                    >
                        {skills && skills.map((skill) => (
                            <MenuItem key={skill.id} value={skill.id}>
                                {skill.name}
                            </MenuItem>
                        ))}
                    </Select>
                    <div style={{ position: 'relative' }}>
                        <TextField
                            label="Description"
                            name="description"
                            margin="normal"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            fullWidth
                            multiline
                            rows={4}
                            required
                        />
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={handleOpenAiDialog}
                            style={{
                                position: 'absolute',
                                top: '25px',
                                right: '10px',
                                minWidth: 'unset',
                                width: '40px',
                                height: '40px',
                                borderRadius: '50%'
                            }}
                            title="Generate with AI"
                        >
                            <SmartToyIcon />
                        </Button>
                    </div>
                    <Button type="submit" fullWidth variant="contained" color="primary" style={{ marginTop: '20px' }}>
                        Save
                    </Button>
                </form>

                <Dialog open={aiDialogOpen} onClose={handleCloseAiDialog} fullWidth maxWidth="md">
                    <DialogTitle>AI Vacancy Description Generator</DialogTitle>
                    <DialogContent>
                        <TextField
                            autoFocus
                            margin="dense"
                            label="Vacancy Title"
                            fullWidth
                            variant="outlined"
                            value={aiVacancyTitle}
                            onChange={(e) => setAiVacancyTitle(e.target.value)}
                        />
                        <TextField
                            margin="dense"
                            label="Short Description"
                            fullWidth
                            multiline
                            rows={3}
                            variant="outlined"
                            value={aiShortDescription}
                            onChange={(e) => setAiShortDescription(e.target.value)}
                            helperText="Provide a brief description of the job for the AI to enhance"
                        />
                        <Button
                            fullWidth
                            variant="contained"
                            color="primary"
                            onClick={handleGenerateWithAI}
                            disabled={isGenerating}
                            style={{ marginTop: '16px' }}
                            startIcon={isGenerating ? <CircularProgress size={24} color="inherit" /> : <SmartToyIcon />}
                        >
                            {isGenerating ? 'Generating...' : 'Generate Description'}
                        </Button>

                        {aiGeneratedDescription && (
                            <>
                                <TextField
                                    margin="dense"
                                    label="Edit AI Generated Description"
                                    fullWidth
                                    multiline
                                    rows={6}
                                    variant="outlined"
                                    value={aiGeneratedDescription}
                                    onChange={(e) => setAiGeneratedDescription(e.target.value)}
                                    sx={{ marginTop: '16px' }}
                                />
                                <Paper
                                    elevation={1}
                                    sx={{
                                        marginTop: '16px',
                                        padding: '16px',
                                        maxHeight: '300px',
                                        overflow: 'auto',
                                        bgcolor: '#f5f5f5'
                                    }}
                                >
                                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                        Preview:
                                    </Typography>
                                    <ReactMarkdown remarkPlugins={[remarkGfm]}>
                                        {processedDescription}
                                    </ReactMarkdown>
                                </Paper>
                            </>
                        )}
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleCloseAiDialog} color="primary">
                            Cancel
                        </Button>
                        <Button
                            onClick={handleUseAiDescription}
                            color="primary"
                            disabled={!aiGeneratedDescription}
                            variant="contained"
                        >
                            Use This Description
                        </Button>
                    </DialogActions>
                </Dialog>
            </Container>
    )
}