import { AttendanceMode } from "../common/attendance.enum";
import { Company } from "../common/company.models";
import { LocationDto } from "../common/location.dto";
import { SkillDto } from "../common/skill.dto";
import { Category } from "./category.model";
import { Experience } from "./experience.enum";

export interface VacancyGet {
    id: string;
    recruiterId: string;
    title: string;
    positionTitle: string;
    description: string;
    salary: number;
    experience: Experience;
    attendanceMode: AttendanceMode;
    createdAt: Date;
    updatedAt: Date;
    company: Company;
    category: Category;
    locations: LocationDto[];
    skills: SkillDto[];
}