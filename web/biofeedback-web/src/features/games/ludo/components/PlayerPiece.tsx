export default function PlayerPiece({ color, id }) {
  return (
    <div
      className={`rounded-full w-5 h-5 shadow-lg border-2 border-white transition-all`}
      style={{
        backgroundColor: color,
        boxShadow: `0 0 10px ${color}`,
      }}
      title={`Player ${id}`}
    />
  );
}
