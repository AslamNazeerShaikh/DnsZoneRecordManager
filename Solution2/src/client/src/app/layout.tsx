import type { Metadata } from "next";
import type { ReactNode } from "react";
import { Inter } from "next/font/google";
import "./globals.css";
import { ThemeProvider } from "@/components/theme-provider";
import { SiteHeader } from "@/components/site-header";
import { Toaster } from "@/components/ui/sonner";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "DNS Manager",
  description: "DNS Zones + Records manager (Solution2: Web API + Next.js)",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning className="h-full">
      <body className={`${inter.variable} min-h-full font-sans antialiased`}>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <SiteHeader />
          <main className="mx-auto w-full max-w-6xl flex-1 px-4 pb-10">
            {children}
          </main>
          <footer className="border-t py-4 text-center text-sm text-muted-foreground">
            DNS Manager (Solution2 · Web API + Next.js) — min 4 NS · max 10
            records · A/AAAA/CNAME/NS/TXT
          </footer>
          <Toaster richColors position="bottom-right" />
        </ThemeProvider>
      </body>
    </html>
  );
}
