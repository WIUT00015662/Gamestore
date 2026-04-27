import { useEffect, useMemo, useState } from 'react';
import { api } from '../api/client';
import { hasPermission } from '../auth';
import type { CartItem, Order, PaymentMethod } from '../types';

export function CartPage() {
  const [items, setItems] = useState<CartItem[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [historyDetails, setHistoryDetails] = useState<CartItem[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState('');
  const [methods, setMethods] = useState<PaymentMethod[]>([]);
  const [selectedMethod, setSelectedMethod] = useState('Visa');
  const [visaCard, setVisaCard] = useState({ holder: 'Admin User', cardNumber: '4111111111111111', monthExpire: 12, yearExpire: 2030, cvv2: 123 });
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const canBuy = hasPermission('BuyGame');
  const canViewHistory = hasPermission('ViewOrderHistory');

  const load = async () => {
    try {
      const cartPromise = canBuy ? api.getCart() : Promise.resolve([]);
      const methodsPromise = canBuy ? api.getPaymentMethods() : Promise.resolve({ paymentMethods: [] as PaymentMethod[] });
      const historyPromise = canViewHistory ? api.getMyOrders() : Promise.resolve([]);

      const [cart, paymentMethods, history] = await Promise.all([cartPromise, methodsPromise, historyPromise]);
      setItems(cart);
      setMethods(paymentMethods.paymentMethods);
      setOrders(history);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load cart');
    }
  };

  useEffect(() => {
    void load();
  }, [canBuy, canViewHistory]);

  const loadHistoryDetails = async (orderId: string) => {
    try {
      setSelectedOrderId(orderId);
      const details = await api.getMyOrderDetails(orderId);
      setHistoryDetails(details);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load order details');
    }
  };

  const total = useMemo(
    () => items.reduce((sum, item) => sum + item.price * item.quantity * (100 - (item.discount ?? 0)) / 100, 0),
    [items],
  );

  const pay = async () => {
    try {
      await api.pay(selectedMethod, selectedMethod === 'Visa' ? visaCard : undefined);
      setMessage('Payment completed.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Payment failed');
    }
  };

  const canPay = canBuy && items.length > 0;

  return (
    <section className="section">
      <h2>Cart & Orders</h2>

      {canBuy ? (
        <>
          <h3>Current cart</h3>
          {items.length === 0 ? <p className="muted">No cart found or cart is empty.</p> : null}
          <ul className="list">
            {items.map((item) => (
              <li key={item.productId}>
                <span>{item.productId}</span>
                <span>x{item.quantity}</span>
                <span>${item.price.toFixed(2)}</span>
              </li>
            ))}
          </ul>

          {items.length > 0 ? (
            <>
              <p>
                <strong>Total:</strong> ${total.toFixed(2)}
              </p>

              <div className="toolbar">
                <select value={selectedMethod} onChange={(e) => setSelectedMethod(e.target.value)}>
                  {methods.map((method) => (
                    <option key={method.title} value={method.title}>
                      {method.title}
                    </option>
                  ))}
                </select>
                <button className="btn" type="button" onClick={() => void pay()} disabled={!canPay}>
                  Pay
                </button>
              </div>
            </>
          ) : null}
        </>
      ) : null}

      {canViewHistory ? (
        <>
          <h3>Order history</h3>
          {orders.length === 0 ? <p className="muted">No completed orders available.</p> : null}
          <ul className="list compact">
            {orders.map((order) => (
              <li key={order.id}>
                <span>{order.id}</span>
                <span>{order.status}</span>
                <span>{order.date ?? 'N/A'}</span>
                <button type="button" className="btn-small" onClick={() => void loadHistoryDetails(order.id)}>
                  Details
                </button>
              </li>
            ))}
          </ul>

          {selectedOrderId ? (
            <>
              <h4>Order details: {selectedOrderId}</h4>
              <ul className="list compact">
                {historyDetails.map((item) => (
                  <li key={item.productId}>
                    <span>{item.productId}</span>
                    <span>x{item.quantity}</span>
                    <span>${item.price.toFixed(2)}</span>
                  </li>
                ))}
              </ul>
            </>
          ) : null}
        </>
      ) : null}

      {error ? <p className="error">{error}</p> : null}
      {message ? <p>{message}</p> : null}
    </section>
  );
}
