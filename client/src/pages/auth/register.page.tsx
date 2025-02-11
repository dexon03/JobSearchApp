import { Container, Typography, TextField, Radio, FormControlLabel, Button, RadioGroup } from '@mui/material';
import { Formik, Field, Form, ErrorMessage } from 'formik';
import { useNavigate } from 'react-router-dom';
import * as Yup from 'yup';
import { ApiServicesRoutes } from '../../api/api.services.routes';
import { RestClient } from '../../api/rest.client';
import useToken from '../../hooks/useToken';
import { TokenResponse } from '../../models/auth/jwt.respone';
import { RegisterModel } from '../../models/auth/register.model';
import { Role } from '../../models/common/role.enum';
import { useState } from 'react';
import { useAppDispatch } from '../../hooks/redux.hooks';
import { setCandidateProfile, setRecruiterProfile } from '../../app/slices/profile.slice';

function RegisterPage() {
    const [selectedRole, setSelectedRole] = useState(0);
    const { _, setToken } = useToken();
    const navigate = useNavigate();
    const restClient = new RestClient();
    const dispatch = useAppDispatch();
    const handleRoleChange = (event) => {
        setSelectedRole(event.target.value);
    };

    const onSubmit = async (values) => {
        const token = await restClient.post<TokenResponse>(ApiServicesRoutes.identity + '/auth/register', {
            email: values.email,
            password: values.password,
            firstName: values.firstName,
            lastName: values.lastName,
            phoneNumber: values.phoneNumber,
            role: selectedRole === 0 ? Role.Recruiter : Role.Candidate,
        } as RegisterModel);

        setToken(token);
        if (token.role === Role[Role.Candidate]) {
            navigate('/vacancy');
            dispatch(setCandidateProfile(await restClient.get(ApiServicesRoutes.profile + `/profile/${Role.Candidate}/${token.userId}`)));
        } else {
            navigate('/candidate');
            dispatch(setRecruiterProfile(await restClient.get(ApiServicesRoutes.profile + `/profile/${Role.Recruiter}/${token.userId}`)));
        }
    }

    const validationSchema = Yup.object().shape({
        firstName: Yup.string().required('Required'),
        lastName: Yup.string().required('Required'),
        email: Yup.string().email('Invalid email').required('Required'),
        phoneNumber: Yup.string().required('Required'),
        password: Yup.string()
            .required('Required')
            .min(8, 'Must be 8 characters or more')
            .matches(/(?=.*[A-Z])/, 'At least one uppercase character')
            .matches(/(?=.*[0-9])/, 'At least one numeric character')
            .matches(/(?=.*[^0-9a-zA-Z])/, 'At least one non-numeric character'),
        role: Yup.number().required('Required')
    });

    return (
        <Container maxWidth="sm">
            <Typography variant="h4" align="center" gutterBottom>
                Register
            </Typography>
            <Formik
                enableReinitialize={true}
                initialValues={{ firstName: '', lastName: '', email: '', phoneNumber: '', password: '', role: 0 }}
                validationSchema={validationSchema}
                onSubmit={onSubmit}
            >
                {() => (
                    <Form>
                        <Field name="firstName" type="text" label="First name" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="firstName" />
                        <Field name="lastName" type="text" label="Last name" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="lastName" />
                        <Field name="email" type="email" label="Email" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="email" />
                        <Field name="phoneNumber" type="text" label="Phone number" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="phoneNumber" />
                        <Field name="password" type="password" label="Password" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="password" />
                        <RadioGroup
                            aria-label="role"
                            name="role"
                            value={selectedRole}
                            onChange={handleRoleChange}
                        >
                            <FormControlLabel value={0} control={<Radio />} label="I am recruiter" />
                            <FormControlLabel value={1} control={<Radio />} label="I am candidate" />
                        </RadioGroup>

                        <ErrorMessage name="role" />
                        <Button
                            variant="contained"
                            color="primary"
                            fullWidth
                            type="submit"
                            size="large"
                            style={{ marginTop: 16 }}
                        >
                            Sign Up
                        </Button>
                    </Form>
                )}
            </Formik>
        </Container>
    );
}

export default RegisterPage;
