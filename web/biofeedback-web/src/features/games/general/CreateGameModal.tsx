import React, { useState, useEffect } from 'react';
import { Modal, Button, Form } from 'react-bootstrap';
import { RoomService, type GameType } from '../../../api/RoomService';

interface GameRoomCreationForm {
    name: string;
    password?: string;
    maxPlayers: number;
}

interface CreateGameModalProps {
    show: boolean;
    onHide: () => void;
    gameType: GameType; // Nová prop
}

export default function CreateGameModal({ show, onHide, gameType }: CreateGameModalProps) {
    // Pomocná konstanta pro logiku hráčů
    const isBallon = gameType === 'ballon';
    const defaultMaxPlayers = isBallon ? 4 : 2;

    const [formData, setFormData] = useState<GameRoomCreationForm>({
        name: '',
        password: '',
        maxPlayers: defaultMaxPlayers
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Reset formuláře při změně gameType nebo otevření modalu
    useEffect(() => {
        if (show) {
            setFormData({
                name: '',
                password: '',
                maxPlayers: defaultMaxPlayers
            });
            setErrors({});
        }
    }, [show, gameType, defaultMaxPlayers]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'maxPlayers' ? parseInt(value) || 2 : value
        }));
        
        if (errors[name]) {
            setErrors(prev => {
                const { [name]: _, ...rest } = prev;
                return rest;
            });
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!formData.name.trim()) newErrors.name = 'Název hry je povinný';
        
        // Dynamická validace na základě typu hry
        const maxLimit = isBallon ? 4 : 2;
        if (formData.maxPlayers < 2 || formData.maxPlayers > maxLimit) {
            newErrors.maxPlayers = `Počet hráčů musí být ${isBallon ? 'mezi 2 a 4' : 'přesně 2'}`;
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    // Pomocná funkce pro získání userId (v reálném appu použij context nebo auth hook)
    const getUserIdAndToken = () => {
        const userJson = localStorage.getItem('user');
        const token = localStorage.getItem('token');
        let userId: string | null = null;

        if (userJson) {
            try { userId = JSON.parse(userJson).email; } catch {}
        }
        if (!userId && token && token !== 'null') {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                userId = payload.sub || payload.id || payload.userId;
            } catch {}
        }
        return { userId, token };
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validateForm()) return;

        setIsSubmitting(true);
            const { userId } = getUserIdAndToken();

            if (!userId) {
                alert('Nelze zjistit userId. Přihlaste se prosím znovu.');
                setIsSubmitting(false);
                return;
            }

            const gameData = {
                userId,
                name: formData.name,
                gameType, // Použije se dynamicky z props
                password: formData.password || undefined,
                maxPlayers: formData.maxPlayers
            };

            try {
                await RoomService.createRoom(gameData);
                alert('Hra byla úspěšně vytvořena!');
                onHide();
            } catch (error) {
                console.error(error);
                alert('Chyba při vytváření hry.');
            } finally {
                setIsSubmitting(false);
            }
    };

    return (
        <Modal show={show} onHide={onHide} size="lg" centered>
            <Modal.Header closeButton>
                <Modal.Title>Vytvořit novou hru: {gameType.toUpperCase()}</Modal.Title>
            </Modal.Header>
            <Form onSubmit={handleSubmit}>
                <Modal.Body>
                    <Form.Group className="mb-3">
                        <Form.Label>Název hry *</Form.Label>
                        <Form.Control
                            name="name"
                            value={formData.name}
                            onChange={handleInputChange}
                            isInvalid={!!errors.name}
                            placeholder="Zadejte název hry"
                        />
                        <Form.Control.Feedback type="invalid">{errors.name}</Form.Control.Feedback>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Maximální počet hráčů *</Form.Label>
                        <Form.Control
                            type="number"
                            name="maxPlayers"
                            value={formData.maxPlayers}
                            onChange={handleInputChange}
                            min="2"
                            max={isBallon ? "4" : "2"}
                            disabled={!isBallon}
                            isInvalid={!!errors.maxPlayers}
                        />
                        <Form.Control.Feedback type="invalid">{errors.maxPlayers}</Form.Control.Feedback>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Heslo (nepovinné)</Form.Label>
                        <Form.Control
                            type="password"
                            name="password"
                            value={formData.password}
                            onChange={handleInputChange}
                            placeholder="Zadejte heslo pro ochranu hry"
                        />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={onHide}>Zrušit</Button>
                    <Button variant="primary" type="submit" disabled={isSubmitting}>
                        {isSubmitting ? 'Vytváří se...' : 'Vytvořit hru'}
                    </Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}