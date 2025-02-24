import { Avatar, Box, Button, Container, InputLabel, MenuItem, OutlinedInput, Select, TextField, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { Document, Page } from 'react-pdf';
import { Input } from "reactstrap";
import { showErrorToast } from '../app/features/common/popup';
import { useLazyDownloadResumeQuery, useUploadResumeMutation } from '../app/features/profile/candidateResume.api';
import { useGetUserCandidateProfileQuery, useLazyGetProfileLocationQuery, useLazyGetProfileSkillsQuery, useQuerySubscriptionCandidate, useUpdateCandidateProfileMutation } from '../app/features/profile/profile.api';
import { setCandidateProfile } from '../app/slices/profile.slice';
import { useAppDispatch } from '../hooks/redux.hooks';
import { CandidateProfile } from '../models/profile/candidate.profile.model';
import { Experience } from '../models/profile/experience.enum';

interface CandidateProfileState {
  name: string;
  surname: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: Date;
  description: string;
  gitHubUrl: string;
  linkedInUrl: string;
  positionTitle: string;
  isActive: boolean;
  desiredSalary: number;
  workExperience: Experience;
  selectedLocations: string[];
  selectedSkills: string[];
}

const CandidateProfileComponent = () => {
  const { data: profile } = useGetUserCandidateProfileQuery();
  const [getProfileSKills, { data: skills }] = useLazyGetProfileSkillsQuery();
  const [getProfileLocations, { data: locations }] = useLazyGetProfileLocationQuery();
  const { refetch } = useQuerySubscriptionCandidate();
  const [updateCandidateProfile, { data: updatedProfile, error: updateError }] = useUpdateCandidateProfileMutation();
  const [uploadResume] = useUploadResumeMutation();
  const [downloadResume] = useLazyDownloadResumeQuery();

  const [profileState, setProfileState] = useState<CandidateProfileState>({
    name: '',
    surname: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: new Date(),
    description: '',
    gitHubUrl: '',
    linkedInUrl: '',
    positionTitle: '',
    isActive: false,
    desiredSalary: 0,
    workExperience: Experience.NoExperience,
    selectedLocations: [],
    selectedSkills: [],
  });

  const [selectedFile, setSelectedFile] = useState<Blob | null>(null);
  const [numPages, setNumPages] = useState<number>();
  const dispatch = useAppDispatch();

  useEffect(() => {
    if (profile) {
      getProfileSKills();
      getProfileLocations();
      setProfileState({
        name: profile.name || '',
        surname: profile.surname || '',
        email: profile.email || '',
        phoneNumber: profile.phoneNumber || '',
        dateOfBirth: profile.dateBirth || new Date(),
        description: profile.description || '',
        gitHubUrl: profile.gitHubUrl || '',
        linkedInUrl: profile.linkedInUrl || '',
        positionTitle: profile.positionTitle || '',
        isActive: profile.isActive,
        desiredSalary: profile.desiredSalary || 0,
        workExperience: profile.workExperience || Experience.NoExperience,
        selectedLocations: profile.locations ? profile.locations.map(location => location.id) : [],
        selectedSkills: profile.skills ? profile.skills.map(skill => skill.id) : [],
      });
      downloadResume(profile.id).then((response) => {
        if (response.data) {
          const file = new Blob([response.data], { type: 'application/pdf' });
          setSelectedFile(file);
        }
      });
      dispatch(setCandidateProfile(profile));
    }
  }, [dispatch, downloadResume, getProfileLocations, getProfileSKills, profile]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    console.log(profileState.workExperience);
    e.preventDefault();
    try {
      await updateCandidateProfile({
        id: profile?.id,
        name: profileState.name,
        surname: profileState.surname,
        email: profileState.email,
        phoneNumber: profileState.phoneNumber,
        dateBirth: profileState.dateOfBirth,
        description: profileState.description,
        gitHubUrl: profileState.gitHubUrl,
        linkedInUrl: profileState.linkedInUrl,
        positionTitle: profileState.positionTitle,
        isActive: profileState.isActive,
        desiredSalary: profileState.desiredSalary,
        workExperience: profileState.workExperience,
        imageUrl: undefined,
        skills: skills?.filter(skill => profileState.selectedSkills.includes(skill.id)),
        locations: locations?.filter(location => profileState.selectedLocations.includes(location.id))
      } as CandidateProfile);

      if (updatedProfile) {
        refetch();
      }
    } catch (error) {
      console.error("Error updating profile:", updateError);
    }

  };

  const handleFileChange = (e: { target: HTMLInputElement; }) => {
    const fileInput = e.target as HTMLInputElement;
    const selectedFile = fileInput.files && fileInput.files[0];
    if (!selectedFile) {
      showErrorToast("Please upload a file")
      return;
    }
    const extension = selectedFile!.name.split('.').pop()?.toLowerCase();

    if (extension !== 'pdf') {
      showErrorToast("File must be pdf");
      e.target.value = '';
    }
    else {
      setSelectedFile(selectedFile);
    }
  }

  const handleUploadFile = async () => {
    if (profile && selectedFile) {
      const formData = new FormData();
      formData.append('candidateId', profile.id);
      formData.append('resume', selectedFile);
      await uploadResume(formData);
    }
  }

  function onDocumentLoadSuccess({ numPages }: { numPages: number }): void {
    setNumPages(numPages);
  }

  const handleInputChange = (field: keyof CandidateProfileState) => (
    e: React.ChangeEvent<HTMLInputElement | { value: unknown }>
  ) => {
    setProfileState(prev => ({
      ...prev,
      [field]: e.target.value
    }));
  };

  return (
    profile &&
    <Container component="main" maxWidth="sm">
      <div>
        <Avatar> {/* Add user avatar here */}</Avatar>
        <Typography component="h1" variant="h5">
          Profile
        </Typography>
        <form onSubmit={handleSubmit}>
          <TextField
            label="Name"
            margin="normal"
            fullWidth
            required
            value={profileState.name}
            onChange={handleInputChange('name')}
          />
          <TextField
            label="Surname"
            margin="normal"
            fullWidth
            required
            value={profileState.surname}
            onChange={handleInputChange('surname')}
          />
          <TextField
            label="Position Title"
            margin="normal"
            fullWidth
            value={profileState.positionTitle}
            onChange={handleInputChange('positionTitle')}
          />
          <TextField
            label="Email"
            margin="normal"
            fullWidth
            required
            value={profileState.email}
            onChange={handleInputChange('email')}
          />
          <TextField
            label="Phone Number"
            margin="normal"
            fullWidth
            value={profileState.phoneNumber}
            onChange={handleInputChange('phoneNumber')}
          />
          <TextField
            label="Date of Birth"
            type="date"
            margin="normal"
            fullWidth
            InputLabelProps={{
              shrink: true,
            }}
            value={profileState.dateOfBirth}
            onChange={handleInputChange('dateOfBirth')}
          />
          <TextField
            label="Desired Salary"
            margin="normal"
            type='number'
            fullWidth
            value={profileState.desiredSalary}
            onChange={(e) => setProfileState(prev => ({
              ...prev,
              desiredSalary: Number(e.target.value)
            }))}
          />
          <TextField
            select
            label="Work Experience"
            margin="normal"
            fullWidth
            defaultValue={Experience.NoExperience}
            value={profileState.workExperience}
            onChange={(e) => {
              setProfileState(prev => ({
                ...prev,
                workExperience: Number(e.target.value) as Experience
              }))
            }}
          >
            {Object.values(Experience).filter((v) => isNaN(Number(v))).map((value) => (
              <MenuItem key={value} value={Experience[value as keyof typeof Experience]}>
                {value}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Description"
            multiline
            rows={4}
            margin="normal"
            fullWidth
            value={profileState.description}
            onChange={handleInputChange('description')}
          />
          <InputLabel>Locations</InputLabel>
          <Select
            multiple
            fullWidth
            value={profileState.selectedLocations}
            onChange={(e) => setProfileState(prev => ({
              ...prev,
              selectedLocations: e.target.value as string[]
            }))}
            input={<OutlinedInput label="Locations" />}
          >
            {locations && locations.map((location) => (
              <MenuItem key={location.id} value={location.id}>
                {location.city}, {location.country}
              </MenuItem>
            ))}
          </Select>
          <InputLabel>Skills</InputLabel>
          <Select
            multiple
            fullWidth
            value={profileState.selectedSkills}
            onChange={(e) => setProfileState(prev => ({
              ...prev,
              selectedSkills: e.target.value as string[]
            }))}
            input={<OutlinedInput label="Skills" />}
          >
            {skills && skills.map((skill) => (
              <MenuItem key={skill.id} value={skill.id}>
                {skill.name}
              </MenuItem>
            ))}
          </Select>
          <Button type="submit" className='mt-3' fullWidth variant="contained" color="primary">
            Save
          </Button>
          <Box marginTop={'2em'}>
            <InputLabel htmlFor="resume">Upload Resume (PDF, with non-cyrillic characters)</InputLabel>
            <Input
              id="handleFileChange"
              name="handleFileChange"
              type="file"
              accept=".pdf"
              onChange={handleFileChange}
            />
            <Button className="my-1" variant="contained" fullWidth color="primary" onClick={handleUploadFile}>
              Upload Resume
            </Button>
            {selectedFile !== null && selectedFile.size > 0 ?
              <div className='mt-2' style={{ height: '700px', overflowY: 'scroll', border: '1px solid #ccc', marginBottom: '20px', borderRadius: '7px' }}>
                <Document file={selectedFile} onLoadSuccess={onDocumentLoadSuccess}>
                  {[...Array(numPages)].map((_, i) => (
                    <Page key={i + 1} pageNumber={i + 1} renderTextLayer={false} renderAnnotationLayer={false} />
                  ))}
                </Document>
              </div>
              : null
            }
          </Box>
        </form>
      </div>
    </Container>
  );
}

export default CandidateProfileComponent;