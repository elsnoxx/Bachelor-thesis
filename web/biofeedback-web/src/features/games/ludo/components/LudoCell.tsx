export default function LudoCell({ type, children, text }) {
  const getColor = () => {
    switch (type) {
      case "RED": return "bg-danger";
      case "BLUE": return "bg-primary";
      case "GREEN": return "bg-success";
      case "YELLOW": return "bg-warning";
      case "SAFE-RED": return "bg-danger bg-opacity-50";
      case "SAFE-BLUE": return "bg-primary bg-opacity-50";
      case "SAFE-GREEN": return "bg-success bg-opacity-50";
      case "SAFE-YELLOW": return "bg-warning bg-opacity-50";
      case "SAFE": return "bg-secondary";
      default: return "bg-light";
    }
  };

  const getText = () => {
    return text ? text : '';
  }

  return (
    <div className={`d-flex align-items-center justify-content-center border ${getColor()}`} 
         style={{ 
           width: 'clamp(30px, 4vw, 50px)', 
           height: 'clamp(30px, 4vw, 50px)',
           fontSize: 'clamp(8px, 1.5vw, 12px)'
         }}>
      {children}
      <div className="fw-bold">
        {getText()}
      </div>
    </div>
  );
}