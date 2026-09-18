import { render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import DashboardPage from "@/app/page";

function mockApi(handler: (url: string) => unknown) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: unknown) => ({
      ok: true,
      status: 200,
      json: async () => handler(url as string),
    })),
  );
}

const ZONES = [
  {
    id: 1,
    name: "nahuexolab.com",
    recordCount: 5,
    nsCount: 4,
    createdUtc: "2024-05-24T20:06:01Z",
    updatedUtc: "2024-05-24T20:06:01Z",
  },
];

const RECORDS = [
  {
    id: 1,
    zoneId: 1,
    zoneName: "nahuexolab.com",
    name: "@",
    fqdn: "nahuexolab.com",
    type: "NS",
    ttl: 172800,
    data: "ns1-33.azure-dns.com.",
    updatedUtc: "2024-05-24T20:06:01Z",
  },
];

describe("DashboardPage", () => {
  it("shows live counts and navigation", async () => {
    mockApi((url) => (url.includes("/api/zones") ? ZONES : RECORDS));
    render(<DashboardPage />);
    expect(await screen.findByText("1 zones")).toBeInTheDocument();
    expect(await screen.findByText("1 records")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Manage zones/ })).toHaveAttribute(
      "href",
      "/zones",
    );
    expect(
      screen.getByRole("link", { name: /Browse records/ }),
    ).toHaveAttribute("href", "/records");
    expect(screen.getByText("min 4 NS")).toBeInTheDocument();
  });

  it("hides counts when the API is unreachable", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () => ({
        ok: false,
        status: 500,
        json: async () => ({ errors: ["Down."] }),
      })),
    );
    render(<DashboardPage />);
    await waitFor(() =>
      expect(screen.queryByText("1 zones")).not.toBeInTheDocument(),
    );
    expect(screen.getByText(/Simple DNS management/)).toBeInTheDocument();
  });

  it("ignores late responses after unmount", async () => {
    const resolvers: ((value: unknown) => void)[] = [];
    vi.stubGlobal(
      "fetch",
      vi.fn(
        () =>
          new Promise((resolve) => {
            resolvers.push(resolve as (value: unknown) => void);
          }),
      ),
    );
    const { unmount } = render(<DashboardPage />);
    unmount();
    for (const resolve of resolvers) {
      resolve({ ok: true, status: 200, json: async () => [] });
    }
    await Promise.resolve();
    expect(screen.queryByText("0 zones")).not.toBeInTheDocument();
  });
});
