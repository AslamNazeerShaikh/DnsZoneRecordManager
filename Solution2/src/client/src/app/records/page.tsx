"use client";

/**
 * Records page: zone picker + text search + type filter (Apply-only refresh),
 * zone meter card, type badges, full-width wrapped data cells, CSV export of
 * the active filter, and create/edit/delete dialogs. Deleting resets to the
 * unfiltered grid; dialog drafts persist across reopens by design.
 */

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  apiErrorMessages,
  recordsApi,
  zonesApi,
  type RecordDto,
  type RecordType,
  type ZoneDto,
} from "@/lib/api";
import { formatUtc } from "@/lib/format";
import { cn } from "@/lib/utils";

const TYPES: RecordType[] = ["A", "AAAA", "CNAME", "NS", "TXT"];

/** Select value meaning "no filter" (shadcn SelectItem needs a non-empty value). */
const ALL = "__all";

/** Maps a select value back to a filter (null-safe for the select contract). */
export function fromSelect(value: string | null): string {
  return value === null || value === ALL ? "" : value;
}

const TYPE_STYLES: Record<string, string> = {
  A: "bg-ok-bg text-ok hover:bg-ok-bg",
  AAAA: "bg-info-bg text-info hover:bg-info-bg",
  CNAME: "bg-warn-bg text-warn hover:bg-warn-bg",
  NS: "bg-accent text-accent-foreground hover:bg-accent",
  TXT: "bg-secondary text-secondary-foreground hover:bg-secondary",
};

interface RecordForm {
  name: string;
  type: RecordType | "";
  ttl: string;
  data: string;
}

const EMPTY_FORM: RecordForm = { name: "", type: "", ttl: "", data: "" };

interface RecordPayload {
  zoneId: number;
  name: string;
  type: RecordType;
  ttl: string;
  data: string;
}

