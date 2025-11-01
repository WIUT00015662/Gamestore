import { useEffect, useMemo, useState } from 'react';
import { api } from '../api/client';
import type { CartItem, PaymentMethod } from '../types';

export function CartPage() {
  const [items, setItems] = useState<CartItem[]>([]);
  const [methods, setMethods] = useState<PaymentMethod[]>([]);
  const [selectedMethod, setSelectedMethod] = useState('Visa');
  const [visaCard, setVisaCard] = useState({ holder: 'Admin User', cardNumber: '4111111111111111', monthExpire: 12, yearExpire: 2030, cvv2: 123 });
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const load = async () => {
    try {
      const [cart, paymentMethods] = await Promise.all([api.getCart(), api.getPaymentMethods()]);
      setItems(cart);
      setMethods(paymentMethods.paymentMethods);
      setError('');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load cart');
    }
  };

  useEffect(() => {
    void load();
  }, []);

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

  return (
    <section className="section">
      <h2>Cart</h2>
      <ul className="list">
        {items.map((item) => (
          <li key={item.productId}>
            <span>{item.productId}</span>
            <span>x{item.quantity}</span>
            <span>${item.price.toFixed(2)}</span>
          </li>
        ))}
      </ul>
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
        <button className="btn" type="button" onClick={() => void pay()}>
          Pay
        </button>
      </div>

      {error ? <p className="error">{error}</p> : null}
      {message ? <p>{message}</p> : null}
    </section>
  );
}
