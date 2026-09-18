import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import RootLayout from "@/app/layout";

vi.mock("next/font/google", () => ({
  Inter: () => ({ variable: "font-inter-mock" }),
}));

vi.mock("next/navigation", () => ({
  usePathname: () => "/",
  useRouter: () => ({ push: vi.fn() }),
}));

describe("RootLayout", () => {
  it("renders the header, footer, and children", () => {
    render(
      <RootLayout>
        <p>page body</p>
      </RootLayout>,
    );
    expect(screen.getByText("page body")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "DNS Manager" }),
    ).toBeInTheDocument();
    expect(screen.getByText(/min 4 NS/)).toBeInTheDocument();
  });
});
