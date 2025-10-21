import React from "react";
import { Container, Navbar, Nav, Button, Dropdown } from "react-bootstrap";
import { Link, useLocation } from "react-router-dom";
import { Bell, PersonCircle } from "react-bootstrap-icons";

const navigation = [
  { name: 'Hry', href: '/hry' },
  { name: 'Statistiky', href: '/statistiky' }
];

export default function AppsNavbar() {
  const location = useLocation();

  return (
    <Navbar expand="lg" bg="light" variant="light" className="shadow-sm">
      <Container>
        {/* LEVÁ STRANA - Brand + Navigace */}
        <Navbar.Brand as={Link} to="/" className="me-4">
          Biofeedback App
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        
        <Navbar.Collapse id="basic-navbar-nav">
          {/* Hlavní navigace vlevo */}
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

          {/* PRAVÁ STRANA - Notifikace + User menu */}
          <Nav className="align-items-center">
            {/* Zvoneček pro notifikace */}
            <Nav.Link href="#" className="position-relative me-3">
              <Bell size={20} />
            </Nav.Link>
            <Nav.Link href="#" className="position-relative me-3">
              <PersonCircle size={20} onClick={() => console.log("click")} />
            </Nav.Link>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}