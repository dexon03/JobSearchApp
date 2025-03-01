import { HeaderComponent } from "./components/header.component.tsx";
import { Outlet, useNavigate } from "react-router-dom";
import { Container } from "@mui/material";
import ErrorBoundary from "./components/error.boundary.tsx";
import { pdfjs } from 'react-pdf';
import { useEffect } from 'react';
import { setNavigate } from "./hooks/navigate.ts";

pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.js`;

function App() {
  const navigate = useNavigate();

  useEffect(() => {
    setNavigate(navigate);
  }, [navigate]);

  return (
    <ErrorBoundary>
      <HeaderComponent />
      <Container className="mt-5 pb-5">
        <Outlet />
      </Container>
    </ErrorBoundary>
  )
}

export default App
