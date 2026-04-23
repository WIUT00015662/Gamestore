import { useEffect, useState, type ChangeEvent } from 'react';
import { api } from '../api/client';
import type { CartItem, Order } from '../types';
import { OrderStatus } from '../types';

export function OrdersManagementPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState('');
  const [details, setDetails] = useState<CartItem[]>([]);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const [newGameKey, setNewGameKey] = useState('');

  const loadOrders = async () => {
    try {
      const data = await api.getOrders();
      setOrders(data);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load orders');
    }
  };

  const loadDetails = async (orderId: string) => {
    try {
      setSelectedOrderId(orderId);
      const data = await api.getOrderDetails(orderId);
      setDetails(data);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load order details');
    }
  };

  useEffect(() => {
    void loadOrders();
  }, []);

  const ship = async (id: string) => {
    try {
      await api.shipOrder(id);
      setMessage('Order shipped.');
      await loadOrders();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ship failed');
    }
  };

  const addGame = async () => {
    if (!selectedOrderId) {
      return;
    }

    try {
      await api.addGameToOrder(selectedOrderId, newGameKey);
      setNewGameKey('');
      setMessage('Game added to order.');
      await loadDetails(selectedOrderId);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Add game failed');
    }
  };

  const changeQuantity = async (detailId: string, count: number) => {
    try {
      await api.updateOrderDetailQuantity(detailId, count);
      await loadDetails(selectedOrderId);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update quantity failed');
    }
  };

  const removeDetail = async (detailId: string) => {
    try {
      await api.deleteOrderDetail(detailId);
      await loadDetails(selectedOrderId);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete detail failed');
    }
  };

  return (
    <section className="section">
      <h2>Orders management</h2>
      <p className="muted">Manage orders, details and shipping.</p>

      <div className="card-grid">
        <article className="card">
          <h3>Orders</h3>
          <ul className="list compact">
            {orders.map((order: Order) => (
              <li key={order.id}>
                <span>{order.id}</span>
                <span>{order.status}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => void loadDetails(order.id)}>Details</button>
                  {order.status === OrderStatus.Paid ? (
                    <button type="button" className="btn-small" onClick={() => void ship(order.id)}>Ship</button>
                  ) : null}
                </div>
              </li>
            ))}
          </ul>
        </article>

        <article className="card">
          <h3>Order details</h3>
          {!selectedOrderId ? <p className="muted">Select an order.</p> : <p className="muted">{selectedOrderId}</p>}
          <div className="form">
            <input
              value={newGameKey}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setNewGameKey(e.target.value)}
              placeholder="Game key"
            />
            <button type="button" className="btn" onClick={() => void addGame()}>
              Add game
            </button>
          </div>

          <ul className="list compact">
            {details.map((detail: CartItem) => (
              <li key={detail.productId}>
                <span>{detail.productId}</span>
                <div className="row-actions">
                  <button type="button" className="btn-small" onClick={() => void changeQuantity(detail.productId, detail.quantity + 1)}>+1</button>
                  <button type="button" className="btn-small" onClick={() => void changeQuantity(detail.productId, Math.max(1, detail.quantity - 1))}>-1</button>
                  <button type="button" className="btn-small danger" onClick={() => void removeDetail(detail.productId)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>
        </article>
      </div>

      {message ? <p>{message}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
