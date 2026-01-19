import React from "react";
import { Container, Row, Col } from "react-bootstrap";
import PlayerPanel from "./components/PlayerPanel";
import BalanceArena from "./components/BalancePanet";

export default function BalanceGame() {
    return (
        <Container fluid className="h-screen py-4">
            {/* Top row: tři sloupce, obsah ve středu */}
            <Row className="mb-4">
                <div className="bg-gray-100 rounded-lg shadow p-4 flex flex-col items-center justify-center text-center">
                    <h3 className="font-semibold">Hlavní objekt</h3>
                    <p className="text-sm text-gray-500">Udrž objekt ve středu pomocí biofeedbacku</p>
                </div>
            </Row>


            {/* Bottom row: tři panely */}
            <Row className="mb-4 align-items-stretch">
                <Col md={3} className="h-100">
                    <PlayerPanel value={600} label="Hráč vlevo" recentMin={700} recentMax={780} />
                </Col>

                <Col md={6} className="h-100">
                    <BalanceArena leftValue={600} rightValue={650} />
                </Col>

                <Col md={3} className="h-100">
                    <PlayerPanel value={650} label="Hráč vpravo" recentMin={620} recentMax={690} />
                </Col>
            </Row>
        </Container>
    );
}