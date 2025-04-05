import { Avatar, Button, Checkbox, Container, Divider, FormControlLabel, InputLabel, MenuItem, OutlinedInput, Select, TextField, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { showSuccessToast } from '../app/features/common/popup';
import { useCreateCompanyMutation, useLazyGetProfileCompaniesQuery, useUpdateCompanyMutation } from '../app/features/company/company.api';
import { useGetUserRecruiterProfileQuery, useUpdateRecruiterProfileMutation } from '../app/features/profile/profile.api';
import { setRecruiterProfile } from '../app/slices/profile.slice';
import { useAppDispatch } from '../hooks/redux.hooks';
import { Company } from '../models/common/company.models';
import { UpdateRecruiterProfileModel } from '../models/profile/updateRecruiterProfileModel';
import { SelectChangeEvent } from '@mui/material/Select';

const RecruiterProfileComponent = () => {
  const { data: profile, isLoading, refetch } = useGetUserRecruiterProfileQuery();
  const [getCompanyQuery, { data: companies }] = useLazyGetProfileCompaniesQuery();
  const [updateRectuterProfile, { error: updateError }] = useUpdateRecruiterProfileMutation();
  const [updateCompany] = useUpdateCompanyMutation();
  const [createCompany] = useCreateCompanyMutation();
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
  const [selectedCompany, setSelectedCompany] = useState<number | null>(null);
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
      setPositionTitle(profile.positionTitle || '');
      setIsActive(profile.isActive);
      setSelectedCompany(profile?.company?.id ?? null);
      setCompanyName(profile?.company ? profile.company.name : '');
      setCompanyDescription(profile?.company ? profile.company.description : '');
      dispatch(setRecruiterProfile(profile));
    }
  }, [profile])

  useEffect(() => {
    if (selectedCompany && companies) {
      onCompanyChanged(selectedCompany);
    }
  }, [selectedCompany, companies])

  const onCompanyChanged = (companyId: number) => {
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

  const handleCompanySelection = (event: SelectChangeEvent<number | "new">) => {
    const selectedValue = event.target.value;
    if (selectedValue === 'new') {
      setIsNewCompany(true);
      setCompanyName('');
      setCompanyDescription('');
    } else {
      setIsNewCompany(false);
      onCompanyChanged(Number(selectedValue));
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
          showSuccessToast("Company created successfully");
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
      showSuccessToast("Company updated successfully");
    }
  };

  if (isLoading) {
    return <p>Loading...</p>;
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      const result = await updateRectuterProfile({
        id: profile!.id,
        name,
        surname,
        email,
        phoneNumber,
        dateBirth: dateOfBirth,
        description,
        positionTitle,
        isActive,
        companyId: selectedCompany
      } as UpdateRecruiterProfileModel);

      await refetch();
      if ('data' in result) {
        showSuccessToast("Profile updated successfully");
      }
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
          {(selectedCompany || isNewCompany) ?
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