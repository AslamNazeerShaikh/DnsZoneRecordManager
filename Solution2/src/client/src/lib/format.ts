/** Formats an ISO UTC timestamp as `yyyy-MM-dd HH:mm` (UTC everywhere). */
export function formatUtc(value: string): string {
  const date = new Date(value);
  const pad = (part: number) => part.toString().padStart(2, "0");
  return `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())} ${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())}`;
}
