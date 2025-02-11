import { useEffect, useState } from "react";
import { useGetCandidatesProfileQuery } from "../../app/features/candidate/candidate.api"
import { useLazyGetProfileLocationQuery, useLazyGetProfileSkillsQuery } from "../../app/features/profile/profile.api";
import { CandidateTile } from "../../components/candidate.tile";
import { AttendanceMode } from "../../models/common/attendance.enum";
import { Experience } from "../../models/vacancy/experience.enum";
import { TextField, FormControl, InputLabel, Select, MenuItem, Button, Pagination } from "@mui/material";

export function CandidateList() {

    const [searchTerm, setSearchTerm] = useState('');
    const [searchValue, setSearchValue] = useState('');
    const [page, setPage] = useState(1);
    const [getLocations, { data: locations }] = useLazyGetProfileLocationQuery();
    const [getSkills, { data: skills }] = useLazyGetProfileSkillsQuery();
    const pageSize = 5;
    const [filterAttendance, setFilterAttendance] = useState<AttendanceMode>();
    const [filterExperience, setFilterExperience] = useState<Experience>();
    const [filterLocation, setFilterLocation] = useState('');
    const [filterSkill, setFilterSkill] = useState('');
    const { data: candidates, isLoading, isError, error, refetch } = useGetCandidatesProfileQuery({ page, pageSize, searchTerm: searchValue, attendanceMode: filterAttendance, experience: filterExperience, location: filterLocation, skill: filterSkill })

    useEffect(() => {
        getLocations();
        getSkills();
    }, [])

    if (isLoading) {
        return <div>Loading...</div>
    }

    if (isError) {
        return <p>Error: {JSON.stringify(error.data)}</p>;
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
                            debugger;
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
            </div>
            {candidates && candidates?.length > 0
                ? candidates.map(candidate => {
                    return <CandidateTile key={candidate.id} profile={candidate} />
                })
                : <p>No candidates found</p>}
            <div className="m-2" style={{ display: 'flex', justifyContent: 'center' }}>
                <Pagination
                    count={Math.ceil((candidates?.length - 1) / pageSize) + 1}
                    page={page}
                    onChange={handlePageChange}
                    color="primary"
                />
            </div>
        </>
    )
}