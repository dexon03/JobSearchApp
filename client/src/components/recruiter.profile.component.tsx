import { TextField, Button, Container, Typography, Avatar, Checkbox, FormControlLabel, InputLabel, MenuItem, OutlinedInput, Select, Divider } from '@mui/material';
import { useGetUserRecruiterProfileQuery, useUpdateRecruiterProfileMutation } from '../app/features/profile/profile.api';
import { useEffect, useState } from 'react';
import { useCreateCompanyMutation, useLazyGetProfileCompaniesQuery, useUpdateCompanyMutation } from '../app/features/company/company.api';
import { Company } from '../models/common/company.models';
import { useAppDispatch } from '../hooks/redux.hooks';
import { setRecruiterProfile } from '../app/slices/profile.slice';

const RecruiterProfileComponent = ({ id }: { id: string }) => {
  const { data: profile, isLoading, refetch } = useGetUserRecruiterProfileQuery(id);
  const [getCompanyQuery, { data: companies }] = useLazyGetProfileCompaniesQuery();
  const [updateCandidateProfile, { error: updateError }] = useUpdateRecruiterProfileMutation();
  const [updateCompany] = useUpdateCompanyMutation();
  const [createCompany, { data: createdCompany }] = useCreateCompanyMutation();
  const dispatch = useAppDispatch();

  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState(new Date());
  const [description, setDescription] = useState('');
  const [linkedInUrl, setLinkedInUrl] = useState('');
  const [positionTitle, setPositionTitle] = useState('');
  const [isActive, setIsActive] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<string>('');
  const [companyName, setCompanyName] = useState('');
  const [companyDescription, setCompanyDescription] = useState('');
  const [isNewCompany, setIsNewCompany] = useState(false);


  useEffect(() => {
    if (profile) {
      getCompanyQuery();
      setName(profile.name || '');
      setSurname(profile.surname || '');
      setEmail(profile.email || '');
      setPhoneNumber(profile.phoneNumber || '');
      setDateOfBirth(profile.dateBirth!);
      setDescription(profile.description || '');
      setLinkedInUrl(profile.linkedInUrl || '');
      setPositionTitle(profile.positionTitle || '');
      setIsActive(profile.isActive);
      setSelectedCompany(profile.company ? profile.company.id : '');
      setCompanyName(profile?.company ? profile.company.name : '');
      setCompanyDescription(profile?.company ? profile.company.description : '');
      onCompanyChanged(selectedCompany)
      dispatch(setRecruiterProfile(profile));
    }
  }, [profile])

  const onCompanyChanged = (companyId: string) => {
    try {
      setSelectedCompany(companyId);
      const company = companies?.find(company => company.id === companyId);
      setCompanyName(company?.name || '');
      setCompanyDescription(company?.description || '');
    }
    catch (error) {
      console.error("Error getting company:", error);
    }
  }

  const handleCompanySelection = (e) => {
    const selectedValue: string = e.target.value;
    if (selectedValue === 'new') {
      setIsNewCompany(true);
      setCompanyName('');
      setCompanyDescription('');
    } else {
      setIsNewCompany(false);
      onCompanyChanged(selectedValue);
    }
  };

  const handleUpdateOrAddCompany = async () => {
    if (isNewCompany) {
      try {
        const response = await createCompany({
          name: companyName,
          description: companyDescription,
        } as Company);
        if (!response.error) {
          setSelectedCompany(response.data.id);
        }
      } catch (error) {
        console.error("Error creating company:", error);
      }
    } else {
      try {
        await updateCompany({
          id: selectedCompany,
          name: companyName,
          description: companyDescription,
        } as Company);
      } catch (error) {
        console.error("Error updating company:", error);
      }
    }
  };

  useEffect(() => {
    onCompanyChanged(selectedCompany);
  }, [selectedCompany])

  if (isLoading) {
    return <p>Loading...</p>;
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await updateCandidateProfile({
        id: profile.id,
        name,
        surname,
        email,
        phoneNumber,
        dateBirth: dateOfBirth,
        description,
        linkedInUrl,
        positionTitle,
        isActive,
        companyId: selectedCompany
      });
      await refetch();

    } catch (error) {
      console.error("Error updating profile:", updateError);
    }
  };


  return (
    <Container component="main" maxWidth="sm">
      <div>
        <Avatar> {/* Add user avatar here */}</Avatar>
        <Typography component="h1" variant="h5">
          Profile
        </Typography>
        <form onSubmit={handleSubmit}>
          <TextField
            required
            label="Name"
            margin="normal"
            fullWidth
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <TextField
            required
            label="Surname"
            margin="normal"
            fullWidth
            value={surname}
            onChange={(e) => setSurname(e.target.value)}
          />
          <TextField
            label="Position Title"
            margin="normal"
            fullWidth
            value={positionTitle}
            onChange={(e) => setPositionTitle(e.target.value)}
          />
          <TextField
            label="Email"
            margin="normal"
            required
            fullWidth
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <TextField
            label="Phone Number"
            margin="normal"
            fullWidth
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
          />
          <TextField
            label="Date of Birth"
            type="date"
            margin="normal"
            fullWidth
            InputLabelProps={{
              shrink: true,
            }}
            value={dateOfBirth}
            onChange={(e) => setDateOfBirth(e.target.value)}
          />
          <TextField
            label="Description"
            multiline
            rows={4}
            margin="normal"
            fullWidth
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <TextField
            label="LinkedIn URL"
            margin="normal"
            fullWidth
            value={linkedInUrl}
            onChange={(e) => setLinkedInUrl(e.target.value)}
          />
          <FormControlLabel
            control={<Checkbox color="primary" />}
            label="Active"
            value={isActive}
            onChange={(e) => setIsActive(!isActive)}
          />
          <Button type="submit" fullWidth variant="contained" color="primary">
            Save
          </Button>
          <Divider style={{ margin: '20px 0px' }} />
          <InputLabel>Company</InputLabel>
          <Select
            fullWidth
            value={isNewCompany ? 'new' : selectedCompany}
            onChange={handleCompanySelection}
            input={<OutlinedInput label="Company" />}
          >

            {companies && companies.map((company) => (
              <MenuItem key={company.id} value={company.id}>
                {company.name}
              </MenuItem>
            ))}
            <MenuItem value="new">Create New Company</MenuItem>
          </Select>
          {selectedCompany || isNewCompany ?
            <>
              <TextField
                label="Company Name"
                margin="normal"
                fullWidth
                value={companyName}
                onChange={(e) => setCompanyName(e.target.value)}
              />
              <TextField
                label="Description"
                multiline
                rows={4}
                margin="normal"
                fullWidth
                value={companyDescription}
                onChange={(e) => setCompanyDescription(e.target.value)}
              />
              {isNewCompany ?
                <>
                  <Button onClick={handleUpdateOrAddCompany} fullWidth variant="contained" color="primary">
                    Create Company
                  </Button>
                </>
                :
                <>
                  <Button onClick={handleUpdateOrAddCompany} fullWidth variant="contained" color="secondary">
                    Update Company
                  </Button>
                </>
              }
            </>
            : null
          }
        </form>
      </div>
    </Container>
  );
};

export default RecruiterProfileComponent;