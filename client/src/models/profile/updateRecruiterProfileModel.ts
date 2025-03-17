import { Profile } from './profile';

export type UpdateRecruiterProfileModel = Profile & {
    companyId?: number | null;
}