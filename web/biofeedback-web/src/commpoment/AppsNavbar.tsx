import React, { useState, useContext } from "react";
import { Nav, Dropdown, Navbar, Container, Button } from "react-bootstrap";
import { Link, useLocation } from "react-router-dom";
import { Bell, PersonCircle } from "react-bootstrap-icons";
import LoginModal from "./LoginModal";
import RegisterModal from "./RegisterModal";
import { AuthContext } from "./AuthContext";

const navigation = [
  { name: 'Hry', href: '/hry' },
  { name: 'Statistiky', href: '/statistiky' }
];

export default function AppsNavbar() {
  const location = useLocation();
  const [showLogin, setShowLogin] = useState(false);
  const [showRegister, setShowRegister] = useState(false);
  const { user, logout } = useContext(AuthContext);

  return (
    <Navbar expand="lg" bg="light" variant="light" className="shadow-sm">
      <Container>
        <Navbar.Brand as={Link} to="/" className="me-4">Biofeedback App</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          {/* Hlavní navigace — zobrazit jen pokud je user */}
          {user && (
            <Nav className="me-auto">
              {navigation.map((item) => (
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
          )}

          {/* PRAVÁ STRANA */}
          <Nav className="align-items-center">
            {/* zvonek — skryt pokud neni user */}
            {user && <Nav.Link href="#" className="position-relative me-3"><Bell size={20} /></Nav.Link>}

            {user ? (
              <Dropdown align="end">
                <Dropdown.Toggle as="div" className="nav-link position-relative me-3" style={{cursor:"pointer"}}>
                  <PersonCircle size={20} />
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.ItemText>{user.name || user.email}</Dropdown.ItemText>
                  <Dropdown.Divider />
                  <Dropdown.Item onClick={logout}>Odhlásit</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            ) : (
              <>
                <Nav.Link onClick={() => setShowLogin(true)} className="position-relative me-2" style={{cursor:"pointer"}}>
                  <PersonCircle size={20} />
                </Nav.Link>
                {/* skryt registraci pokud nechceš aby nebyla dostupná */}
                <Button variant="outline-primary" size="sm" onClick={() => setShowRegister(true)}>Registrovat</Button>
              </>
            )}

            <LoginModal show={showLogin} onHide={() => setShowLogin(false)} />
            <RegisterModal show={showRegister} onHide={() => setShowRegister(false)} onRegistered={() => setShowLogin(true)} />
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}