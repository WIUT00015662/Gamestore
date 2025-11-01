import { Link, Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { clearToken, getPrimaryRole, getUserName, hasPermission, hasToken } from './auth';
import { AdminPage } from './pages/AdminPage';
import { CartPage } from './pages/CartPage';
import { EntitiesPage } from './pages/EntitiesPage';
import { ForbiddenPage } from './pages/ForbiddenPage';
import { GameDetailsPage } from './pages/GameDetailsPage';
import { GamesManagementPage } from './pages/GamesManagementPage';
import { GamesPage } from './pages/GamesPage';
import { GuestPage } from './pages/GuestPage';
import { HomePage } from './pages/HomePage';
import { LoginPage } from './pages/LoginPage';
import { ManagerPage } from './pages/ManagerPage';
import { ModeratorPage } from './pages/ModeratorPage';
import { OrdersManagementPage } from './pages/OrdersManagementPage';
import { UserPage } from './pages/UserPage';

type AppProps = {
  refreshAuth: () => void;
};

export function App({ refreshAuth }: AppProps) {
  const navigate = useNavigate();
  const isAuthenticated = hasToken();
  const canBuy = hasPermission('BuyGame');
  const canManageEntities = hasPermission('ManageEntities');
  const canManageUsers = hasPermission('ManageUsers');
  const canManageRoles = hasPermission('ManageRoles');
  const canManageComments = hasPermission('ManageComments');
  const userName = isAuthenticated ? getUserName() : 'Guest';
  const role = isAuthenticated ? getPrimaryRole() : 'Guest';

  const logout = () => {
    clearToken();
    refreshAuth();
    navigate('/');
  };

  return (
    <div className="layout">
      <header className="topbar">
        <h1>Gamestore Aggregator</h1>
        <div className="identity-pill">{userName} · {role}</div>
        <nav>
          <Link to="/">Home</Link>
          <Link to="/games">Games</Link>
          {!isAuthenticated ? <Link to="/guest">Guest</Link> : null}
          {canBuy ? <Link to="/user">User</Link> : null}
          {canManageComments ? <Link to="/moderator">Moderator</Link> : null}
          {canManageEntities ? <Link to="/manager">Manager</Link> : null}
          {canManageUsers && canManageRoles ? <Link to="/admin">Admin</Link> : null}
          {canBuy ? <Link to="/cart">Cart</Link> : null}
          {!isAuthenticated ? <Link to="/login">Login</Link> : <button type="button" onClick={logout}>Logout</button>}
        </nav>
      </header>

      <main className="content">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/games" element={<GamesPage />} />
          <Route path="/games/:key" element={<GameDetailsPage />} />
          <Route path="/guest" element={<GuestPage />} />
          <Route path="/user" element={isAuthenticated && canBuy ? <UserPage /> : <ForbiddenPage />} />
          <Route path="/moderator" element={isAuthenticated && canManageComments ? <ModeratorPage /> : <ForbiddenPage />} />
          <Route path="/manager" element={isAuthenticated && canManageEntities ? <ManagerPage /> : <ForbiddenPage />} />
          <Route path="/manager/games" element={isAuthenticated && canManageEntities ? <GamesManagementPage /> : <ForbiddenPage />} />
          <Route path="/manager/entities" element={isAuthenticated && canManageEntities ? <EntitiesPage /> : <ForbiddenPage />} />
          <Route path="/manager/orders" element={isAuthenticated && canManageEntities ? <OrdersManagementPage /> : <ForbiddenPage />} />
          <Route path="/admin" element={isAuthenticated && canManageUsers && canManageRoles ? <AdminPage /> : <ForbiddenPage />} />
          <Route path="/cart" element={isAuthenticated && canBuy ? <CartPage /> : <Navigate to="/" replace />} />
          <Route path="/login" element={!isAuthenticated ? <LoginPage onLogin={refreshAuth} /> : <Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}
