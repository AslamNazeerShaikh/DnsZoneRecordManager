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
import ZonesPage from "@/app/zones/page";
import type { ZoneDto } from "@/lib/api";

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

function mockZones(data: unknown) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({
      ok: true,
      status: 200,
      json: async () => data,
    })),
  );
}

function mockZonesError(status: number, errors: string[]) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({
      ok: false,
      status,
      json: async () => ({ errors }),
    })),
  );
}

describe("ZonesPage", () => {
  it("shows loading, rows, meter, and footer total", async () => {
    mockZones(ZONES);
    render(<ZonesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
    expect(await screen.findByText("nahuexolab.com")).toBeInTheDocument();
    expect(screen.getByText("demo.example")).toBeInTheDocument();
    expect(screen.getByText("5 / 10")).toBeInTheDocument();
    expect(document.querySelector("tfoot")?.textContent).toBe("Total: 2 zones");
  });

  it("shows an empty state and load errors", async () => {
    mockZones([]);
    render(<ZonesPage />);
    expect(await screen.findByText(/No zones found/)).toBeInTheDocument();

    mockZonesError(500, ["Down."]);
    render(<ZonesPage />);
    await waitFor(() => expect(toast.error).toHaveBeenCalledWith("Down."));
  });

  it("searches and clears", async () => {
    const user = userEvent.setup();
    mockZones(ZONES);
    render(<ZonesPage />);
    await screen.findByText("nahuexolab.com");

    await user.type(screen.getByLabelText("Search zones"), "nahu");
    await user.click(screen.getByRole("button", { name: "Search" }));
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/zones?search=nahu"),
      expect.anything(),
    );

    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(fetch).toHaveBeenCalledWith(
      "https://localhost:7264/api/zones",
      expect.anything(),
    );

    await user.click(screen.getByRole("button", { name: "Search" }));
    expect(fetch).toHaveBeenCalledWith(
      "https://localhost:7264/api/zones",
      expect.anything(),
    );
  });

  it("creates a zone and shows server errors", async () => {
    const user = userEvent.setup();
    mockZones(ZONES);
    render(<ZonesPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getByRole("button", { name: "New zone" }));
    const dialog = await screen.findByRole("dialog");
    await user.type(within(dialog).getByLabelText("Zone name"), "new.example");
    await user.click(within(dialog).getByRole("button", { name: "Save" }));
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith(
        expect.stringContaining("was created"),
      ),
    );

    await user.click(screen.getByRole("button", { name: "New zone" }));
    const retry = await screen.findByRole("dialog");
    expect(within(retry).getByLabelText("Zone name")).toHaveValue("");
    await user.type(within(retry).getByLabelText("Zone name"), "dup.example");
    mockZonesError(409, ["Zone 'dup.example' already exists."]);
    await user.click(within(retry).getByRole("button", { name: "Save" }));
    expect(
      await within(retry).findByText(/already exists/),
    ).toBeInTheDocument();
    await user.click(within(retry).getByRole("button", { name: "Cancel" }));
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );
  });

  it("renames a zone", async () => {
    const user = userEvent.setup();
    mockZones(ZONES);
    render(<ZonesPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getAllByRole("button", { name: "Rename" })[0]);
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByLabelText("Zone name")).toHaveValue(
      "nahuexolab.com",
    );
    await user.clear(within(dialog).getByLabelText("Zone name"));
    await user.type(within(dialog).getByLabelText("Zone name"), "ren.example");
    await user.click(within(dialog).getByRole("button", { name: "Save" }));
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith(
        expect.stringContaining("renamed to"),
      ),
    );
  });

  it("deletes a zone after confirm and on cancel", async () => {
    const user = userEvent.setup();
    mockZones(ZONES);
    render(<ZonesPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getAllByRole("button", { name: "Delete" })[0]);
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText(/5 records/)).toBeInTheDocument();
    await user.keyboard("{Escape}");
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );

    await user.click(screen.getAllByRole("button", { name: "Delete" })[0]);
    const cancel = await screen.findByRole("dialog");
    fireEvent.click(within(cancel).getByRole("button", { name: "Cancel" }));
    await waitFor(() =>
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument(),
    );

    await user.click(screen.getAllByRole("button", { name: "Delete" })[0]);
    const confirm = await screen.findByRole("dialog");
    await user.click(
      within(confirm).getByRole("button", { name: "Delete zone" }),
    );
    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith(
        expect.stringContaining("were deleted"),
      ),
    );
  });

  it("shows delete failures", async () => {
    const user = userEvent.setup();
    mockZones(ZONES);
    render(<ZonesPage />);
    await screen.findByText("nahuexolab.com");

    await user.click(screen.getAllByRole("button", { name: "Delete" })[0]);
    const dialog = await screen.findByRole("dialog");
    mockZonesError(404, ["Zone not found."]);
    await user.click(
      within(dialog).getByRole("button", { name: "Delete zone" }),
    );
    await waitFor(() =>
      expect(toast.error).toHaveBeenCalledWith("Zone not found."),
    );
  });
});
