export default function LudoCell({ type, children }) {
  const getColor = () => {
    switch (type) {
      case "RED": return "bg-red-400";
      case "BLUE": return "bg-blue-400";
      case "GREEN": return "bg-green-400";
      case "YELLOW": return "bg-yellow-400";
      case "SAFE": return "bg-gray-300";
      default: return "bg-white";
    }
  };

  return (
    <div className={`flex items-center justify-center border border-gray-300 ${getColor()} w-full h-full`}>
      {children}
    </div>
  );
}
