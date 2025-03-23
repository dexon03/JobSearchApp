import { Experience } from "../vacancy/experience.enum";
import { AttendanceMode } from "./attendance.enum";

export interface VacancyFilter {
    searchTerm?: string;
    page: number;
    pageSize: number;
    experience?: Experience | null;
    attendanceMode?: AttendanceMode | null;
    skill?: string;
    category?: string;
    location?: string;
}