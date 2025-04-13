import { Experience } from "./experience.enum";

export type GenerateProfileDescriptionRequest = {
    description: string;
    experience: Experience;
    positionTitle: string;
}