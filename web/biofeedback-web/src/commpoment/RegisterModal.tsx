import React, { useState } from "react";
import { Modal, Form, Button } from "react-bootstrap";

export default function RegisterModal({ show, onHide, onRegistered }:
  { show: boolean; onHide: () => void; onRegistered?: () => void }) {

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [password2, setPassword2] = useState("");
  const [loading, setLoading] = useState(false);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== password2) { alert("Hesla se neshodují"); return; }
    setLoading(true);
    try {
      const res = await fetch("/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });
      if (!res.ok) throw new Error("Register failed");
      alert("Účet vytvořen. Přihlaste se.");
      onHide();
      onRegistered?.();
      setEmail(""); setPassword(""); setPassword2("");
    } catch (err) {
      console.error(err);
      alert("Registrace selhala");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton><Modal.Title>Registrace</Modal.Title></Modal.Header>
      <Modal.Body>
        <Form onSubmit={handleRegister}>
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <Form.Control type="email" value={email} onChange={e => setEmail(e.target.value)} required />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Heslo</Form.Label>
            <Form.Control type="password" value={password} onChange={e => setPassword(e.target.value)} required />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Heslo znovu</Form.Label>
            <Form.Control type="password" value={password2} onChange={e => setPassword2(e.target.value)} required />
          </Form.Group>

          <div className="d-grid">
            <Button variant="success" type="submit" disabled={loading}>{loading ? "Prosím čekejte…" : "Vytvořit účet"}</Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
}