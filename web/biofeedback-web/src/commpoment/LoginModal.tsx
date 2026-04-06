import React, { useState, useContext } from "react";
import { Modal, Form, Button, InputGroup } from "react-bootstrap";
import { AuthContext } from "./AuthContext";
import api from "../api/axiosInstance";
import PasswordModal from "./PasswordModal"; // Importuješ, ale nepoužíváš - opraveno níže

export default function LoginModal({ show, onHide }: { show: boolean; onHide: () => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const auth = useContext(AuthContext);
  const [showPasswordModal, setShowPasswordModal] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      // Axios volání
      const res = await api.post("/login", { email, password });
      const data = res.data; 
      
      // Sjednocení case-sensitivity z C# Backendu
      const token = data?.Token || data?.token;
      
      if (!token) throw new Error("Server nevrátil přístupový token.");
      
      // Volání login metody z kontextu
      if (auth) {
        auth.login(token, { email }); 
        onHide();
      }
    } catch (err: any) {
      console.error(err);
      alert(err.response?.data || "Přihlášení selhalo. Zkontrolujte údaje.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
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
                <Form.Control 
                  type="email" 
                  value={email} 
                  onChange={e => setEmail(e.target.value)} 
                  placeholder="name@example.com"
                  required 
                />
              </InputGroup>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Heslo</Form.Label>
              <InputGroup>
                <InputGroup.Text>🔒</InputGroup.Text>
                <Form.Control 
                  type="password" 
                  value={password} 
                  onChange={e => setPassword(e.target.value)} 
                  placeholder="Vaše heslo"
                  required 
                />
              </InputGroup>
              <div className="text-end mt-1">
                <Button 
                  variant="link" 
                  size="sm" 
                  className="p-0 text-decoration-none"
                  onClick={() => {
                    onHide(); // Zavře login modál
                    setShowPasswordModal(true); // Otevře reset modál
                  }}
                >
                  Zapomenuté heslo?
                </Button>
              </div>
            </Form.Group>

            <div className="d-grid gap-2">
              <Button variant="primary" type="submit" disabled={loading}>
                {loading ? "Prosím čekejte…" : "Přihlásit se"}
              </Button>
            </div>
          </Form>
        </Modal.Body>
      </Modal>

      {/* Modál pro reset hesla */}
      <PasswordModal 
        show={showPasswordModal} 
        onHide={() => setShowPasswordModal(false)} 
      />
    </>
  );
}