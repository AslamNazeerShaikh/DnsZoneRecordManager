import { describe, expect, it, vi, afterEach } from "vitest";
import { ApiError, apiErrorMessages, recordsApi, zonesApi } from "@/lib/api";

function mockFetch(
  data: unknown,
  options: { ok?: boolean; status?: number } = {},
) {
  const { ok = true, status = 200 } = options;
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({
      ok,
      status,
      json: async () => data,
    })),
  );
}

function mockFetchRaw(
  handler: (url: string, init?: RequestInit) => Promise<Response>,
) {
  vi.stubGlobal("fetch", vi.fn(handler));
}

afterEach(() => {
  vi.unstubAllGlobals();
  delete process.env.NEXT_PUBLIC_API_URL;
});

describe("ApiError", () => {
  it("carries status and messages", () => {
    const error = new ApiError(404, ["Zone not found."]);
    expect(error).toBeInstanceOf(Error);
    expect(error.name).toBe("ApiError");
    expect(error.status).toBe(404);
    expect(error.messages).toEqual(["Zone not found."]);
    expect(error.message).toBe("Zone not found.");
  });
});

describe("apiErrorMessages", () => {
  it("unwraps ApiError messages", () => {
    expect(apiErrorMessages(new ApiError(400, ["a", "b"]))).toEqual(["a", "b"]);
  });

  it("falls back for unknown errors", () => {
    expect(apiErrorMessages(new Error("boom"))).toEqual([
      "Something went wrong. Please try again.",
    ]);
    expect(apiErrorMessages(null)).toEqual([
      "Something went wrong. Please try again.",
    ]);
  });
});

describe("baseUrl", () => {
  it("uses the environment override when set", async () => {
    process.env.NEXT_PUBLIC_API_URL = "http://api.test";
    mockFetch([]);
    await zonesApi.list();
    expect(fetch).toHaveBeenCalledWith(
      "http://api.test/api/zones",
      expect.anything(),
    );
  });

  it("defaults to the local HTTPS API", async () => {
    mockFetch([]);
    await zonesApi.list();
    expect(fetch).toHaveBeenCalledWith(
      "https://localhost:7264/api/zones",
      expect.anything(),
    );
  });
});

describe("request", () => {
  it("returns undefined for 204 responses", async () => {
    mockFetchRaw(async () => new Response(null, { status: 204 }));
    await expect(zonesApi.remove(1)).resolves.toBeUndefined();
  });

  it("throws ApiError with server messages on failure", async () => {
    mockFetch({ errors: ["Zone not found."] }, { ok: false, status: 404 });
    const error = await zonesApi.get(999).catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(404);
    expect((error as ApiError).messages).toEqual(["Zone not found."]);
  });

  it("throws a generic ApiError when the body has no errors", async () => {
    mockFetchRaw(
      async () =>
        new Response("boom", {
          status: 500,
          headers: { "Content-Type": "text/plain" },
        }),
    );
    const error = await zonesApi.get(1).catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).messages).toEqual(["Request failed (500)."]);
  });
});

describe("zonesApi", () => {
  it("lists with and without search", async () => {
    mockFetch([]);
    await zonesApi.list();
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones"),
      expect.anything(),
    );
    await zonesApi.list("nahu");
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones?search=nahu"),
      expect.anything(),
    );
  });

  it("creates, renames, and deletes", async () => {
    mockFetch({ id: 1 });
    await zonesApi.create("x.example");
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones"),
      expect.objectContaining({ method: "POST" }),
    );
    await zonesApi.rename(1, "y.example");
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones/1"),
      expect.objectContaining({ method: "PUT" }),
    );
    await zonesApi.remove(1);
  });

  it("builds detail urls", async () => {
    mockFetch({ id: 7 });
    await zonesApi.get(7);
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones/7"),
      expect.anything(),
    );
  });
});

describe("recordsApi", () => {
  it("lists with filters and builds export urls", async () => {
    mockFetch([]);
    await recordsApi.list();
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/records"),
      expect.anything(),
    );
    await recordsApi.list(2, "www", "A");
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/records?zoneId=2&search=www&type=A"),
      expect.anything(),
    );
    expect(recordsApi.exportUrl()).toContain("/api/records/export");
    expect(recordsApi.exportUrl(2, "www", "TXT")).toContain(
      "zoneId=2&search=www&type=TXT",
    );
  });

  it("creates, updates, gets, and deletes", async () => {
    mockFetch({ id: 3 });
    await recordsApi.create({
      zoneId: 2,
      name: "www",
      type: "A",
      ttl: 300,
      data: "10.0.0.1",
    });
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/records"),
      expect.objectContaining({ method: "POST" }),
    );
    await recordsApi.update(3, {
      name: "www",
      type: "A",
      ttl: 600,
      data: "10.0.0.2",
    });
    await recordsApi.get(3);
    await recordsApi.remove(3);
  });
});
