export default function VerticalMeter({ label, value, color }) {
  return (
    <div className="meter-container">
      <div className="meter-bar">
        <div
          className="meter-fill"
          style={{ height: `${value}%`, backgroundColor: color }}
        />
      </div>
      {/* <p>{label}: {value}</p> */}
    </div>
  );
}