function RecordDialog({
  title,
  description,
  open,
  onOpenChange,
  zones,
  initialZoneId,
  lockZone,
  zoneName,
  initial,
  submitLabel,
  onSubmit,
}: {
  title: string;
  description: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  zones: ZoneDto[];
  initialZoneId: number | undefined;
  lockZone: boolean;
  zoneName: string;
  initial: RecordForm;
  submitLabel: string;
  onSubmit: (form: RecordPayload) => Promise<void>;
}) {
  const [zoneId, setZoneId] = useState<number | undefined>(initialZoneId);
  const [form, setForm] = useState<RecordForm>(initial);
  const [errors, setErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  function set<Key extends keyof RecordForm>(key: Key, value: RecordForm[Key]) {
    setForm((previous) => ({ ...previous, [key]: value }));
  }

  const pickedZone = zones.find((zone) => zone.id === zoneId);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const { name, type, ttl, data } = form;
    if (zoneId === undefined) {
      setErrors(["Pick a zone first."]);
      return;
    }
    if (!type || !ttl) {
      setErrors(["Pick a record type and enter a TTL."]);
      return;
    }
    setSaving(true);
    try {
      await onSubmit({ zoneId, name, type, ttl, data });
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
          <DialogDescription>
            {description} {zoneName}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {errors.length > 0 && (
            <Alert variant="destructive">
              <AlertDescription>{errors.join(" ")}</AlertDescription>
            </Alert>
          )}
          <div className="flex flex-col gap-2">
            <Label htmlFor="record-zone">Zone</Label>
            {lockZone ? (
              <Input id="record-zone" value={zoneName} disabled />
            ) : (
              <Select
                value={zoneId?.toString() ?? ALL}
                onValueChange={(value) => {
                  setZoneId(Number(fromSelect(value)));
                }}
              >
                <SelectTrigger id="record-zone" className="w-full">
                  <span className={cn(!pickedZone && "text-muted-foreground")}>
                    {pickedZone ? pickedZone.name : "Pick a zone"}
                  </span>
                </SelectTrigger>
                <SelectContent>
                  {zones.map((zone) => (
                    <SelectItem key={zone.id} value={zone.id.toString()}>
                      {zone.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="record-name">Record name</Label>
            <Input
              id="record-name"
              value={form.name}
              onChange={(event) => set("name", event.target.value)}
              placeholder="@, www, _dmarc"
              autoComplete="off"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-2">
              <Label htmlFor="record-type">Type</Label>
              <Select
                value={form.type}
                onValueChange={(value) => set("type", value as RecordType)}
              >
                <SelectTrigger id="record-type" className="w-full">
                  <span className={cn(!form.type && "text-muted-foreground")}>
                    {form.type || "Pick…"}
                  </span>
                </SelectTrigger>
                <SelectContent>
                  {TYPES.map((type) => (
                    <SelectItem key={type} value={type}>
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex flex-col gap-2">
              <Label htmlFor="record-ttl">TTL (seconds)</Label>
              <Input
                id="record-ttl"
                value={form.ttl}
                onChange={(event) => set("ttl", event.target.value)}
                placeholder="3600"
                inputMode="numeric"
              />
            </div>
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="record-data">Data</Label>
            <Input
              id="record-data"
              value={form.data}
              onChange={(event) => set("data", event.target.value)}
              placeholder="1.2.3.4, hostname, or text"
              autoComplete="off"
            />
            <p className="text-sm text-muted-foreground">
              A = IPv4 · AAAA = IPv6 · CNAME/NS = hostname · TXT = text (≤1000
              chars). A CNAME name holds no other records.
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

interface Filters {
  zoneId?: number;
  search?: string;
  type?: RecordType;
}

export default function RecordsPage() {
  const [zones, setZones] = useState<ZoneDto[]>([]);
  const [records, setRecords] = useState<RecordDto[]>([]);
  const [draft, setDraft] = useState<{
    zone: string;
    search: string;
    type: string;
  }>({ zone: "", search: "", type: "" });
  const [filters, setFilters] = useState<Filters>({});
  const [loading, setLoading] = useState(true);
  const [createOpen, setCreateOpen] = useState(false);
  const [createNonce, setCreateNonce] = useState(0);
  const [editRecord, setEditRecord] = useState<RecordDto | null>(null);
  const [deleteRecord, setDeleteRecord] = useState<RecordDto | null>(null);

  const refresh = useCallback(async (active: Filters) => {
    setLoading(true);
    try {
      const [fetchedZones, fetchedRecords] = await Promise.all([
        zonesApi.list(),
        recordsApi.list(active.zoneId, active.search, active.type),
      ]);
      setZones(fetchedZones);
      setRecords(fetchedRecords);
    } catch (error) {
      toast.error(apiErrorMessages(error).join(" "));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect -- filter-change fetch; mutations refresh explicitly
    refresh(filters);
  }, [refresh, filters]);

  const selectedZone = zones.find(
    (zone) => filters.zoneId !== undefined && zone.id === filters.zoneId,
  );
  const draftZone = zones.find((zone) => zone.id.toString() === draft.zone);

  function applyFilters() {
    setFilters({
      zoneId: draft.zone ? Number(draft.zone) : undefined,
      search: draft.search || undefined,
      type: (draft.type as RecordType) || undefined,
    });
  }

  function clearFilters() {
    setDraft({ zone: "", search: "", type: "" });
    setFilters({});
  }

  async function handleCreate(form: RecordPayload) {
    await recordsApi.create({
      zoneId: form.zoneId,
      name: form.name,
      type: form.type,
      ttl: Number(form.ttl),
      data: form.data,
    });
    toast.success("Record was created.");
    refresh(filters);
  }

  async function handleEdit(form: {
    name: string;
    type: RecordType;
    ttl: string;
    data: string;
  }) {
    // Dialog submits only while open, so editRecord is set here.
    await recordsApi.update(editRecord!.id, {
      name: form.name,
      type: form.type,
      ttl: Number(form.ttl),
      data: form.data,
    });
    toast.success("Record was updated.");
    refresh(filters);
  }

  async function handleDelete() {
    // Dialog submits only while open, so deleteRecord is set here.
    const id = deleteRecord!.id;
    try {
      await recordsApi.remove(id);
      toast.success("Record was deleted.");
      setDeleteRecord(null);
      // Resetting filters refires the loader effect (single fetch).
      setDraft({ zone: "", search: "", type: "" });
      setFilters({});
    } catch (error) {
      toast.error(apiErrorMessages(error).join(" "));
    }
  }

  return (
    <div className="flex flex-col gap-4 py-6">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h1 className="text-2xl font-semibold tracking-tight">
          Records{" "}
          <Badge variant="secondary" className="ml-1 align-middle">
            {records.length} shown
          </Badge>
        </h1>
        <div className="flex gap-2">
          <a
            href={recordsApi.exportUrl(
              filters.zoneId,
              filters.search,
              filters.type,
            )}
            download
            className={buttonVariants({ variant: "outline" })}
          >
            Export CSV
          </a>
          <Button
            onClick={() => {
              setCreateNonce((n) => n + 1);
              setCreateOpen(true);
            }}
          >
            New record
          </Button>
        </div>
      </div>

      <Card>
        <CardContent className="flex flex-col gap-3 pt-6 md:flex-row md:items-end">
          <div className="flex flex-1 flex-col gap-2">
            <Label htmlFor="filter-zone">Zone</Label>
            <Select
              value={draft.zone || ALL}
              onValueChange={(value) =>
                setDraft((previous) => ({
                  ...previous,
                  zone: fromSelect(value),
                }))
              }
            >
              <SelectTrigger id="filter-zone" className="w-full">
                <span className={cn(!draftZone && "text-muted-foreground")}>
                  {draftZone ? draftZone.name : "All zones"}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={ALL}>All zones</SelectItem>
                {zones.map((zone) => (
                  <SelectItem key={zone.id} value={zone.id.toString()}>
                    {zone.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-1 flex-col gap-2">
            <Label htmlFor="filter-search">Search</Label>
            <Input
              id="filter-search"
              value={draft.search}
              onChange={(event) =>
                setDraft((previous) => ({
                  ...previous,
                  search: event.target.value,
                }))
              }
              placeholder="name or data…"
            />
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="filter-type">Type</Label>
            <Select
              value={draft.type || ALL}
              onValueChange={(value) =>
                setDraft((previous) => ({
                  ...previous,
                  type: fromSelect(value),
                }))
              }
            >
              <SelectTrigger id="filter-type" className="w-full md:w-36">
                <span className={cn(!draft.type && "text-muted-foreground")}>
                  {draft.type || "All types"}
                </span>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={ALL}>All types</SelectItem>
                {TYPES.map((type) => (
                  <SelectItem key={type} value={type}>
                    {type}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={applyFilters}>
              Apply
            </Button>
            <Button variant="ghost" onClick={clearFilters}>
              Clear
            </Button>
          </div>
        </CardContent>
      </Card>

      {selectedZone && (
        <Card>
          <CardContent className="flex flex-wrap items-center gap-3 pt-6">
            <strong>{selectedZone.name}</strong>
            <Progress
              value={selectedZone.recordCount * 10}
              className="h-2 min-w-32 flex-1"
              aria-label="Record count"
            />
            <Badge variant="secondary">
              {selectedZone.recordCount} / 10 records
            </Badge>
            <Badge variant="secondary">{selectedZone.nsCount} NS</Badge>
          </CardContent>
        </Card>
      )}

      <Card>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>FQDN</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>TTL</TableHead>
                <TableHead>Data</TableHead>
                <TableHead>Modified (UTC)</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center">
                    Loading…
                  </TableCell>
                </TableRow>
              ) : records.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={7}
                    className="py-10 text-center text-muted-foreground"
                  >
                    No records match. Adjust the filters or add a record.
                  </TableCell>
                </TableRow>
              ) : (
                records.map((record) => (
                  <TableRow key={record.id}>
                    <TableCell className="font-medium">{record.fqdn}</TableCell>
                    <TableCell>
                      <code className="text-sm">{record.name}</code>
                    </TableCell>
                    <TableCell>
                      <Badge
                        className={cn(
                          TYPE_STYLES[record.type],
                          "font-semibold",
                        )}
                      >
                        {record.type}
                      </Badge>
                    </TableCell>
                    <TableCell>{record.ttl}</TableCell>
                    <TableCell className="max-w-70 break-all whitespace-normal">
                      {record.data}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatUtc(record.updatedUtc)}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => setEditRecord(record)}
                        >
                          Edit
                        </Button>
                        <Button
                          size="sm"
                          variant="destructive"
                          onClick={() => setDeleteRecord(record)}
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
                <TableCell colSpan={7} className="text-muted-foreground">
                  Total: <strong>{records.length}</strong> records
                </TableCell>
              </TableRow>
            </TableFooter>
          </Table>
        </div>
      </Card>

      <RecordDialog
        key={`record-create-${createNonce}`}
        title="New record"
        description="Pick a zone, then the record details."
        open={createOpen}
        onOpenChange={setCreateOpen}
        zones={zones}
        initialZoneId={filters.zoneId}
        lockZone={false}
        zoneName=""
        initial={EMPTY_FORM}
        submitLabel="Save"
        onSubmit={handleCreate}
      />
      <RecordDialog
        key={editRecord ? `record-edit-${editRecord.id}` : "record-edit"}
        title="Edit record"
        description="in"
        open={editRecord !== null}
        onOpenChange={() => setEditRecord(null)}
        zones={[]}
        initialZoneId={editRecord?.zoneId}
        lockZone
        zoneName={editRecord?.zoneName ?? ""}
        initial={
          editRecord
            ? {
                name: editRecord.name,
                type: editRecord.type,
                ttl: editRecord.ttl.toString(),
                data: editRecord.data,
              }
            : EMPTY_FORM
        }
        submitLabel="Save"
        onSubmit={handleEdit}
      />

      <Dialog
        open={deleteRecord !== null}
        onOpenChange={() => setDeleteRecord(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete record</DialogTitle>
            <DialogDescription>
              Delete <strong>{deleteRecord?.fqdn}</strong>? This cannot be
              undone. Zones must always keep at least 4 NS records.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteRecord(null)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Delete record
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
