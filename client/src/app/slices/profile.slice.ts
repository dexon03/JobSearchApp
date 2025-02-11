import { CandidateProfile } from '../../models/profile/candidate.profile.model';
import { RecruiterProfile } from '../../models/profile/recruiter.profile.model';
import { createSlice } from "@reduxjs/toolkit";

export const ProfileSlice = createSlice({
    name: 'profile',
    initialState: {
        recruiterProfile: undefined as RecruiterProfile | undefined,
        candidateProfile: undefined as CandidateProfile | undefined
    },
    reducers: {
        setRecruiterProfile: (state, action) => {
            state.recruiterProfile = action.payload;
        },
        setCandidateProfile: (state, action) => {
            state.candidateProfile = action.payload;
        },
        resetProfile: (state) => {
            state.recruiterProfile = undefined;
            state.candidateProfile = undefined;
        }
    }
});

export const { setRecruiterProfile, setCandidateProfile, resetProfile } = ProfileSlice.actions;

export default ProfileSlice.reducer;