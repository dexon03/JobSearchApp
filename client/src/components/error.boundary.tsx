import React, { ErrorInfo } from 'react';
import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';

class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            hasError: false,
            errorMessage: ''
        };
    }

    static getDerivedStateFromError(error) {
        return { hasError: true };
    }

    componentDidCatch(error: Error, errorInfo: ErrorInfo) {
        console.log(error, errorInfo);
        this.setState({ errorMessage: error.message });
    }

    handleReset = () => {
        this.setState({ hasError: false });
    };

    render() {
        if (this.state.hasError) {
            return (
                <Dialog open={this.state.hasError} onClose={this.handleReset}>
                    <DialogTitle>Error</DialogTitle>
                    <DialogContent>
                        {this.state.errorMessage}
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={this.handleReset} color="primary">
                            Close
                        </Button>
                    </DialogActions>
                </Dialog>
            );
        }

        return this.props.children;
    }
}

export default ErrorBoundary;