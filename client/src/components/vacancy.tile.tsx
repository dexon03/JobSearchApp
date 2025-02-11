import { useNavigate } from "react-router-dom";
import { VacancyGetAll } from "../models/vacancy/vacancy.getall.dto.ts";
import { Button, Card, CardContent } from "@mui/material";
import { useActivateDisactivateVacancyMutation } from "../app/features/vacancy/vacancy.api.ts";
import { useState } from "react";

export function VacancyTile({ vacancy, isRecruiterList }: { vacancy: VacancyGetAll, isRecruiterList: boolean }) {
    const navigate = useNavigate();
    const [activateDisactivateVacancy] = useActivateDisactivateVacancyMutation();
    const [isActivated, setIsActivated] = useState<boolean>(vacancy.isActive);

    const handleActivateDeactivateClick = () => {
        activateDisactivateVacancy(vacancy.id)
        setIsActivated(!isActivated);
    };

    const handleViewClick = () => {
        navigate(`/vacancy/${vacancy.id}`);
    };

    const handlerEditClick = () => {
        navigate(`/vacancy/edit/${vacancy.id}`);
    }

    const cardClassName = isActivated ? 'm-2' : 'm-2 bg-light';

    return (
        <Card className={cardClassName}>
            <CardContent style={{ display: 'flex', flexDirection: 'column' }}>
                <h2 className="fw-bold">{vacancy.title}</h2>
                <h4>Company: {vacancy.companyName}</h4>
                <p>{vacancy.attendanceMode}</p>
                <p>{vacancy.experience}</p>
                {vacancy.locations.map((location) => (
                    <p key={location.id}>{location.city}, {location.country}</p>
                ))}
                <p className="text-success">{vacancy.salary}$</p>
                <p>{vacancy.description}</p>
                {isRecruiterList ? <>
                    <Button
                        variant="contained"
                        onClick={handlerEditClick}
                        color="primary"
                        style={{ alignSelf: 'flex-end' }}>
                        Edit
                    </Button>
                    <Button
                        variant="contained"
                        color="primary"
                        style={{ alignSelf: 'flex-end' }}
                        onClick={handleActivateDeactivateClick}
                    >
                        {isActivated ? "Deactivate" : "Activate"}
                    </Button>
                </> : null
                }
                <Button
                    variant="contained"
                    color="primary"
                    style={{ alignSelf: 'flex-end' }}
                    onClick={handleViewClick}
                >
                    View
                </Button>
            </CardContent>
        </Card>
    );
}