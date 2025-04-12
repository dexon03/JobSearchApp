import { Experience } from "./experience.enum";

export interface GenerateVacancyDescriptionRequest {
    experience: Experience;
    position: string;
    description?: string;
}