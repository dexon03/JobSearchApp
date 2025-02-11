import { useEffect, useState } from "react";
import { useCreateVacancyMutation, useLazyGetVacancyCategoriesQuery, useLazyGetVacancyLocationQuery, useLazyGetVacancySkillsQuery, useLazyGenerateVacancyDesciprtionQuery } from "../../app/features/vacancy/vacancy.api"
import { Experience } from "../../models/vacancy/experience.enum";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { Button, Container, InputLabel, MenuItem, OutlinedInput, Select, TextField } from "@mui/material";
import { VacancyCreate } from "../../models/vacancy/vacancy.create.dto";
import { useAppSelector } from "../../hooks/redux.hooks";
import useToken from "../../hooks/useToken";
import { RecruiterProfile } from "../../models/profile/recruiter.profile.model";
import { useNavigate } from "react-router-dom";
import { showWarningToast } from "../../app/features/common/popup";

export function VacancyCreatePage() {
    const [createVacancy, { data: createdProfile, isError: isCreateError }] = useCreateVacancyMutation();
    const [getVacancySkills, { data: skills, isError: isSkillsLoadingError }] = useLazyGetVacancySkillsQuery();
    const [getVacancyLocations, { data: locations, isError: isErrorLoadingError }] = useLazyGetVacancyLocationQuery();
    const [getVacancyCategories, { data: categories, isError: isCategoriesLoadingError }] = useLazyGetVacancyCategoriesQuery();
    const [generateDescription, { data: generatedDescription }] = useLazyGenerateVacancyDesciprtionQuery();
    const { token } = useToken();
    const navigate = useNavigate();

    const recruiterProfile: RecruiterProfile = useAppSelector(state => state.profile.recruiterProfile)

    const [title, setTitle] = useState('');
    const [positionTitle, setPositionTitle] = useState('');
    const [description, setDescription] = useState('');
    const [salary, setSalary] = useState(0);
    const [experience, setExperience] = useState<Experience>(0);
    const [attendanceMode, setAttendanceMode] = useState<AttendanceMode>(0);
    const [selectedCategory, setSelectedCategory] = useState('');
    const [selectedLocations, setSelectedLocations] = useState<string[]>([]);
    const [selectedSkills, setSelectedSkills] = useState<string[]>([]);

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

    if (token?.role == 'Candidate') {
        return <p>Access denied</p>
    }

    if (isSkillsLoadingError || isErrorLoadingError || isCategoriesLoadingError || isCreateError) {
        return <p>Error</p>
    }

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (recruiterProfile.company == null) {
            showWarningToast('You must be registered in company')
            return;
        }
        const result = await createVacancy({
            title,
            positionTitle,
            description,
            salary,
            experience,
            attendanceMode,
            companyId: recruiterProfile?.company?.id,
            recruiterId: recruiterProfile.id,
            categoryId: categories && categories.find(category => category.id === selectedCategory)?.id,
            locations: locations && locations.filter(location => selectedLocations.includes(location.id)),
            skills: skills && skills.filter(skill => selectedSkills.includes(skill.id)),
        } as VacancyCreate)
        if (result.error == null) {
            navigate('/vacancy')
        }
    }

    const handleGenerateWithGPT = async () => {
        if (!title || !description) {
            showWarningToast('Title and description must be filled')
            return;
        }

        if (recruiterProfile.company == null) {
            showWarningToast('You must be registered in company')
            return;
        }
        const result = await generateDescription({
            title,
            vacancyShortDescription: description,
            companyDescription: recruiterProfile.company.description
        });
        if (result.data) {
            setDescription(result.data)
        }
    }

    return (
        token?.role == 'Candidate'
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
                        label="Position Title"
                        margin="normal"
                        name="positionTitle"
                        value={positionTitle}
                        onChange={(e) => setPositionTitle(e.target.value)}
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
                        fullWidth
                        variant="contained"
                        style={{ backgroundColor: 'green', color: 'white', marginTop: '10px' }}
                        onClick={handleGenerateWithGPT}
                    >
                        Generate vacancy with GPT
                    </Button>
                    <Button type="submit" fullWidth variant="contained" color="primary">
                        Save
                    </Button>
                </form>
            </Container>
    )
}