import { Button, Container, TextField, Typography } from '@mui/material';
import { ErrorMessage, Field, Form, Formik } from 'formik';
import { NavLink, useNavigate } from 'react-router-dom';
import * as Yup from 'yup';
import { RestClient } from '../../api/rest.client';
import { setCandidateProfile, setRecruiterProfile } from '../../app/slices/profile.slice';
import { useAppDispatch } from '../../hooks/redux.hooks';
import useToken from '../../hooks/useToken';
import { TokenResponse } from '../../models/auth/jwt.respone';
import { LoginModel } from '../../models/auth/login.model';
import { Role } from '../../models/common/role.enum';

function LoginPage() {
    const navigate = useNavigate();
    const { setToken } = useToken();
    const restClient = new RestClient();
    const dispatch = useAppDispatch();

    const onSubmit = async (values: { email: string; password: string; }) => {
        const tokenResponse = await restClient.post<TokenResponse>('/identity/login', {
            email: values.email,
            password: values.password
        } as LoginModel);
        setToken(tokenResponse);
        const role = await restClient.get(`/role`);

        if (role === Role[Role.Candidate]) {
            dispatch(setCandidateProfile(await restClient.get(`/profile/${Role.Candidate}`)));
            navigate('/vacancy');
        } else if (role === Role[Role.Recruiter]) {
            dispatch(setRecruiterProfile(await restClient.get(`/profile/${Role.Recruiter}`)));
            navigate('/candidate');
        } else {
            navigate('/users')
        }
    }

    const validationSchema = Yup.object().shape({
        email: Yup.string().email('Invalid email').required('Required'),
        password: Yup.string().required('Required')
    });

    return (
        <Container maxWidth="sm">
            <Typography variant="h4" align="center" gutterBottom>
                Login
            </Typography>
            <Formik
                enableReinitialize={true}
                initialValues={{ email: '', password: '' }}
                validationSchema={validationSchema}
                onSubmit={onSubmit}
            >
                {() => (
                    <Form>
                        <Field name="email" type="email" label="Email" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="email" />
                        <Field name="password" type="password" label="Password" fullWidth variant="outlined" margin="normal" as={TextField} />
                        <ErrorMessage name="password" />
                        <Button
                            variant="contained"
                            color="primary"
                            type="submit"
                            fullWidth
                            size="large"
                            style={{ marginTop: 16 }}
                        >
                            Sign In
                        </Button>
                    </Form>
                )}
            </Formik>
            <Button
                variant="text"
                color="primary"
                style={{ marginTop: 8 }}
                component={NavLink} to="/register"
            >
                Sign Up
            </Button>
        </Container>
    );
}

export default LoginPage;
