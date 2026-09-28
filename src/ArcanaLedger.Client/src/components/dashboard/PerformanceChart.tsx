export function PerformanceChart() {
  return <svg className="chart" viewBox="0 0 620 190" preserveAspectRatio="none" role="img" aria-label="Portfolio performance over the last 30 trading days">
    {[25, 72, 119, 166].map((y) => <line className="grid-line" key={y} x1="30" y1={y} x2="610" y2={y} />)}
    {[['263k', 28], ['260k', 75], ['258k', 122], ['256k', 169]].map(([label, y]) => <text className="chart-label" key={label} x="0" y={y}>{label}</text>)}
    <polyline className="line-pink" points="30,70 80,63 130,78 180,55 230,73 280,62 330,91 380,79 430,112 480,101 530,126 580,116 610,132" />
    <polyline className="line-green" points="30,48 80,35 130,51 180,39 230,47 280,30 330,49 380,42 430,68 480,61 530,78 580,70 610,84" />
    <circle cx="610" cy="84" r="5" fill="currentColor" />
  </svg>;
}
