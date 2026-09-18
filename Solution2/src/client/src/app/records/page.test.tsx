import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { toast } from "sonner";
import RecordsPage, { fromSelect } from "@/app/records/page";
import type { RecordDto, ZoneDto } from "@/lib/api";

vi.mock("sonner", async (importOriginal) => {
  const actual = await importOriginal<typeof import("sonner")>();
  return {
    ...actual,
    toast: { success: vi.fn(), error: vi.fn() },
  };
});

const ZONES: ZoneDto[] = [
  {
    id: 1,
    name: "nahuexolab.com",
    recordCount: 5,
    nsCount: 4,
    createdUtc: "2024-05-24T20:06:01Z",
    updatedUtc: "2024-05-24T20:06:01Z",
  },
  {
    id: 2,
    name: "demo.example",
    recordCount: 8,
    nsCount: 4,
    createdUtc: "2024-05-24T20:06:01Z",
    updatedUtc: "2024-05-24T20:06:01Z",
  },
];

const RECORDS: RecordDto[] = [
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
  {
    id: 9,
    zoneId: 1,
    zoneName: "nahuexolab.com",
    name: "_dmarc",
    fqdn: "_dmarc.nahuexolab.com",
    type: "TXT",
    ttl: 3600,
    data: "v=DMARC1",
    updatedUtc: "2024-05-24T20:06:01Z",
  },
];

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

function mockApiError(status: number, errors: string[]) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({
      ok: false,
      status,
      json: async () => ({ errors }),
    })),
  );
}

function apiData(url: string): unknown {
  if (url.includes("/api/zones")) {
    return ZONES;
  }
  return RECORDS;
}

describe("fromSelect", () => {
  it("maps select values back to filters", () => {
    expect(fromSelect(null)).toBe("");
    expect(fromSelect("__all")).toBe("");
    expect(fromSelect("2")).toBe("2");
  });
});

