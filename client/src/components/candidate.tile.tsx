import { Card, CardContent, Typography, Chip, Box, Button, CardActions } from "@mui/material";
import { CandidateProfile } from "../models/profile/candidate.profile.model";
import { Experience } from "../models/vacancy/experience.enum";
import { useNavigate } from "react-router-dom";
import { AttendanceMode } from "../models/common/attendance.enum";

export function CandidateTile({ profile }: { profile: CandidateProfile }) {
    const countries = [...new Set(profile?.locations?.map(location => location.country))].join(', ');
    const navigate = useNavigate();

    const handleViewClick = () => {
        navigate('/candidate/' + profile.id);
    }

    return (
        profile &&
        <Card className="m-2">
            <CardContent style={{ display: 'flex', flexDirection: 'column' }}>
                <div style={{ flexGrow: 1 }}>
                    <Typography variant="h5">{profile.positionTitle}</Typography>
                    <Typography variant="body2">{countries}, {Experience[profile.workExperience]}, {AttendanceMode[profile.attendance]}</Typography>
                    <Typography variant="h6" align="right" style={{ color: 'green' }}>{profile.desiredSalary} USD</Typography>
                    <Typography variant="body1">{profile.description.slice(0, 100)}{profile.description.length > 100 ? '...' : null}</Typography>
                    <Box m={2} />
                    {profile.skills && profile.skills.length > 0 ? profile.skills.map((skill) => (
                        <Chip label={skill.name} variant="outlined" key={skill.id} />
                    )) : <Chip label="No skills" variant="outlined" key={0} />}
                </div>
                <Button variant="contained" color="primary" onClick={handleViewClick} style={{ alignSelf: 'flex-end' }}>
                    View
                </Button>
            </CardContent>
        </Card>
    );
}   