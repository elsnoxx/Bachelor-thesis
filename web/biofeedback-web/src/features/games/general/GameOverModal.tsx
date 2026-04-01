import { Modal, Button } from "react-bootstrap";
import { Trophy, EmojiFrown } from "react-bootstrap-icons";

interface GameOverModalProps {
    show: boolean;
    result: { isWin: boolean, reason: string | null } | null;
    onRetry: () => void;
}

export default function GameOverModal({ show, result, onRetry }: GameOverModalProps) {
    return (
        <Modal show={show} centered backdrop="static">
            <Modal.Body className="text-center p-5">
                {result?.isWin ? (
                    <>
                        <Trophy size={80} className="text-success mb-3" />
                        <h1 className="display-5 font-black text-success">SPOLEČNÝ ÚSPĚCH!</h1>
                    </>
                ) : (
                    <>
                        <EmojiFrown size={80} className="text-danger mb-3" />
                        <h1 className="display-5 font-black text-danger">JE KONEC...</h1>
                    </>
                )}

                <p className="lead text-muted mt-3">
                    {result?.reason}
                </p>

                <div className="mt-4">
                    <Button 
                        variant={result?.isWin ? "success" : "danger"} 
                        size="lg" 
                        onClick={onRetry} 
                        className="rounded-pill px-5"
                    >
                        Zpět do lobby
                    </Button>
                </div>
            </Modal.Body>
        </Modal>
    );
}