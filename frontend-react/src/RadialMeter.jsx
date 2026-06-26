export default function RadialMeter({ value }) {
  const radius = 40;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (value / 100) * circumference;

  return (
    <div className="radial-container">
      <svg width="100" height="100">
        <circle
          className="radial-bg"
          cx="50"
          cy="50"
          r={radius}
        />
        <circle
          className="radial-progress"
          cx="50"
          cy="50"
          r={radius}
          strokeDasharray={circumference}
          strokeDashoffset={offset}
        />
      </svg>
      {/* <p>Happiness: {value}</p> */}
    </div>
  );
}
