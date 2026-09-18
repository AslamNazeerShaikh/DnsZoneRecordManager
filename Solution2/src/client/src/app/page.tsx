"use client";

/** Dashboard: hero, solution cards, live zone/record counts, and rule chips. */

import Link from "next/link";
import { useEffect, useState } from "react";
import { ArrowRight, Database, Globe } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { recordsApi, zonesApi } from "@/lib/api";

export default function DashboardPage() {
  const [zoneCount, setZoneCount] = useState<number | null>(null);
  const [recordCount, setRecordCount] = useState<number | null>(null);

  useEffect(() => {
    let live = true;
    zonesApi
      .list()
      .then((zones) => {
        if (!live) return;
        setZoneCount(zones.length);
      })
      .catch(() => {
        if (live) setZoneCount(null);
      });
    recordsApi
      .list()
      .then((records) => {
        if (!live) return;
        setRecordCount(records.length);
      })
      .catch(() => {
        if (live) setRecordCount(null);
      });
    return () => {
      live = false;
    };
  }, []);

  return (
    <div className="flex flex-col gap-6 py-6">
      <Card className="border-primary/20 bg-gradient-to-br from-card to-accent">
        <CardHeader>
          <CardTitle className="text-2xl">DNS Zones &amp; Records</CardTitle>
          <CardDescription>
            Simple DNS management for everyone — query, add, edit, and delete
            zones and records with guided validation.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap items-center gap-2">
          <Link href="/zones">
            <Button>
              Manage zones <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
          <Link href="/records">
            <Button variant="outline">Browse records</Button>
          </Link>
          {zoneCount !== null && (
            <Badge variant="secondary" className="ml-2">
              <Globe className="h-3 w-3" /> {zoneCount} zones
            </Badge>
          )}
          {recordCount !== null && (
            <Badge variant="secondary">
              <Database className="h-3 w-3" /> {recordCount} records
            </Badge>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Zones</CardTitle>
            <CardDescription>
              Create and rename zones, search by name, delete with a cascade
              preview.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/zones">
              <Button variant="outline" size="sm">
                Open zones
              </Button>
            </Link>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Records</CardTitle>
            <CardDescription>
              Filter by zone, search, filter by type, and export the grid as
              CSV.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/records">
              <Button variant="outline" size="sm">
                Open records
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>

      <div className="flex flex-wrap gap-2">
        <Badge className="bg-ok-bg text-ok hover:bg-ok-bg">min 4 NS</Badge>
        <Badge className="bg-warn-bg text-warn hover:bg-warn-bg">
          max 10 records
        </Badge>
        <Badge className="bg-info-bg text-info hover:bg-info-bg">
          A · AAAA · CNAME · NS · TXT
        </Badge>
        <Badge variant="secondary">no duplicates</Badge>
      </div>
    </div>
  );
}
