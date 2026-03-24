import React, { useState } from "react";
import { Modal, Form, Button } from "react-bootstrap";

export default function RegisterModal({ show, onHide, onRegistered }:
  { show: boolean; onHide: () => void; onRegistered?: () => void }) {

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== confirmPassword) { alert("Hesla se neshodují"); return; }
    setLoading(true);
    try {
      const apiUrl = import.meta.env.VITE_API_URL;
      const res = await fetch(`${apiUrl}/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          Username: username,
          Password: password,
          ConfirmPassword: confirmPassword,
          Email: email
        }),
      });
      const text = await res.text();
      if (!res.ok) throw new Error(text || "Register failed");
      alert("Účet vytvořen. Přihlaste se.");
      onHide();
      onRegistered?.();
      setUsername(""); setEmail(""); setPassword(""); setConfirmPassword("");
    } catch (err: any) {
      console.error(err);
      alert("Registrace selhala: " + (err.message || err));
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
            <Form.Label>Uživatelské jméno</Form.Label>
            <Form.Control value={username} onChange={e => setUsername(e.target.value)} required />
          </Form.Group>

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
            <Form.Control type="password" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} required />
          </Form.Group>

          <div className="d-grid">
            <Button variant="success" type="submit" disabled={loading}>{loading ? "Prosím čekejte…" : "Vytvořit účet"}</Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
}