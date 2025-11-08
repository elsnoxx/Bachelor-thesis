import React, { useState } from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';

interface GameRoomCreationForm {
    userId: string;
    name: string;
    password?: string;
    maxPlayers: number;
}

interface CreateGameModalProps {
    show: boolean;
    onHide: () => void;
}

export default function CreateGameModal({ show, onHide }: CreateGameModalProps) {
    const [formData, setFormData] = useState<GameRoomCreationForm>({
        userId: '', // Toto by mělo být získáno z autentizace
        name: '',
        password: '',
        maxPlayers: 2
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'maxPlayers' ? parseInt(value) || 2 : value
        }));
        
        // Vymazat chybu pro dané pole
        if (errors[name]) {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[name];
                return newErrors;
            });
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.name.trim()) {
            newErrors.name = 'Název hry je povinný';
        }

        if (formData.maxPlayers < 2 || formData.maxPlayers > 4) {
            newErrors.maxPlayers = 'Počet hráčů musí být mezi 2 a 4';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!validateForm()) {
            return;
        }

        setIsSubmitting(true);

        try {
            // Automaticky přidáme gameType: "ludo"
            let userId = localStorage.getItem('authToken');
            const gameData = {
                userId: userId,
                name: formData.name,
                gameType: 'ludo',
                password: formData.password || "",
                maxPlayers: formData.maxPlayers
            };

            console.log('Creating game room with data:', gameData);
            let token = localStorage.getItem('authToken');
            // Příklad API callu:
            const apiUrl = import.meta.env.VITE_API_URL;
            const response = await fetch(`${apiUrl}/gamerooms`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(gameData)
            });
            console.log('API response:', response);
            
            alert('Hra byla úspěšně vytvořena!');
            onHide(); // Zavřít modal
            
            // Reset formuláře
            setFormData({
                userId: '',
                name: '',
                password: '',
                maxPlayers: 2
            });
            setErrors({});
            
        } catch (error) {
            console.error('Chyba při vytváření hry:', error);
            alert('Nastala chyba při vytváření hry');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        setErrors({});
        onHide();
    };

    return (
        <Modal show={show} onHide={handleClose} size="lg" centered>
            <Modal.Header closeButton>
                <Modal.Title>Vytvořit novou LUDO hru</Modal.Title>
            </Modal.Header>
            
            <Form onSubmit={handleSubmit}>
                <Modal.Body>
                    <Form.Group className="mb-3">
                        <Form.Label>Název hry *</Form.Label>
                        <Form.Control
                            type="text"
                            name="name"
                            value={formData.name}
                            onChange={handleInputChange}
                            placeholder="Zadejte název hry"
                            isInvalid={!!errors.name}
                        />
                        <Form.Control.Feedback type="invalid">
                            {errors.name}
                        </Form.Control.Feedback>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Maximální počet hráčů *</Form.Label>
                        <Form.Control
                            type="number"
                            name="maxPlayers"
                            value={formData.maxPlayers}
                            onChange={handleInputChange}
                            min="2"
                            max="4"
                            isInvalid={!!errors.maxPlayers}
                        />
                        <Form.Control.Feedback type="invalid">
                            {errors.maxPlayers}
                        </Form.Control.Feedback>
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
                        <Form.Text className="text-muted">
                            Pokud chcete, aby se do hry mohli připojit pouze hráči se znalostí hesla
                        </Form.Text>
                    </Form.Group>
                </Modal.Body>
                
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleClose}>
                        Zrušit
                    </Button>
                    <Button 
                        variant="primary" 
                        type="submit" 
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? 'Vytváří se...' : 'Vytvořit hru'}
                    </Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}