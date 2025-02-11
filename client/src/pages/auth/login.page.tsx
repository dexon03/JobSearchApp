import { Container, Typography, TextField, Button } from '@mui/material';
import { Formik, Field, Form, ErrorMessage } from 'formik';
import { useNavigate, NavLink } from 'react-router-dom';
import * as Yup from 'yup';
import { ApiServicesRoutes } from '../../api/api.services.routes';
import { RestClient } from '../../api/rest.client';
import useToken from '../../hooks/useToken';
import { TokenResponse } from '../../models/auth/jwt.respone';
import { LoginModel } from '../../models/auth/login.model';
import { Role } from '../../models/common/role.enum';
import { useAppDispatch } from '../../hooks/redux.hooks';
import { setCandidateProfile, setRecruiterProfile } from '../../app/slices/profile.slice';

function LoginPage() {
    const navigate = useNavigate();
    const { token, setToken } = useToken();
    const restClient = new RestClient();
    const dispatch = useAppDispatch();

    const onSubmit = async (values) => {
        const tokenResponse = await restClient.post<TokenResponse>(ApiServicesRoutes.identity + '/auth/login', {
            email: values.email,
            password: values.password
        } as LoginModel);
        setToken(tokenResponse);
        if (tokenResponse.role === Role[Role.Candidate]) {
            dispatch(setCandidateProfile(await restClient.get(ApiServicesRoutes.profile + `/profile/${Role.Candidate}/${tokenResponse.userId}`)));
            navigate('/vacancy');
        } else if (tokenResponse.role === Role[Role.Recruiter]) {
            dispatch(setRecruiterProfile(await restClient.get(ApiServicesRoutes.profile + `/profile/${Role.Recruiter}/${tokenResponse.userId}`)));
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
