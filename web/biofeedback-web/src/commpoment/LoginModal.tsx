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
      const apiUrl = import.meta.env.VITE_API_URL;
      const res = await fetch(`${apiUrl}/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });
      if (!res.ok) throw new Error("Login failed");
      const data = await res.json(); // { Token: "...", RefreshToken: "..." }
      const token = data?.Token || data?.token;
      if (!token) throw new Error("No token returned");
      localStorage.setItem("token", token);
      auth.login(token, { email }); // <--- nastavit user, aby navbar poznal přihlášení
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