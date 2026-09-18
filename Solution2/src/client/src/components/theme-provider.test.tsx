import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ThemeProvider } from "@/components/theme-provider";

describe("ThemeProvider", () => {
  it("renders children", () => {
    render(
      <ThemeProvider attribute="class">
        <p>child content</p>
      </ThemeProvider>,
    );
    expect(screen.getByText("child content")).toBeInTheDocument();
  });
});
