import React, { useState } from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { App } from './App';
import './styles.css';

function Root() {
  const [, setTick] = useState(0);
  const refreshAuth = () => setTick((x) => x + 1);

  return (
    <BrowserRouter>
      <App refreshAuth={refreshAuth} />
    </BrowserRouter>
  );
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Root />
  </React.StrictMode>,
);
