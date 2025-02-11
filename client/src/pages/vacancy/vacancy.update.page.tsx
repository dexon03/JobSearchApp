import { useNavigate, useParams } from "react-router-dom";
import { useDeleteVacancyMutation, useGetVacancyQuery, useLazyGetVacancyCategoriesQuery, useLazyGetVacancyLocationQuery, useLazyGetVacancySkillsQuery, useUpdateVacancyMutation } from "../../app/features/vacancy/vacancy.api";
import useToken from "../../hooks/useToken";
import { FormEvent, useEffect, useState } from "react";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { Experience } from "../../models/vacancy/experience.enum";
import { Button, Container, InputLabel, MenuItem, OutlinedInput, Select, TextField } from "@mui/material";
import { VacancyUpdateModel } from "../../models/vacancy/vacancy.update.dto";

export function VacancyUpdatePage() {
    const { id } = useParams();
    const { data: vacancy, isLoading, isError, error } = useGetVacancyQuery(id);
    const [getVacancySkills, { data: skills, isError: isSkillsLoadingError }] = useLazyGetVacancySkillsQuery();
    const [getVacancyLocations, { data: locations, isError: isErrorLoadingError }] = useLazyGetVacancyLocationQuery();
    const [getVacancyCategories, { data: categories, isError: isCategoriesLoadingError }] = useLazyGetVacancyCategoriesQuery();
    const [deleteVacancy] = useDeleteVacancyMutation();
    const [updateVacancy] = useUpdateVacancyMutation();
    const { token } = useToken();
    const navigate = useNavigate();

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
        if (vacancy) {
            setTitle(vacancy.title);
            setPositionTitle(vacancy.positionTitle);
            setDescription(vacancy.description);
            setSalary(vacancy.salary);
            setExperience(vacancy.experience);
            setAttendanceMode(vacancy.attendanceMode);
            setSelectedCategory(vacancy.category.id);
            setSelectedLocations(vacancy.locations.map(location => location.id));
            setSelectedSkills(vacancy.skills.map(skill => skill.id));
            getVacancyCategories();
            getVacancyLocations();
            getVacancySkills();
        }
    }, [vacancy])

    if (token?.role == 'Candidate') {
        return <p>Access denied</p>
    }

    if (isError || isSkillsLoadingError || isErrorLoadingError || isCategoriesLoadingError) {
        return <p>Error</p>
    }

    if (isLoading) {
        return <p>Loading...</p>
    }

    function handleSubmit(event: FormEvent<HTMLFormElement>): void {
        event.preventDefault();
        updateVacancy({
            id: id,
            title: title,
            positionTitle: positionTitle,
            description: description,
            salary: salary,
            experience: experience,
            attendanceMode: attendanceMode,
            categoryId: selectedCategory,
            locations: locations.filter(location => selectedLocations.includes(location.id)),
            skills: skills.filter(skill => selectedSkills.includes(skill.id))
        } as VacancyUpdateModel);
        navigate('/vacancy/myVacancies');
    }

    function handleDelete() {
        deleteVacancy(id);
        navigate('/vacancy/myVacancies');
    }

    return (
        <Container component="main" maxWidth="sm">
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
                <Button type="submit" fullWidth variant="contained" color="primary">
                    Save
                </Button>
                <Button fullWidth onClick={handleDelete} variant="contained" className="my-2" color="error">
                    Delete
                </Button>
            </form>
        </Container>
    )
}