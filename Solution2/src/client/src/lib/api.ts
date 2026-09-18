/**
 * Typed fetch client for the DNS Manager Web API.
 *
 * Server is authoritative: every failure surfaces as {@link ApiError} with
 * the HTTP status and the server's guided messages. Request shapes mirror
 * `Solution1/.../Controllers/Api` DTOs (camelCase JSON, string enums).
 */
function baseUrl(): string {
  return process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7264";
}

export type RecordType = "A" | "AAAA" | "CNAME" | "NS" | "TXT";

export interface ZoneDto {
  id: number;
  name: string;
  recordCount: number;
  nsCount: number;
  createdUtc: string;
  updatedUtc: string;
}

export interface RecordDto {
  id: number;
  zoneId: number;
  zoneName: string;
  name: string;
  fqdn: string;
  type: RecordType;
  ttl: number;
  data: string;
  updatedUtc: string;
}

/** Typed API failure: HTTP status plus the server's guided messages. */
export class ApiError extends Error {
  status: number;
  messages: string[];

  constructor(status: number, messages: string[]) {
    super(messages.join(" "));
    this.name = "ApiError";
    this.status = status;
    this.messages = messages;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl()}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });
  if (response.status === 204) {
    return undefined as T;
  }
  const body = (await response.json().catch(() => null)) as {
    errors?: string[];
  } | null;
  if (!response.ok) {
    throw new ApiError(
      response.status,
      body?.errors ?? [`Request failed (${response.status}).`],
    );
  }
  return body as T;
}

function query(params: Record<string, string | undefined>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") {
      search.set(key, value);
    }
  }
  const text = search.toString();
  return text ? `?${text}` : "";
}

export const zonesApi = {
  list: (search?: string) =>
    request<ZoneDto[]>(`/api/zones${query({ search })}`),
  get: (id: number) => request<ZoneDto>(`/api/zones/${id}`),
  create: (name: string) =>
    request<ZoneDto>("/api/zones", {
      method: "POST",
      body: JSON.stringify({ name }),
    }),
  rename: (id: number, name: string) =>
    request<ZoneDto>(`/api/zones/${id}`, {
      method: "PUT",
      body: JSON.stringify({ id, name }),
    }),
  remove: (id: number) =>
    request<void>(`/api/zones/${id}`, { method: "DELETE" }),
};

export const recordsApi = {
  list: (zoneId?: number, search?: string, type?: RecordType) =>
    request<RecordDto[]>(
      `/api/records${query({ zoneId: zoneId?.toString(), search, type })}`,
    ),
  get: (id: number) => request<RecordDto>(`/api/records/${id}`),
  create: (input: {
    zoneId: number;
    name: string;
    type: RecordType;
    ttl: number;
    data: string;
  }) =>
    request<RecordDto>("/api/records", {
      method: "POST",
      body: JSON.stringify(input),
    }),
  update: (
    id: number,
    input: { name: string; type: RecordType; ttl: number; data: string },
  ) =>
    request<RecordDto>(`/api/records/${id}`, {
      method: "PUT",
      body: JSON.stringify({ id, ...input }),
    }),
  remove: (id: number) =>
    request<void>(`/api/records/${id}`, { method: "DELETE" }),
  exportUrl: (zoneId?: number, search?: string, type?: RecordType) =>
    `${baseUrl()}/api/records/export${query({ zoneId: zoneId?.toString(), search, type })}`,
};

export function apiErrorMessages(error: unknown): string[] {
  if (error instanceof ApiError) {
    return error.messages;
  }
  return ["Something went wrong. Please try again."];
}
