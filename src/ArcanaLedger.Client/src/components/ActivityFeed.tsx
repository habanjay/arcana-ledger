import type { ActivityItem } from '../types/dashboard';
import { SectionHeading } from './SectionHeading';

export function ActivityFeed({ items }: { items: ActivityItem[] }) {
  return <section className="panel activity"><SectionHeading title="Recent activity" subtitle="Your latest portfolio events" action="View all" /><div className="activity-list">{items.map((item) => <div className="activity-row" key={item.title}><span><strong>{item.title}</strong><br />{item.detail}</span><time>{item.time}</time></div>)}</div></section>;
}
