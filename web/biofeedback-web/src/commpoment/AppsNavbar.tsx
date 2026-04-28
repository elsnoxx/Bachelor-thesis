import React, { useState, useContext } from "react";
import { Nav, Dropdown, Navbar, Container, Button } from "react-bootstrap";
import { Link, useLocation } from "react-router-dom";
import { Bell, PersonCircle } from "react-bootstrap-icons";
import LoginModal from "./LoginModal";
import RegisterModal from "./RegisterModal";
import PasswordModal from "./PasswordModal";
import { AuthContext } from "./AuthContext";

const navigation = [
  { name: 'Hry', href: '/hry' },
  { name: 'Statistiky', href: '/statistiky' }
];

export default function AppsNavbar() {
  const location = useLocation();
  const [showLogin, setShowLogin] = useState(false);
  const [showRegister, setShowRegister] = useState(false);
  const [showPasswordReset, setShowPasswordReset] = useState(false);
  const { user, logout } = useContext(AuthContext);

  return (
    <Navbar expand="lg" bg="light" variant="light" className="shadow-sm">
      <Container>
        <Navbar.Brand as={Link} to="/" className="me-4">Biofeedback App</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          

          <Nav className="me-auto">
            {user && navigation.map((item) => (
              <Nav.Link
                key={item.name}
                as={Link}
                to={item.href}
                active={location.pathname === item.href}
              >
                {item.name}
              </Nav.Link>
            ))}
          </Nav>


          <Nav className="align-items-center">
            {user ? (
              <>
                <Nav.Link href="#" className="me-3">
                  <Bell size={20} />
                </Nav.Link>
                <Dropdown align="end">
                  <Dropdown.Toggle as="div" className="nav-link" style={{cursor:"pointer"}}>
                    <PersonCircle size={20} />
                  </Dropdown.Toggle>
                  <Dropdown.Menu>
                    <Dropdown.ItemText className="fw-bold">{user.name || user.email}</Dropdown.ItemText>
                    <Dropdown.Divider />
                    <Dropdown.Item onClick={() => setShowPasswordReset(true)}>
                      Změnit heslo
                    </Dropdown.Item>
                    <Dropdown.Item onClick={logout} className="text-danger">
                      Odhlásit
                    </Dropdown.Item>
                  </Dropdown.Menu>
                </Dropdown>
              </>
            ) : (
              <>
                <Button 
                  variant="link" 
                  className="nav-link me-2 text-decoration-none" 
                  onClick={() => setShowLogin(true)}
                >
                  Přihlásit
                </Button>
                <Button 
                  variant="primary" 
                  size="sm" 
                  onClick={() => setShowRegister(true)}
                >
                  Registrovat
                </Button>
              </>
            )}
          </Nav>

        </Navbar.Collapse>
      </Container>

      {/* Modály */}
      <LoginModal show={showLogin} onHide={() => setShowLogin(false)} />
      <RegisterModal 
        show={showRegister} 
        onHide={() => setShowRegister(false)} 
        onRegistered={() => setShowLogin(true)} 
      />
      <PasswordModal 
        show={showPasswordReset} 
        onHide={() => setShowPasswordReset(false)} 
      />
    </Navbar>
  );
}