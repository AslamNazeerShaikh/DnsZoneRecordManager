import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ThemeProvider } from "next-themes";
import { describe, expect, it, vi } from "vitest";
import { SiteHeader } from "@/components/site-header";

const { mockPath } = vi.hoisted(() => ({ mockPath: { value: "/" } }));

vi.mock("next/navigation", () => ({
  usePathname: () => mockPath.value,
  useRouter: () => ({ push: vi.fn() }),
}));

describe("SiteHeader", () => {
  it("renders nav links with the active page highlighted", () => {
    mockPath.value = "/zones";
    render(<SiteHeader />);
    expect(screen.getByRole("link", { name: "DNS Manager" })).toHaveAttribute(
      "href",
      "/",
    );
    expect(screen.getByRole("link", { name: "Dashboard" })).toHaveAttribute(
      "href",
      "/",
    );
    expect(screen.getByRole("link", { name: "Zones" })).toHaveAttribute(
      "href",
      "/zones",
    );
    expect(screen.getByRole("link", { name: "Records" })).toHaveAttribute(
      "href",
      "/records",
    );
    expect(screen.getByRole("button", { name: "Zones" })).toHaveClass(
      "font-semibold",
    );
  });

  it("toggles the theme between light and dark", async () => {
    mockPath.value = "/";
    const user = userEvent.setup();
    render(
      <ThemeProvider attribute="class">
        <SiteHeader />
      </ThemeProvider>,
    );
    await user.click(screen.getByRole("button", { name: "Toggle theme" }));
    expect(document.documentElement.classList.contains("dark")).toBe(true);
    await user.click(screen.getByRole("button", { name: "Toggle theme" }));
    expect(document.documentElement.classList.contains("dark")).toBe(false);
  });
});