describe("RecordsPage", () => {
  it("shows loading, rows, badges, and footer total", async () => {
    mockApi(apiData);
    render(<RecordsPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
    expect(await screen.findByText("nahuexolab.com")).toBeInTheDocument();
    expect(screen.getByText("_dmarc.nahuexolab.com")).toBeInTheDocument();
    expect(screen.getByText("NS")).toBeInTheDocument();
    const dataCell = screen.getByText("v=DMARC1").closest("td");
    expect(dataCell).toHaveClass("whitespace-normal");
    expect(dataCell).not.toHaveClass("whitespace-nowrap");
    expect(document.querySelector("tfoot")?.textContent).toBe(
      "Total: 2 records",
    );
  });

  it("shows an empty state and load errors", async () => {
    mockApi((url) => (url.includes("/api/zones") ? ZONES : []));
    render(<RecordsPage />);
    expect(await screen.findByText(/No records match/)).toBeInTheDocument();

    mockApiError(500, ["Down."]);
    render(<RecordsPage />);
    await waitFor(() => expect(toast.error).toHaveBeenCalledWith("Down."));
  });

  it("applies and clears filters with the zone meter", async () => {
    const user = userEvent.setup();
    mockApi(apiData);
    render(<RecordsPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getByLabelText("Zone"));
    await user.click(
      await screen.findByRole("option", { name: "demo.example" }),
    );
    await user.type(screen.getByLabelText("Search"), "www");
    await user.click(screen.getByLabelText("Type"));
    await user.click(await screen.findByRole("option", { name: "A" }));
    await user.click(screen.getByRole("button", { name: "Apply" }));
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/records?zoneId=2&search=www&type=A"),
      expect.anything(),
    );
    expect(await screen.findByText("8 / 10 records")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(fetch).toHaveBeenCalledWith(
      "https://localhost:7264/api/records",
      expect.anything(),
    );

    await user.click(screen.getByRole("button", { name: "Apply" }));
    expect(fetch).toHaveBeenCalledWith(
      "https://localhost:7264/api/records",
      expect.anything(),
    );
  });

  it("creates a record with validation and server errors", async () => {
    const user = userEvent.setup();
    mockApi(apiData);
    render(<RecordsPage />);
    await screen.findByText("nahuexolab.com");

    expect(screen.getByRole("button", { name: "New record" })).toBeEnabled();

    await user.click(screen.getByRole("button", { name: "New record" }));
    const fresh = await screen.findByRole("dialog");
    expect(within(fresh).getByLabelText("Zone").textContent).toContain(
      "Pick a zone",
    );
    await user.click(within(fresh).getByRole("button", { name: "Save" }));
    expect(
      await within(fresh).findByText(/Pick a zone first/),
    ).toBeInTheDocument();

    await user.click(within(fresh).getByLabelText("Zone"));
    await user.click(
      await screen.findByRole("option", { name: "nahuexolab.com" }),
    );
    await user.click(within(fresh).getByRole("button", { name: "Save" }));
    expect(
      await within(fresh).findByText(/Pick a record type/),
    ).toBeInTheDocument();
    await user.type(within(fresh).getByLabelText("Record name"), "www");
    await user.click(within(fresh).getByLabelText("Type"));
    await user.click(await screen.findByRole("option", { name: "A" }));
    await user.type(within(fresh).getByLabelText("TTL (seconds)"), "300");
    await user.type(within(fresh).getByLabelText("Data"), "10.0.0.1");
    await user.click(within(fresh).getByRole("button", { name: "Save" }));
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith("Record was created."),
    );
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/records"),
      expect.objectContaining({
        method: "POST",
        body: expect.stringContaining('"zoneId":1'),
      }),
    );

    await user.click(screen.getByRole("button", { name: "New record" }));
    const reopened = await screen.findByRole("dialog");
    expect(within(reopened).getByLabelText("Record name")).toHaveValue("");
    await user.click(within(reopened).getByRole("button", { name: "Save" }));
    expect(
      await within(reopened).findByText(/Pick a zone first/),
    ).toBeInTheDocument();

    mockApiError(409, ["This exact record already exists in the zone."]);
    await user.click(within(reopened).getByLabelText("Zone"));
    await user.click(
      await screen.findByRole("option", { name: "nahuexolab.com" }),
    );
    await user.click(within(reopened).getByLabelText("Type"));
    await user.click(await screen.findByRole("option", { name: "TXT" }));
    await user.type(within(reopened).getByLabelText("TTL (seconds)"), "300");
    await user.type(within(reopened).getByLabelText("Data"), "x");
    await user.type(within(reopened).getByLabelText("Record name"), "dup");
    await user.click(within(reopened).getByRole("button", { name: "Save" }));
    expect(
      await within(reopened).findByText(/already exists/),
    ).toBeInTheDocument();
    await user.click(within(reopened).getByRole("button", { name: "Cancel" }));
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );
  });

  it("edits a record", async () => {
    const user = userEvent.setup();
    mockApi(apiData);
    render(<RecordsPage />);
    await screen.findByText("_dmarc.nahuexolab.com");

    await user.click(screen.getAllByRole("button", { name: "Edit" })[1]);
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByLabelText("Record name")).toHaveValue("_dmarc");
    await user.clear(within(dialog).getByLabelText("TTL (seconds)"));
    await user.type(within(dialog).getByLabelText("TTL (seconds)"), "7200");
    await user.click(within(dialog).getByRole("button", { name: "Save" }));
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith("Record was updated."),
    );
  });

  it("deletes a record, resets filters, and reports blocks", async () => {
    const user = userEvent.setup();
    mockApi(apiData);
    render(<RecordsPage />);
    await screen.findByText("_dmarc.nahuexolab.com");

    await user.click(screen.getAllByRole("button", { name: "Delete" })[1]);
    const dialog = await screen.findByRole("dialog");
    expect(
      within(dialog).getByText("_dmarc.nahuexolab.com"),
    ).toBeInTheDocument();
    await user.keyboard("{Escape}");
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );

    await user.click(screen.getAllByRole("button", { name: "Delete" })[1]);
    const cancel = await screen.findByRole("dialog");
    fireEvent.click(within(cancel).getByRole("button", { name: "Cancel" }));
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );

    await user.click(screen.getAllByRole("button", { name: "Delete" })[1]);
    const confirm = await screen.findByRole("dialog");
    await user.click(
      within(confirm).getByRole("button", { name: "Delete record" }),
    );
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith("Record was deleted."),
    );
    await waitFor(() =>
      expect(fetch).toHaveBeenCalledWith(
        "https://localhost:7264/api/records",
        expect.anything(),
      ),
    );

    await user.click(screen.getAllByRole("button", { name: "Delete" })[0]);
    const blocked = await screen.findByRole("dialog");
    mockApiError(400, ["A zone must keep at least 4 NS records."]);
    await user.click(
      within(blocked).getByRole("button", { name: "Delete record" }),
    );
    await waitFor(() =>
      expect(toast.error).toHaveBeenCalledWith(
        "A zone must keep at least 4 NS records.",
      ),
    );
  });

  it("links the filtered CSV export", async () => {
    const user = userEvent.setup();
    mockApi(apiData);
    render(<RecordsPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getByLabelText("Zone"));
    await user.click(
      await screen.findByRole("option", { name: "nahuexolab.com" }),
    );
    await user.click(screen.getByRole("button", { name: "Apply" }));
    const link = await screen.findByRole("link", { name: "Export CSV" });
    expect(link).toHaveAttribute(
      "href",
      expect.stringContaining("/api/records/export?zoneId=1"),
    );
  });
});
