import { Experience } from "../vacancy/experience.enum";
import { AttendanceMode } from "./attendance.enum";

export interface CandidateFilter {
    searchTerm?: string;
    page: number;
    pageSize: number;
    experience?: Experience;
    attendanceMode?: AttendanceMode;
    skill?: string;
    location?: string;
}