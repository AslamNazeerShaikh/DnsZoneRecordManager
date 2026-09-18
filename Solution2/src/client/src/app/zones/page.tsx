"use client";

/**
 * Zones page: searchable grid with per-zone record/NS meters, footer totals,
 * and create/rename/delete dialogs. Grids refresh on Apply and after
 * mutations; server messages surface via inline alerts and sonner toasts.
 */

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Progress } from "@/components/ui/progress";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { apiErrorMessages, zonesApi, type ZoneDto } from "@/lib/api";
import { formatUtc } from "@/lib/format";

function ZoneDialog({
  title,
  description,
  open,
  onOpenChange,
  initialName,
  submitLabel,
  onSubmit,
}: {
  title: string;
  description: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialName: string;
  submitLabel: string;
  onSubmit: (name: string) => Promise<void>;
}) {
  const [name, setName] = useState(initialName);
  const [errors, setErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    try {
      await onSubmit(name);
      onOpenChange(false);
    } catch (error) {
      setErrors(apiErrorMessages(error));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {errors.length > 0 && (
            <Alert variant="destructive">
              <AlertDescription>{errors.join(" ")}</AlertDescription>
            </Alert>
          )}
          <div className="flex flex-col gap-2">
            <Label htmlFor="zone-name">Zone name</Label>
            <Input
              id="zone-name"
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="nahuexolab.com"
              autoComplete="off"
            />
            <p className="text-sm text-muted-foreground">
              Letters, digits, hyphens and dots; ≤253 chars; lowercased
              automatically.
            </p>
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? "Saving…" : submitLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default function ZonesPage() {
  const [zones, setZones] = useState<ZoneDto[]>([]);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState<string | undefined>();
  const [loading, setLoading] = useState(true);
  const [createOpen, setCreateOpen] = useState(false);
  const [createNonce, setCreateNonce] = useState(0);
  const [editZone, setEditZone] = useState<ZoneDto | null>(null);
  const [deleteZone, setDeleteZone] = useState<ZoneDto | null>(null);

  const refresh = useCallback(async (term?: string) => {
    setLoading(true);
    try {
      setZones(await zonesApi.list(term));
    } catch (error) {
      toast.error(apiErrorMessages(error).join(" "));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect -- filter-change fetch; mutations refresh explicitly
    refresh(appliedSearch);
  }, [refresh, appliedSearch]);

  async function handleCreate(name: string) {
    const zone = await zonesApi.create(name);
    toast.success(`Zone '${zone.name}' was created.`);
    refresh(appliedSearch);
  }

  async function handleRename(name: string) {
    // Dialog submits only while open, so editZone is set here.
    const zone = await zonesApi.rename(editZone!.id, name);
    toast.success(`Zone renamed to '${zone.name}'.`);
    refresh(appliedSearch);
  }

  async function handleDelete() {
    // Dialog submits only while open, so deleteZone is set here.
    const id = deleteZone!.id;
    try {
      await zonesApi.remove(id);
      toast.success("Zone and its records were deleted.");
      setDeleteZone(null);
      refresh(appliedSearch);
    } catch (error) {
      toast.error(apiErrorMessages(error).join(" "));
    }
  }

  return (
    <div className="flex flex-col gap-4 py-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h1 className="text-2xl font-semibold tracking-tight">
          Zones{" "}
          <Badge variant="secondary" className="ml-1 align-middle">
            {zones.length}
          </Badge>
        </h1>
        <Button
          onClick={() => {
            setCreateNonce((n) => n + 1);
            setCreateOpen(true);
          }}
        >
          New zone
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Search zones</CardTitle>
          <CardDescription>Filter by zone name.</CardDescription>
        </CardHeader>
        <CardContent>
          <form
            className="flex flex-col gap-2 sm:flex-row"
            onSubmit={(event) => {
              event.preventDefault();
              setAppliedSearch(search || undefined);
            }}
          >
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="e.g. nahuexolab.com"
              aria-label="Search zones"
            />
            <div className="flex gap-2">
              <Button type="submit" variant="outline">
                Search
              </Button>
              <Button
                type="button"
                variant="ghost"
                onClick={() => {
                  setSearch("");
                  setAppliedSearch(undefined);
                }}
              >
                Clear
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Records (n / 10)</TableHead>
                <TableHead>NS</TableHead>
                <TableHead>Updated (UTC)</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center">
                    Loading…
                  </TableCell>
                </TableRow>
              ) : zones.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={5}
                    className="py-10 text-center text-muted-foreground"
                  >
                    No zones found. Create your first zone to get started.
                  </TableCell>
                </TableRow>
              ) : (
                zones.map((zone) => (
                  <TableRow key={zone.id}>
                    <TableCell className="font-medium">{zone.name}</TableCell>
                    <TableCell>
                      <div className="flex min-w-40 items-center gap-2">
                        <Progress
                          value={zone.recordCount * 10}
                          className="h-2 flex-1"
                          aria-label="Record count"
                        />
                        <span className="text-sm text-muted-foreground">
                          {zone.recordCount} / 10
                        </span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{zone.nsCount} NS</Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatUtc(zone.updatedUtc)}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => setEditZone(zone)}
                        >
                          Rename
                        </Button>
                        <Button
                          size="sm"
                          variant="destructive"
                          onClick={() => setDeleteZone(zone)}
                        >
                          Delete
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
            <TableFooter>
              <TableRow>
                <TableCell colSpan={5} className="text-muted-foreground">
                  Total: <strong>{zones.length}</strong> zones
                </TableCell>
              </TableRow>
            </TableFooter>
          </Table>
        </div>
      </Card>

      <ZoneDialog
        key={`zone-create-${createNonce}`}
        title="New zone"
        description="Lowercased automatically. Every zone keeps at least 4 NS records."
        open={createOpen}
        onOpenChange={setCreateOpen}
        initialName=""
        submitLabel="Save"
        onSubmit={handleCreate}
      />
      <ZoneDialog
        key={editZone ? `zone-edit-${editZone.id}` : "zone-edit"}
        title="Rename zone"
        description="Lowercased automatically; names stay unique."
        open={editZone !== null}
        onOpenChange={() => setEditZone(null)}
        initialName={editZone?.name ?? ""}
        submitLabel="Save"
        onSubmit={handleRename}
      />

      <Dialog
        open={deleteZone !== null}
        onOpenChange={() => setDeleteZone(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete zone</DialogTitle>
            <DialogDescription>
              Delete <strong>{deleteZone?.name}</strong> and all of its records?
              This cannot be undone.
            </DialogDescription>
          </DialogHeader>
          {deleteZone && (
            <div className="flex gap-2">
              <Badge variant="secondary">
                {deleteZone.recordCount} records
              </Badge>
              <Badge variant="secondary">{deleteZone.nsCount} NS</Badge>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteZone(null)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Delete zone
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
