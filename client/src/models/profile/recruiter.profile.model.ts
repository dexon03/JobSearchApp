import { Company } from "../common/company.models";
import { Profile } from "./profile";

export interface RecruiterProfile extends Profile {
    company?: Company;
}