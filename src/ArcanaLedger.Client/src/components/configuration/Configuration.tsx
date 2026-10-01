import { useEffect, useState, type ChangeEvent } from "react";
import { configurationDefaults, optionGroups, portfolioFields } from '../../data/configurationData';
import type {
  ConfigurationField,
  ConfigurationValues,
} from "../../types/configuration";
import { NavigationRail } from "../layout/NavigationRail";
import { TopBar } from "../layout/TopBar";

interface ConfigurationProps {
  onNavigate: (label: string) => void;
}

function Field({
  field,
  values,
  onChange,
}: {
  field: ConfigurationField;
  values: ConfigurationValues;
  onChange: (event: ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <div className="field">
      <label htmlFor={`configuration-${field.key}`}>{field.label}</label>
      <input
        id={`configuration-${field.key}`}
        className={field.tone ? `input-${field.tone}` : undefined}
        value={values[field.key]}
        onChange={onChange}
      />
    </div>
  );
}

function PanelHeader({
  title,
  subtitle,
  badge,
}: {
  title: string;
  subtitle: string;
  badge?: string;
}) {
  return (
    <div className="panel-head">
      <div>
        <h2>{title}</h2>
        <p>{subtitle}</p>
      </div>
      {badge && <span className="panel-badge">{badge}</span>}
    </div>
  );
}

const displaySetupName = (setupName: string) => setupName.replace(/^[PSMC] - /, "");

function SetupChart({ setupName, large = false }: { setupName: string; large?: boolean }) {
  const family = setupName.startsWith("P - ") ? "Position" : setupName.startsWith("S - ") ? "Swing" : setupName.startsWith("C - ") ? "Counter trend" : "Momentum";
  const entryX = family === "Position" ? "82" : "66";
  const entryY = family === "Position" ? "15" : "20";
  return <svg className={large ? "setup-chart-large" : "setup-thumbnail"} viewBox={large ? "0 0 520 180" : "0 0 120 42"} role="img" aria-label={`${setupName} sample chart`}><polyline className="thumbnail-line" points={family === "Position" ? (large ? "2,142 70,122 138,130 206,100 274,108 342,75 410,86 518,38" : "2,32 18,27 34,29 50,22 66,24 82,15 98,18 118,9") : (large ? "2,148 70,144 138,136 206,140 274,88 342,98 410,51 470,62 518,18" : "2,34 18,33 34,31 50,32 66,20 82,23 98,12 118,6")} /><line className="thumbnail-entry" x1={large ? entryX === "82" ? "410" : "274" : entryX} y1="4" x2={large ? entryX === "82" ? "410" : "274" : entryX} y2={large ? "170" : "38"} /><circle className="thumbnail-dot" cx={large ? entryX === "82" ? "410" : "274" : entryX} cy={large ? entryY === "15" ? "86" : "88" : entryY} r={large ? "6" : "3"} /></svg>;
}

export function SetupReferencePanel({ onNavigate }: { onNavigate?: (label: string) => void }) {
  const setups = optionGroups.find((group) => group.label === "Setup")?.options ?? [];
  const [selectedSetup, setSelectedSetup] = useState<string | null>(null);
  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === "Escape") setSelectedSetup(null); };
    document.addEventListener("keydown", closeOnEscape);
    return () => document.removeEventListener("keydown", closeOnEscape);
  }, []);
  return (
    <section className="panel setup-reference-panel">
      <PanelHeader title="Configured Trade Setups" subtitle="Each setup has its own execution rules and visual reference" badge="Reference" />
        {onNavigate && <button className="setup-playbook-link" type="button" onClick={() => onNavigate('Setup playbook')}>Open execution playbook</button>}
        <div className="setup-reference-table-wrap">
        <table className="setup-reference-table">
          <thead><tr><th>Order Type</th><th>Setup</th><th>Family</th><th>Trend</th><th>Tranches</th><th>VAR</th><th>Sample</th></tr></thead>
          <tbody>{setups.map((setupName) => {
            const family = setupName.startsWith("P - ") ? "Position" : setupName.startsWith("S - ") ? "Swing" : setupName.startsWith("C - ") ? "Counter trend" : "Momentum";
            return <tr key={setupName}><td>Long</td><td className="setup-name-cell">{displaySetupName(setupName)}</td><td>{family}</td><td>Up</td><td>{family === "Position" ? "2" : "No"}</td><td>1.25</td><td><button className="setup-thumbnail-button" type="button" onClick={() => setSelectedSetup(setupName)} aria-label={`Enlarge ${displaySetupName(setupName)} sample chart`}><SetupChart setupName={setupName} /></button></td></tr>;
          })}</tbody>
        </table>
      </div>
      {selectedSetup && <div className="setup-modal-backdrop" role="presentation" onClick={(event) => { if (event.target === event.currentTarget) setSelectedSetup(null); }}><section className="setup-modal" role="dialog" aria-modal="true" aria-labelledby="setup-modal-title"><div className="setup-modal-head"><div><span className="control-label">Sample chart</span><h2 id="setup-modal-title">{displaySetupName(selectedSetup)}</h2></div><button className="setup-modal-close" type="button" onClick={() => setSelectedSetup(null)} aria-label="Close enlarged chart">×</button></div><SetupChart setupName={selectedSetup} large /></section></div>}
    </section>
  );
}

