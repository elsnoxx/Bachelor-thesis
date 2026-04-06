import React, { useState } from "react";
import { Modal, Form, Button, InputGroup } from "react-bootstrap";
import api from "../api/axiosInstance";

export default function PasswordModal({ show, onHide }: { show: boolean; onHide: () => void }) {
  const [email, setEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmNewPassword) {
      alert("Hesla se neshodují");
      return;
    }

    setLoading(true);
    try {
      const res = await api.post("/reset-password", {
        email,
        newPassword,
        confirmNewPassword
      });
      // pokud backend vrací text nebo JSON, případně kontroluj res.status
      alert("Heslo úspěšně změněno (nebo požadavek přijat).");
      onHide();
    } catch (err: any) {
      console.error(err);
      const msg = err?.response?.data || err?.message || String(err);
      alert("Chyba při změně hesla: " + msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Obnova / změna hesla</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <InputGroup>
              <InputGroup.Text>@</InputGroup.Text>
              <Form.Control type="email" value={email} onChange={e => setEmail(e.target.value)} required />
            </InputGroup>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Nové heslo</Form.Label>
            <Form.Control type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} required />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Nové heslo znovu</Form.Label>
            <Form.Control type="password" value={confirmNewPassword} onChange={e => setConfirmNewPassword(e.target.value)} required />
          </Form.Group>

          <div className="d-grid">
            <Button type="submit" disabled={loading}>
              {loading ? "Odesílám…" : "Změnit heslo"}
            </Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
}