export function TopBar({ title = 'Home' }: { title?: string }) {
  return <header className="topbar">
    <div className="page-title">{title}</div>
    <div className="top-actions">
      <label className="search"><input type="search" placeholder="Search" aria-label="Search" /><span aria-hidden="true">⌕</span></label>
      <button className="icon-button" type="button" aria-label="Notifications">♧</button>
      <div className="avatar" aria-label="Profile">AL</div>
    </div>
  </header>;
}