export interface TradeSetup {
  name: string;
  family: string;
  entry: string;
  exit: string;
  setupType: string;
  trend: string;
  usesTranches: string;
  trancheCount: string;
  varMultiplier: string;
  whenToUse: string;
}

export const defaultTradeSetup: TradeSetup = {
  name: "Blue Sky Breakout",
  family: "Swing",
  entry: "Break and close above resistance",
  exit: "Close below pattern low",
  setupType: "S - Blue Sky Breakout",
  trend: "Up",
  usesTranches: "false",
  trancheCount: "2",
  varMultiplier: "1.25",
  whenToUse:
    "Use when price clears a well-defined base with rising volume and the broader trend is supportive.",
};

export function TradeSetupPanel({
  setup,
  onChange,
}: {
  setup: TradeSetup;
  onChange: (key: keyof TradeSetup, value: string) => void;
}) {
  return (
    <section className="panel trade-setup-panel">
      <div className="panel-head">
        <div>
          <h2>Trade Setup Builder</h2>
          <p>Capture the rules behind a repeatable setup</p>
        </div>
        <span className="panel-badge">Setup</span>
      </div>
      <div className="panel-body">
        <div className="form-grid">
          <div className="field full-field">
            <label htmlFor="trade-setup-name">Setup name</label>
            <input
              id="trade-setup-name"
              value={setup.name}
              onChange={(event) => onChange("name", event.target.value)}
              placeholder="e.g. Blue Sky Breakout"
            />
          </div>
          <div className="field">
            <label htmlFor="trade-setup-family">Setup family</label>
            <select
              id="trade-setup-family"
              value={setup.family}
              onChange={(event) => onChange("family", event.target.value)}
            >
              <option>Position</option>
              <option>Swing</option>
              <option>Momentum</option>
              <option>Counter trend</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="trade-setup-type">Setup type</label>
            <select
              id="trade-setup-type"
              value={setup.setupType}
              onChange={(event) => onChange("setupType", event.target.value)}
            >
              <option value="P - Coil">Coil</option>
              <option value="S - Pullback">Pullback</option>
              <option value="S - Blue Sky Breakout">Blue Sky Breakout</option>
              <option value="M - Expansion Breakout">Expansion Breakout</option>
              <option value="M - Slingshot">Slingshot</option>
              <option value="Others">Others</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="trade-setup-var">VAR multiplier</label>
            <select
              id="trade-setup-var"
              value={setup.varMultiplier}
              onChange={(event) => onChange("varMultiplier", event.target.value)}
            >
              <option>0.25</option>
              <option>0.50</option>
              <option>0.75</option>
              <option>1.00</option>
              <option>1.25</option>
              <option>1.50</option>
              <option>1.75</option>
              <option>2.00</option>
            </select>
          </div>
          {setup.family === "Position" && (
            <>
              <label className="setup-toggle">
                <input
                  type="checkbox"
                  checked={setup.usesTranches === "true"}
                  onChange={(event) =>
                    onChange("usesTranches", String(event.target.checked))
                  }
                />
                <span>Buy this setup in tranches</span>
              </label>
              {setup.usesTranches === "true" && (
                <div className="field">
                  <label htmlFor="trade-setup-tranches">Number of tranches</label>
                  <input
                    id="trade-setup-tranches"
                    type="number"
                    min="2"
                    max="5"
                    value={setup.trancheCount}
                    onChange={(event) => onChange("trancheCount", event.target.value)}
                  />
                </div>
              )}
            </>
          )}
          <div className="field">
            <label htmlFor="trade-setup-trend">Trend</label>
            <select
              id="trade-setup-trend"
              value={setup.trend}
              onChange={(event) => onChange("trend", event.target.value)}
            >
              <option>Up</option>
              <option>Down</option>
              <option>Range</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="trade-setup-entry">Entry rule</label>
            <input
              id="trade-setup-entry"
              value={setup.entry}
              onChange={(event) => onChange("entry", event.target.value)}
              placeholder="What confirms entry?"
            />
          </div>
          <div className="field">
            <label htmlFor="trade-setup-exit">Exit rule</label>
            <input
              id="trade-setup-exit"
              value={setup.exit}
              onChange={(event) => onChange("exit", event.target.value)}
              placeholder="What invalidates the setup?"
            />
          </div>
          <div className="field full-field">
            <label htmlFor="trade-setup-when">When to use</label>
            <textarea
              id="trade-setup-when"
              value={setup.whenToUse}
              onChange={(event) => onChange("whenToUse", event.target.value)}
              rows={3}
            />
          </div>
        </div>
        <div className="sample-chart">
          <div className="sample-chart-head">
            <div>
              <span className="control-label">Sample chart</span>
              <strong>{setup.name || "Untitled setup"}</strong>
            </div>
            <span className="chart-legend">
              <i /> Entry <b /> Exit
            </span>
          </div>
          <svg
            viewBox="0 0 520 150"
            role="img"
            aria-label="Sample breakout chart"
          >
            <line className="sample-grid" x1="0" y1="30" x2="520" y2="30" />
            <line className="sample-grid" x1="0" y1="75" x2="520" y2="75" />
            <line className="sample-grid" x1="0" y1="120" x2="520" y2="120" />
            <polyline
              className="sample-line"
              points="0,108 35,99 65,105 95,83 125,94 155,75 185,82 215,59 245,68 275,49 305,55 335,40 365,47 395,26 425,35 455,18 490,24 520,10"
            />
            <line className="entry-line" x1="365" y1="16" x2="365" y2="138" />
            <line className="exit-line" x1="455" y1="16" x2="455" y2="138" />
            <circle className="entry-dot" cx="365" cy="47" r="5" />
            <circle className="exit-dot" cx="455" cy="18" r="5" />
          </svg>
          <div className="sample-chart-labels">
            <span>Base forms</span>
            <span>Entry</span>
            <span>Target / exit</span>
          </div>
        </div>
      </div>
    </section>
  );
}

