import React, { useState, useEffect } from 'react';
import { Modal, Form, Button } from 'react-bootstrap';

export default function PasswordModal({
  show,
  onHide,
  roomName,
  onSubmit,
  submitting
}: {
  show: boolean;
  onHide: () => void;
  roomName?: string;
  onSubmit: (password: string) => void;
  submitting: boolean;
}) {
  const [password, setPassword] = useState('');
  useEffect(() => { if (!show) setPassword(''); }, [show]);

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Zadejte heslo pro místnost {roomName ?? ''}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form onSubmit={(e) => { e.preventDefault(); onSubmit(password); }}>
          <Form.Group>
            <Form.Label>Heslo</Form.Label>
            <Form.Control
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Heslo místnosti"
              required
            />
          </Form.Group>
          <div className="mt-3 d-flex justify-content-end">
            <Button variant="secondary" onClick={onHide} disabled={submitting} className="me-2">Zrušit</Button>
            <Button type="submit" disabled={submitting}>{submitting ? 'Odesílám...' : 'Připojit'}</Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
}