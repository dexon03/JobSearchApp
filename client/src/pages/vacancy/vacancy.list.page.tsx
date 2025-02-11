import { VacancyTile } from "../../components/vacancy.tile.tsx";
import { useGetVacanciesQuery, useLazyGetVacancyCategoriesQuery, useLazyGetVacancyLocationQuery, useLazyGetVacancySkillsQuery } from "../../app/features/vacancy/vacancy.api.ts";
import { Typography, Button, TextField, Pagination, FormControl, InputLabel, MenuItem, Select } from "@mui/material";
import { useNavigate } from "react-router-dom";
import useToken from "../../hooks/useToken.ts";
import { Role } from "../../models/common/role.enum.ts";
import { useEffect, useState } from "react";
import { AttendanceMode } from "../../models/common/attendance.enum.ts";
import { Experience } from "../../models/vacancy/experience.enum.ts";

export function VacancyListPage() {
    const { token } = useToken();
    const [searchTerm, setSearchTerm] = useState('');
    const [searchValue, setSearchValue] = useState('');
    const [page, setPage] = useState(1);
    const [getLocations, { data: locations }] = useLazyGetVacancyLocationQuery();
    const [getSkills, { data: skills }] = useLazyGetVacancySkillsQuery();
    const [getCategories, { data: categories }] = useLazyGetVacancyCategoriesQuery();
    const pageSize = 5;
    const [filterAttendance, setFilterAttendance] = useState<AttendanceMode>();
    const [filterExperience, setFilterExperience] = useState<Experience>();
    const [filterLocation, setFilterLocation] = useState('');
    const [filterSkill, setFilterSkill] = useState('');
    const [filterCategory, setFilterCategory] = useState('');
    const { data, isLoading, refetch } = useGetVacanciesQuery({ page, pageSize, searchTerm: searchValue, attendanceMode: filterAttendance, experience: filterExperience, location: filterLocation, skill: filterSkill, category: filterCategory });
    const navigate = useNavigate();

    useEffect(() => {
        getLocations();
        getSkills();
        getCategories();
    }, [])

    if (isLoading) {
        return <p>Loading...</p>;
    }

    const handleCreateVacancy = () => {
        navigate(`/vacancy/create`);
    }

    const handleMyVacanciesClicked = () => {
        navigate(`/vacancy/myVacancies`);
    }

    const handlePageChange = (event, value) => {
        setPage(value);
    }

    const handleSearch = (event) => {
        setPage(1);
        setSearchValue(searchTerm);
        refetch();
    }


    return (
        <>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', }} className="m-2">
                <Typography variant="h5">Vacancies</Typography>
                {token.role == Role[Role.Recruiter] ?
                    <div>
                        <Button variant="contained" className="mx-1" onClick={() => handleMyVacanciesClicked()}>
                            My vacancies
                        </Button>
                        <Button variant="contained" onClick={() => handleCreateVacancy()}>
                            Create Vacancy
                        </Button>
                    </div>
                    : null
                }
            </div>
            <div className="m-2">
                <TextField
                    label="Search"
                    variant="outlined"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                />
                <Button variant="contained" className="ml-2" onClick={handleSearch}>
                    Search
                </Button>
            </div>
            <div className="flex row m-2">
                <FormControl variant="outlined" className="mr-2 col">
                    <InputLabel id="attendance-label">Attendance</InputLabel>
                    <Select
                        label="Attendance"
                        value={filterAttendance}
                        onChange={(e) => {
                            setFilterAttendance(Number(e.target.value))
                        }}
                        labelId="attendance-label"
                    >
                        {Object.values(AttendanceMode).filter((v) => isNaN(Number(v))).map((value) => (
                            <MenuItem key={value} value={AttendanceMode[value]}>
                                {value}
                            </MenuItem>
                        ))}
                        <MenuItem value={undefined}>Clear</MenuItem>
                    </Select>
                </FormControl>

                <FormControl variant="outlined" className="mx-2 col">
                    <InputLabel id="experience-label">Experience</InputLabel>
                    <Select
                        label="Experience"
                        value={filterExperience}
                        onChange={(e) => setFilterExperience(Number(e.target.value))}
                        labelId="experience-label"
                    >
                        {Object.values(Experience).filter((v) => isNaN(Number(v))).map((value) => (
                            <MenuItem key={value} value={Experience[value]}>
                                {value}
                            </MenuItem>
                        ))}
                        <MenuItem value={undefined}>Clear</MenuItem>
                    </Select>
                </FormControl>

                <FormControl variant="outlined" className="mx-2 col">
                    <InputLabel id="location-label">Location</InputLabel>
                    <Select
                        label="Location"
                        value={filterLocation}
                        onChange={(e) => setFilterLocation(e.target.value)}
                        labelId="location-label"
                    >
                        {locations && locations.map((location) => <MenuItem key={location.id} value={location.id}>{location.city}, {location.country}</MenuItem>)}
                        <MenuItem value={''}>Clear</MenuItem>
                    </Select>
                </FormControl>

                <FormControl variant="outlined" className="mx-2 col">
                    <InputLabel id="skill-label">Skill</InputLabel>
                    <Select
                        label="Skill"
                        value={filterSkill}
                        onChange={(e) => setFilterSkill(e.target.value)}
                        labelId="skill-label"
                    >
                        {skills && skills.map((skill) => <MenuItem key={skill.id} value={skill.id}>{skill.name}</MenuItem>)}
                        <MenuItem value={''}>Clear</MenuItem>
                    </Select>
                </FormControl>
                <FormControl variant="outlined" className="ml-2 col">
                    <InputLabel id="category-label">Category</InputLabel>
                    <Select
                        label="Category"
                        value={filterCategory}
                        onChange={(e) => setFilterCategory(e.target.value)}
                        labelId="category-label"
                    >
                        {categories && categories.map((category) => <MenuItem key={category.id} value={category.id}>{category.name}</MenuItem>)}
                        <MenuItem value={''}>Clear</MenuItem>
                    </Select>
                </FormControl>
            </div>
            <div style={{ flex: 1, marginLeft: 'auto' }}>
                {data && data.map((vacancy) => <VacancyTile key={vacancy.id} vacancy={vacancy} isRecruiterList={false} />)}
                <div className="m-2" style={{ display: 'flex', justifyContent: 'center' }}>
                    <Pagination
                        count={Math.ceil((data?.length - 1) / pageSize) + 1}
                        page={page}
                        onChange={handlePageChange}
                        color="primary"
                    />
                </div>

            </div>
        </>
    )
}