import React from "react";
import {
    AppBar,
    Avatar,
    Box,
    Button,
    Container, createTheme,
    IconButton,
    Menu,
    MenuItem, ThemeProvider,
    Toolbar,
    Tooltip,
    Typography
} from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import AdbIcon from '@mui/icons-material/Adb';
import 'bootstrap/dist/css/bootstrap.css';
import { NavLink } from "react-router-dom";
import { Role } from "../models/common/role.enum.ts";
import { useAppDispatch, useAppSelector } from "../hooks/redux.hooks.ts";
import { RoleRoute } from "../models/role_routes/role.routes.model.ts";
import useRole from "../hooks/useRole.ts";

declare module '@mui/material/styles' {
    interface Palette {
        myTheme: Palette['primary'];
    }
    interface PaletteOptions {
        myTheme?: PaletteOptions['primary'];
    }
}

declare module '@mui/material' {
    interface AppBarPropsColorOverrides {
        myTheme: true;
    }
}

export function HeaderComponent() {
    const dispatch = useAppDispatch();
    const { role } = useRole();

    const pages = role !== null ? RoleRoute[Role[role! as keyof typeof Role]] : [];
    const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null);
    const [anchorElUser, setAnchorElUser] = React.useState<null | HTMLElement>(null);

    const profile = useAppSelector((state) => state.profile.candidateProfile || state.profile.recruiterProfile);

    let theme = createTheme({});
    theme = createTheme(theme, {
        palette: {
            myTheme: theme.palette.augmentColor({
                color: {
                    main: '#1a1a1a',
                    contrastText: '#ced4da',
                },
                name: 'myTheme',
            }),
        }
    })

    const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorElNav(event.currentTarget);
    };
    const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorElUser(event.currentTarget);
    };

    const handleCloseNavMenu = () => {
        setAnchorElNav(null);
    };

    const handleCloseUserMenu = () => {
        setAnchorElUser(null);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        dispatch({ type: 'SIGNOUT_REQUEST' });

        setAnchorElUser(null);
    }

    return (
        <ThemeProvider theme={theme}>
            <AppBar position="static" color="myTheme">
                <Container maxWidth="lg">
                    <Toolbar disableGutters>
                        <Typography
                            variant="h6"
                            noWrap
                            sx={{
                                mr: 2,
                                display: { xs: 'none', md: 'flex' },
                                fontFamily: 'Roboto,sans-serif',
                                fontWeight: 800,
                                letterSpacing: '.3rem',
                                color: 'inherit',
                                textDecoration: 'none',
                            }}
                        >
                            JobSearchApp
                        </Typography>

                        <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
                            <IconButton
                                size="large"
                                aria-label="account of current user"
                                aria-controls="menu-appbar"
                                aria-haspopup="true"
                                onClick={handleOpenNavMenu}
                                color="inherit"
                            >
                                <MenuIcon />
                            </IconButton>
                            <Menu
                                id="menu-appbar"
                                anchorEl={anchorElNav}
                                anchorOrigin={{
                                    vertical: 'bottom',
                                    horizontal: 'left',
                                }}
                                keepMounted
                                transformOrigin={{
                                    vertical: 'top',
                                    horizontal: 'left',
                                }}
                                open={Boolean(anchorElNav)}
                                onClose={handleCloseNavMenu}
                                sx={{
                                    display: { xs: 'block', md: 'none' },
                                }}
                            >
                                {pages.map((page) => (
                                    <MenuItem key={page.name} onClick={handleCloseNavMenu}>
                                        <Typography textAlign="center">{page.name}</Typography>
                                    </MenuItem>
                                ))}
                            </Menu>
                        </Box>
                        <AdbIcon sx={{ display: { xs: 'flex', md: 'none' }, mr: 1 }} />
                        <Typography
                            variant="h5"
                            noWrap
                            component="a"
                            sx={{
                                mr: 2,
                                display: { xs: 'flex', md: 'none' },
                                flexGrow: 1,
                                fontFamily: 'monospace',
                                fontWeight: 700,
                                letterSpacing: '.3rem',
                                color: 'inherit',
                                textDecoration: 'none',
                            }}
                        >
                            JobSearchApp
                        </Typography>
                        <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
                            {pages.map((page) => (
                                <NavLink key={page.name} to={page.route} className="nav-link">
                                    <Typography
                                        variant="h6"
                                        noWrap
                                        sx={{
                                            mr: 2,
                                            display: { xs: 'none', md: 'flex' },
                                            flexGrow: 1,
                                            fontFamily: 'Open sans',
                                            color: 'inherit',
                                            textDecoration: 'none',
                                        }}
                                    >
                                        {page.name}
                                    </Typography>
                                </NavLink>
                            ))}
                        </Box>


                        <Box sx={{ flexGrow: 0 }}>
                            <Button variant="contained" color="success" className="m-3 rounded-5">online</Button>
                            <Tooltip title="Open settings">
                                <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                                    <Avatar alt="Remy Sharp" />
                                </IconButton>
                            </Tooltip>
                            <Typography
                                variant="h5"
                                noWrap
                                className="d-inline m-3 fs-4"
                                sx={{
                                    fontFamily: 'Open sans',
                                }}
                            >

                                {profile ? profile.name + ' ' + profile.surname : 'User'}
                            </Typography>
                            <Menu
                                sx={{ mt: '45px' }}
                                id="menu-appbar"
                                anchorEl={anchorElUser}
                                anchorOrigin={{
                                    vertical: 'top',
                                    horizontal: 'right',
                                }}
                                keepMounted
                                transformOrigin={{
                                    vertical: 'top',
                                    horizontal: 'right',
                                }}
                                open={Boolean(anchorElUser)}
                                onClose={handleCloseUserMenu}
                            >
                                {
                                    role != Role[Role.Admin] ?
                                        <NavLink to={'/profile'} className={'nav-link'}>
                                            <MenuItem onClick={handleCloseUserMenu}>
                                                <Typography textAlign="center">Profile</Typography>
                                            </MenuItem>
                                        </NavLink> : null
                                }

                                <NavLink to={'/login'} className={'nav-link'}>
                                    <MenuItem onClick={handleLogout}>
                                        <Typography textAlign="center">Logout</Typography>
                                    </MenuItem>
                                </NavLink>
                            </Menu>
                        </Box>
                    </Toolbar>
                </Container>
            </AppBar>
        </ThemeProvider>
    );
}