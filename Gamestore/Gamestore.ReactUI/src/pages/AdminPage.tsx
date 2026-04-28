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

  const emptyUserForm = { id: '', name: '', email: '', password: '', roles: [] as string[] };
  const emptyRoleForm = { id: '', name: '', permissions: [] as string[] };

  const [userForm, setUserForm] = useState(emptyUserForm);
  const [roleForm, setRoleForm] = useState(emptyRoleForm);

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

  const saveUser = async () => {
    try {
      if (!userForm.email.trim()) {
        setError('Email is required.');
        return;
      }

      if (userForm.id) {
        await api.updateUser({
          user: { id: userForm.id, name: userForm.name, email: userForm.email },
          roles: userForm.roles,
          password: userForm.password || 'temporary-password',
        });
        setMessage('User updated.');
      } else {
        await api.createUser({
          user: { id: crypto.randomUUID(), name: userForm.name, email: userForm.email },
          roles: userForm.roles,
          password: userForm.password,
        });
        setMessage('User created.');
      }

      setUserForm(emptyUserForm);
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save user failed');
    }
  };

  const selectUser = async (id: string) => {
    const user = users.find((x: BasicUser) => x.id === id);
    if (!user) {
      return;
    }

    setUserForm((old) => ({
      ...old,
      id: user.id,
      name: user.name,
      email: user.email ?? '',
      password: '',
    }));

    try {
      const roleItems = await api.getUserRoles(id);
      setUserForm((old) => ({ ...old, roles: roleItems.map((x) => x.id) }));
    } catch {
      setUserForm((old) => ({ ...old, roles: [] }));
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

  const saveRole = async () => {
    try {
      if (roleForm.id) {
        await api.updateRole({
          role: { id: roleForm.id, name: roleForm.name },
          permissions: roleForm.permissions,
        });
        setMessage('Role updated.');
      } else {
        await api.createRole({
          role: { id: crypto.randomUUID(), name: roleForm.name },
          permissions: roleForm.permissions,
        });
        setMessage('Role created.');
      }

      setRoleForm(emptyRoleForm);
      setError('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save role failed');
    }
  };

  const selectRole = async (id: string) => {
    const role = roles.find((x: BasicRole) => x.id === id);
    if (!role) {
      return;
    }

    setRoleForm((old) => ({ ...old, id: role.id, name: role.name }));

    try {
      const rolePermissions = await api.getRolePermissions(id);
      setRoleForm((old) => ({ ...old, permissions: rolePermissions }));
    } catch {
      setRoleForm((old) => ({ ...old, permissions: [] }));
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
                <span>{user.name}{user.email ? ` (${user.email})` : ''}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => void selectUser(user.id)}>Edit</button>
                  <button type="button" className="btn-small danger" onClick={() => void deleteUser(user.id)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>

          <div className="form">
            <label>
              User name
              <input value={userForm.name} onChange={(e: ChangeEvent<HTMLInputElement>) => setUserForm((old) => ({ ...old, name: e.target.value }))} />
            </label>
            <label>
              Email
              <input value={userForm.email} onChange={(e: ChangeEvent<HTMLInputElement>) => setUserForm((old) => ({ ...old, email: e.target.value }))} type="email" />
            </label>
            <label>
              Password
              <input value={userForm.password} onChange={(e: ChangeEvent<HTMLInputElement>) => setUserForm((old) => ({ ...old, password: e.target.value }))} type="password" />
            </label>
            <div className="check-grid">
              {roles.map((role: BasicRole) => (
                <label key={role.id}>
                  <input
                    type="checkbox"
                    checked={userForm.roles.includes(role.id)}
                    onChange={() => setUserForm((old) => ({ ...old, roles: toggle(old.roles, role.id) }))}
                  />
                  {role.name}
                </label>
              ))}
            </div>
            <div className="row-actions">
              <button type="button" className="btn" onClick={() => void saveUser()}>{userForm.id ? 'Update user' : 'Create user'}</button>
              {userForm.id ? (
                <button type="button" className="btn-small" onClick={() => setUserForm(emptyUserForm)}>Clear</button>
              ) : null}
            </div>
          </div>
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
              Role name
              <input value={roleForm.name} onChange={(e) => setRoleForm((old) => ({ ...old, name: e.target.value }))} />
            </label>
            <div className="check-grid">
              {permissions.map((permission) => (
                <label key={permission}>
                  <input
                    type="checkbox"
                    checked={roleForm.permissions.includes(permission)}
                    onChange={() => setRoleForm((old) => ({ ...old, permissions: toggle(old.permissions, permission) }))}
                  />
                  {permission}
                </label>
              ))}
            </div>
            <div className="row-actions">
              <button type="button" className="btn" onClick={() => void saveRole()}>{roleForm.id ? 'Update role' : 'Create role'}</button>
              {roleForm.id ? (
                <button type="button" className="btn-small" onClick={() => setRoleForm(emptyRoleForm)}>Clear</button>
              ) : null}
            </div>
          </div>
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