export function Configuration({ onNavigate }: ConfigurationProps) {
  const [values, setValues] = useState<ConfigurationValues>(
    configurationDefaults,
  );
  const [currency, setCurrency] = useState("CAD");
  const [saved, setSaved] = useState(false);
  const updateValue = (event: ChangeEvent<HTMLInputElement>) => {
    setValues(
      (current) =>
        ({
          ...current,
          [event.target.id.replace("configuration-", "")]: event.target.value,
        }) as ConfigurationValues,
    );
    setSaved(false);
  };
  const reset = () => {
    setValues(configurationDefaults);
    setCurrency("CAD");
    setSaved(false);
  };
  const save = () => {
    setSaved(true);
    window.setTimeout(() => setSaved(false), 1400);
  };

  return (
    <div className="app">
      <NavigationRail activeLabel="Configuration" onNavigate={onNavigate} />
      <main className="workspace configuration-workspace">
        <TopBar title="Configuration" onLogout={() => onNavigate("Login")} />
        <section
          className="configuration-content"
          aria-label="Trading configuration"
        >
          <div className="page-heading">
            <div>
              <h1>Trading Configuration</h1>
              <p>
                Set portfolio risk, position sizing, and execution rules for
                your active setups.
              </p>
            </div>
            <div className="heading-actions">
              <button className="ghost" type="button" onClick={reset}>
                Reset Changes
              </button>
              <button className="primary" type="button" onClick={save}>
                {saved ? "Saved" : "Save Configuration"}
              </button>
            </div>
          </div>
          <div className="configuration-layout">
            <div className="column">
              <section className="panel">
                <PanelHeader
                  title="Portfolio Risk"
                  subtitle="Base capital and allocation limits"
                  badge="Active"
                />
                <div className="panel-body">
                  <div className="form-grid">
                    <div className="field">
                      <label htmlFor="configuration-currency">Currency</label>
                      <select
                        id="configuration-currency"
                        value={currency}
                        onChange={(event) => {
                          setCurrency(event.target.value);
                          setSaved(false);
                        }}
                      >
                        <option value="CAD">CAD - Canadian dollar</option>
                        <option value="PHP">PHP - Philippine peso</option>
                        <option value="USD">USD - US dollar</option>
                        <option value="EUR">EUR - Euro</option>
                        <option value="GBP">GBP - British pound</option>
                        <option value="JPY">JPY - Japanese yen</option>
                      </select>
                    </div>
                    {portfolioFields.map((field) => (
                      <Field
                        key={field.key}
                        field={field}
                        values={values}
                        onChange={updateValue}
                      />
                    ))}
                  </div>
                  <div className="divider" />
                  <h3 className="section-title">Risk thresholds</h3>
                  <div className="rule-grid">
                    <div className="rule green">
                      <span>VAR @ 0.25%</span>
                      <strong>C$640.26</strong>
                    </div>
                    <div className="rule red">
                      <span>Max daily loss</span>
                      <strong>(C$803.48)</strong>
                    </div>
                    <div className="rule">
                      <span>Position risk</span>
                      <strong>1.25 VAR</strong>
                    </div>
                  </div>
                </div>
              </section>
              <section className="panel">
                <PanelHeader
                  title="Execution Defaults"
                  subtitle="Standard values used when creating a new trade"
                />
                <div className="panel-body">
                  <div className="form-grid">
                    <div className="field">
                      <label htmlFor="order-type-default">Order type</label>
                      <select id="order-type-default" defaultValue="Long">
                        <option>Long</option>
                        <option>Short</option>
                        <option>Sell</option>
                        <option>Cover</option>
                      </select>
                    </div>
                    <div className="field">
                      <label htmlFor="trend-default">Trend</label>
                      <select id="trend-default" defaultValue="Up">
                        <option>Up</option>
                        <option>Down</option>
                      </select>
                    </div>
                  </div>
                </div>
              </section>
            </div>
            <div className="column">
              <section className="panel summary">
                <h2>Configuration Snapshot</h2>
                <p>Current portfolio allocation</p>
                <div className="summary-value">
                  {values.capital}
                  <small>{currency} display currency · VAR capacity C$640.26</small>
                </div>
                <div className="summary-line">
                  <span>Capital at risk</span>
                  <strong>{values.varRate}</strong>
                </div>
                <div className="summary-line">
                  <span>Available cash</span>
                  <strong>{values.cashValue}</strong>
                </div>
              </section>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}
