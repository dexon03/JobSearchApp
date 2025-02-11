import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip, Legend } from "recharts";
import { useGetMockedStatisticQuery, useGetStatisticQuery, useGetVacancySkillsQuery, useLazyGetStatisticQuery } from "../../app/features/vacancy/vacancy.api";
import { Container, Grid, Paper, Typography, Button } from "@mui/material";
import { useEffect, useState } from "react";
import { StatisticDataModeEnum } from "../../models/statistic/data.mode.enum";
import { SkillDto } from "../../models/common/skill.dto";

export function StatisticPage() {
    const { data: mockedVacancies, isLoading: isMockedLoading } = useGetMockedStatisticQuery();
    const [getStatistic, { data: realVacancies }] = useLazyGetStatisticQuery();
    const { data: skills, isLoading: isSkillLoading } = useGetVacancySkillsQuery();
    const [selectedSkill, setSelectedSkill] = useState<string>('');
    const [mode, setMode] = useState<StatisticDataModeEnum>(StatisticDataModeEnum.Mocked);

    useEffect(() => {
        if (mode === StatisticDataModeEnum.Real && !realVacancies) {
            getStatistic(selectedSkill);
        }
    }, [mode, realVacancies, selectedSkill]);

    const handleSkillClick = (skill: SkillDto) => {
        setSelectedSkill(skill.id);
        if (mode === StatisticDataModeEnum.Real) {
            let skillName: string;
            switch (skill.name) {
                case "C#": skillName = "C%23";
                    break;
                case "C++": skillName = "C%2B%2B";
                    break;
                default: skillName = skill.name;
                    break;
            };
            getStatistic(skill.name);
        }
    }

    const handleModeSwitch = () => {
        setMode(mode === StatisticDataModeEnum.Mocked ? StatisticDataModeEnum.Real : StatisticDataModeEnum.Mocked);
    };

    if (isMockedLoading || isSkillLoading) {
        return <div>Loading...</div>;
    }

    const vacancies = mode === StatisticDataModeEnum.Mocked ? mockedVacancies : realVacancies;

    return (
        <Container component="main" maxWidth="md">
            <h1 className="m-3">Statistic</h1>

            <Button variant="contained" color="primary" onClick={handleModeSwitch}>
                {mode === StatisticDataModeEnum.Mocked ? "Switch to Real Data" : "Switch to Mocked Data"}
            </Button>

            <Grid container spacing={2} sx={{ my: 1 }}>
                {skills && skills.map((skill) => (
                    <Grid item xs={6} sm={4} md={3}>
                        <Paper
                            onClick={() => handleSkillClick(skill)}
                            style={{ padding: '10px', cursor: 'pointer', backgroundColor: skill.id === selectedSkill ? '#e0e0e0' : 'white' }}
                        >
                            <Typography variant="body1">{skill.name}</Typography>
                        </Paper>
                    </Grid>
                ))}
            </Grid>

            <LineChart width={800} height={500} data={vacancies} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="salary" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="date" />
                <YAxis dataKey="salary" />
                <Tooltip />
                <Legend />
            </LineChart>
        </Container>
    );
}
