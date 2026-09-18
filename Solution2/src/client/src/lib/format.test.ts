import { describe, expect, it } from "vitest";
import { formatUtc } from "@/lib/format";

describe("formatUtc", () => {
  it("formats ISO timestamps as yyyy-MM-dd HH:mm in UTC", () => {
    expect(formatUtc("2024-05-24T20:06:01Z")).toBe("2024-05-24 20:06");
    expect(formatUtc("2024-01-02T03:04:05Z")).toBe("2024-01-02 03:04");
  });
});
