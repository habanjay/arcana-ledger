interface SectionHeadingProps {
  title: string;
  subtitle: string;
  action?: string;
}

export function SectionHeading({ title, subtitle, action }: SectionHeadingProps) {
  return (
    <div className="section-head">
      <div><h2>{title}</h2><p>{subtitle}</p></div>
      {action && <button className="link-button" type="button">{action}</button>}
    </div>
  );
}
