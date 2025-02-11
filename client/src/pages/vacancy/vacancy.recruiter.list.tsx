import { Button, FormControl, InputLabel, MenuItem, Pagination, Select, TextField, Typography } from "@mui/material";
import { useGetRecruiterVacanciesQuery, useLazyGetVacancyCategoriesQuery, useLazyGetVacancyLocationQuery, useLazyGetVacancySkillsQuery } from "../../app/features/vacancy/vacancy.api";
import { VacancyTile } from "../../components/vacancy.tile";
import { useAppSelector } from "../../hooks/redux.hooks";
import { useEffect, useState } from "react";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { Experience } from "../../models/vacancy/experience.enum";

export function RecruiterVacanciesList() {
    const [searchTerm, setSearchTerm] = useState('');
    const [searchValue, setSearchValue] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 5;
    const [getLocations, { data: locations }] = useLazyGetVacancyLocationQuery();
    const [getSkills, { data: skills }] = useLazyGetVacancySkillsQuery();
    const [getCategories, { data: categories }] = useLazyGetVacancyCategoriesQuery();
    const recruiterProfileId: string = useAppSelector(state => state.profile.recruiterProfile?.id)
    const [filterAttendance, setFilterAttendance] = useState<AttendanceMode>();
    const [filterExperience, setFilterExperience] = useState<Experience>();
    const [filterLocation, setFilterLocation] = useState('');
    const [filterSkill, setFilterSkill] = useState('');
    const [filterCategory, setFilterCategory] = useState('');
    const { data, isError, isLoading, error, refetch } = useGetRecruiterVacanciesQuery({ recruiterId: recruiterProfileId, filter: { page, pageSize, searchTerm: searchValue, attendanceMode: filterAttendance, experience: filterExperience, location: filterLocation, skill: filterSkill, category: filterCategory } });


    useEffect(() => {
        getLocations();
        getSkills();
        getCategories();
    }, [])

    if (isLoading) {
        return <p>Loading...</p>
    }

    if (isError) {
        return <p>Error: {JSON.stringify(error)}</p>
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
                <Typography variant="h5">My vacancies</Typography>
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
                        {locations && locations.map((location) => <MenuItem value={location.id}>{location.city}, {location.country}</MenuItem>)}
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
                        {skills && skills.map((skill) => <MenuItem value={skill.id}>{skill.name}</MenuItem>)}
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
                        {categories && categories.map((category) => <MenuItem value={category.id}>{category.name}</MenuItem>)}
                        <MenuItem value={''}>Clear</MenuItem>
                    </Select>
                </FormControl>
            </div>
            {data && data.map((vacancy) => <VacancyTile key={vacancy.id} vacancy={vacancy} isRecruiterList={true} />)}
            <div className="m-2" style={{ display: 'flex', justifyContent: 'center' }}>
                <Pagination
                    count={Math.ceil((data?.length - 1) / pageSize) + 1}
                    page={page}
                    onChange={handlePageChange}
                    color="primary"
                />
            </div>
        </>
    )
}