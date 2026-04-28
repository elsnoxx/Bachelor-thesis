import React, { useState, useContext, useEffect } from "react";
import { Modal, Form, Button, InputGroup, Alert } from "react-bootstrap";
import { AuthContext } from "./AuthContext";
import api from "../api/axiosInstance";
import PasswordModal from "./PasswordModal";

export default function LoginModal({ show, onHide }: { show: boolean; onHide: () => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const auth = useContext(AuthContext);
  const [showPasswordModal, setShowPasswordModal] = useState(false);


  useEffect(() => {
    if (!show) {
      setErrorMessage(null);
      setPassword("");
    }
  }, [show]);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setErrorMessage(null);

    try {
      const res = await api.post("/login", { email, password });
      const data = res.data; 
      
      const token = data?.Token || data?.token;
      
      if (!token) throw new Error("Server nevrátil přístupový token.");
      
      if (auth) {
        auth.login(token, { email }); 
        onHide();
      }
    } catch (err: any) {
      console.error(err);
      const msg = err.response?.data || "Přihlášení selhalo. Zkontrolujte údaje.";
      setErrorMessage(typeof msg === 'string' ? msg : "Chybné přihlašovací údaje");
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
          {errorMessage && (
            <Alert variant="danger" className="py-2 small">
              {errorMessage}
            </Alert>
          )}

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
                    onHide();
                    setShowPasswordModal(true);
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

      <PasswordModal 
        show={showPasswordModal} 
        onHide={() => setShowPasswordModal(false)} 
      />
    </>
  );
}