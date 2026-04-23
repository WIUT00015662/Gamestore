import { useEffect, useState, type ChangeEvent } from 'react';
import { api } from '../api/client';
import type { BasicRole, BasicUser } from '../types';

function toggle(values: string[], value: string) {
  return values.includes(value)
    ? values.filter((x) => x !== value)
    : [...values, value];
}

export function AdminPage() {
  const [users, setUsers] = useState<BasicUser[]>([]);
  const [roles, setRoles] = useState<BasicRole[]>([]);
  const [permissions, setPermissions] = useState<string[]>([]);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const [newUserName, setNewUserName] = useState('');
  const [newUserPassword, setNewUserPassword] = useState('');
  const [newUserRoles, setNewUserRoles] = useState<string[]>([]);

  const [editUserId, setEditUserId] = useState('');
  const [editUserName, setEditUserName] = useState('');
  const [editUserPassword, setEditUserPassword] = useState('');
  const [editUserRoles, setEditUserRoles] = useState<string[]>([]);

  const [newRoleName, setNewRoleName] = useState('');
  const [newRolePermissions, setNewRolePermissions] = useState<string[]>([]);

  const [editRoleId, setEditRoleId] = useState('');
  const [editRoleName, setEditRoleName] = useState('');
  const [editRolePermissions, setEditRolePermissions] = useState<string[]>([]);

  const load = async () => {
    try {
      const [usersResponse, rolesResponse, permissionsResponse] = await Promise.all([
        api.getUsers(),
        api.getRoles(),
        api.getPermissions(),
      ]);

      setUsers(usersResponse);
      setRoles(rolesResponse);
      setPermissions(permissionsResponse);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load admin data');
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const createUser = async () => {
    try {
      await api.createUser({
        user: { id: crypto.randomUUID(), name: newUserName },
        roles: newUserRoles,
        password: newUserPassword,
      });

      setNewUserName('');
      setNewUserPassword('');
      setNewUserRoles([]);
      setMessage('User created.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create user failed');
    }
  };

  const selectUser = async (id: string) => {
    const user = users.find((x: BasicUser) => x.id === id);
    if (!user) {
      return;
    }

    setEditUserId(user.id);
    setEditUserName(user.name);

    try {
      const roleItems = await api.getUserRoles(id);
      setEditUserRoles(roleItems.map((x) => x.id));
    } catch {
      setEditUserRoles([]);
    }
  };

  const updateUser = async () => {
    try {
      await api.updateUser({
        user: { id: editUserId, name: editUserName },
        roles: editUserRoles,
        password: editUserPassword || 'temporary-password',
      });

      setMessage('User updated.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update user failed');
    }
  };

  const deleteUser = async (id: string) => {
    try {
      await api.deleteUser(id);
      setMessage('User deleted.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete user failed');
    }
  };

  const createRole = async () => {
    try {
      await api.createRole({
        role: { id: crypto.randomUUID(), name: newRoleName },
        permissions: newRolePermissions,
      });

      setNewRoleName('');
      setNewRolePermissions([]);
      setMessage('Role created.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create role failed');
    }
  };

  const selectRole = async (id: string) => {
    const role = roles.find((x: BasicRole) => x.id === id);
    if (!role) {
      return;
    }

    setEditRoleId(role.id);
    setEditRoleName(role.name);

    try {
      const rolePermissions = await api.getRolePermissions(id);
      setEditRolePermissions(rolePermissions);
    } catch {
      setEditRolePermissions([]);
    }
  };

  const updateRole = async () => {
    try {
      await api.updateRole({
        role: { id: editRoleId, name: editRoleName },
        permissions: editRolePermissions,
      });

      setMessage('Role updated.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update role failed');
    }
  };

  const deleteRole = async (id: string) => {
    try {
      await api.deleteRole(id);
      setMessage('Role deleted.');
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete role failed');
    }
  };

  return (
    <section className="section">
      <h2>Administrator panel</h2>
      <p className="muted">Manage users, roles and permissions.</p>

      <div className="card-grid">
        <article className="card">
          <h3>Users</h3>
          <ul className="list compact">
            {users.map((user: BasicUser) => (
              <li key={user.id}>
                <span>{user.name}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => void selectUser(user.id)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void deleteUser(user.id)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>

          <div className="form">
            <label>
              New user name
              <input value={newUserName} onChange={(e: ChangeEvent<HTMLInputElement>) => setNewUserName(e.target.value)} />
            </label>
            <label>
              Password
              <input value={newUserPassword} onChange={(e: ChangeEvent<HTMLInputElement>) => setNewUserPassword(e.target.value)} type="password" />
            </label>
            <div className="check-grid">
              {roles.map((role: BasicRole) => (
                <label key={role.id}>
                  <input
                    type="checkbox"
                    checked={newUserRoles.includes(role.id)}
                    onChange={() => setNewUserRoles((old: string[]) => toggle(old, role.id))}
                  />
                  {role.name}
                </label>
              ))}
            </div>
            <button type="button" className="btn" onClick={() => void createUser()}>Create user</button>
          </div>

          {editUserId ? (
            <div className="form">
              <h4>Edit user</h4>
              <label>
                Name
                <input value={editUserName} onChange={(e: ChangeEvent<HTMLInputElement>) => setEditUserName(e.target.value)} />
              </label>
              <label>
                New password (optional)
                <input value={editUserPassword} onChange={(e: ChangeEvent<HTMLInputElement>) => setEditUserPassword(e.target.value)} type="password" />
              </label>
              <div className="check-grid">
                {roles.map((role: BasicRole) => (
                  <label key={role.id}>
                    <input
                      type="checkbox"
                      checked={editUserRoles.includes(role.id)}
                      onChange={() => setEditUserRoles((old: string[]) => toggle(old, role.id))}
                    />
                    {role.name}
                  </label>
                ))}
              </div>
              <button type="button" className="btn" onClick={() => void updateUser()}>Update user</button>
            </div>
          ) : null}
        </article>

        <article className="card">
          <h3>Roles</h3>
          <ul className="list compact">
            {roles.map((role) => (
              <li key={role.id}>
                <span>{role.name}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => void selectRole(role.id)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void deleteRole(role.id)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>

          <div className="form">
            <label>
              New role name
              <input value={newRoleName} onChange={(e) => setNewRoleName(e.target.value)} />
            </label>
            <div className="check-grid">
              {permissions.map((permission) => (
                <label key={permission}>
                  <input
                    type="checkbox"
                    checked={newRolePermissions.includes(permission)}
                    onChange={() => setNewRolePermissions((old: string[]) => toggle(old, permission))}
                  />
                  {permission}
                </label>
              ))}
            </div>
            <button type="button" className="btn" onClick={() => void createRole()}>Create role</button>
          </div>

          {editRoleId ? (
            <div className="form">
              <h4>Edit role</h4>
              <label>
                Name
                <input value={editRoleName} onChange={(e) => setEditRoleName(e.target.value)} />
              </label>
              <div className="check-grid">
                {permissions.map((permission) => (
                  <label key={permission}>
                    <input
                      type="checkbox"
                      checked={editRolePermissions.includes(permission)}
                      onChange={() => setEditRolePermissions((old: string[]) => toggle(old, permission))}
                    />
                    {permission}
                  </label>
                ))}
              </div>
              <button type="button" className="btn" onClick={() => void updateRole()}>Update role</button>
            </div>
          ) : null}
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
