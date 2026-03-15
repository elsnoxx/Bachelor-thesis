import React, { useState, useContext } from "react";
import { Modal, Form, Button, InputGroup } from "react-bootstrap";
import { AuthContext } from "./AuthContext";

export default function LoginModal({ show, onHide }: { show: boolean; onHide: () => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const auth = useContext(AuthContext);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });
      if (!res.ok) throw new Error("Login failed");
      const data = await res.json(); // { token, user }
      auth.login(data.token, data.user);
      onHide();
    } catch (err) {
      console.error(err);
      alert("Přihlášení selhalo");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Přihlášení</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form onSubmit={handleLogin}>
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <InputGroup>
              <InputGroup.Text>@</InputGroup.Text>
              <Form.Control type="email" value={email} onChange={e => setEmail(e.target.value)} required />
            </InputGroup>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Heslo</Form.Label>
            <Form.Control type="password" value={password} onChange={e => setPassword(e.target.value)} required />
          </Form.Group>

          <div className="d-grid">
            <Button type="submit" disabled={loading}>{loading ? "Prosím čekejte…" : "Přihlásit"}</Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
}