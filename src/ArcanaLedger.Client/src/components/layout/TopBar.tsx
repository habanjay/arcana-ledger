export function TopBar({ title = 'Home', onLogout }: { title?: string; onLogout?: () => void }) {
  return <header className="topbar">
    <div className="page-title"><span className="page-kicker">ARCANA LEDGER</span><strong>{title}</strong></div>
    <div className="top-actions">
      <label className="search"><input type="search" placeholder="Search" aria-label="Search" /><span aria-hidden="true">⌕</span></label>
      <button className="icon-button" type="button" aria-label="Notifications">♧</button>
      <div className="avatar" aria-label="Profile">AL</div>
      <button className="logout-button" type="button" aria-label="Log out" title="Log out" onClick={onLogout}><span aria-hidden="true">⏻</span><span>Log out</span></button>
    </div>
  </header>;
}
