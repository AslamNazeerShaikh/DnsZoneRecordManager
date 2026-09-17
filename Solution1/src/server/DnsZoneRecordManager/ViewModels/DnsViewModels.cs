using System.ComponentModel.DataAnnotations;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DnsZoneRecordManager.ViewModels
{
    /// <summary>Zone list page: search box plus zone rows with the A1/A2 meter.</summary>
    public class ZoneIndexViewModel
    {
        /// <summary>Current name filter.</summary>
        public string? Search { get; set; }

        /// <summary>Zone rows ordered by name.</summary>
        public List<ZoneListItem> Zones { get; set; } = [];
    }

    /// <summary>Zone create/rename form (DataAnnotations mirror the server rules for instant feedback).</summary>
    public class ZoneFormViewModel
    {
        /// <summary>Zone id (null on create).</summary>
        public int? Id { get; set; }

        /// <summary>Zone name.</summary>
        [Required(ErrorMessage = "Give the zone a name, e.g. nahuexolab.com.")]
        [StringLength(253, ErrorMessage = "Zone names cannot exceed 253 characters.")]
        [RegularExpression(
            DnsRules.HostnamePattern,
            ErrorMessage = "Use letters, digits, hyphens and dots only (no leading or trailing hyphen/dot)."
        )]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Zone delete confirmation: counts show what cascading destroys.</summary>
    public class ZoneDeleteViewModel
    {
        /// <summary>Zone id.</summary>
        public int Id { get; set; }

        /// <summary>Zone name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Total records (cascade-deleted).</summary>
        public int RecordCount { get; set; }

        /// <summary>NS records.</summary>
        public int NsCount { get; set; }
    }

    /// <summary>Record grid page: zone picker, search, type filter, rows, and meter.</summary>
    public class RecordIndexViewModel
    {
        /// <summary>Selected zone id (null = all zones).</summary>
        public int? ZoneId { get; set; }

        /// <summary>Current name/data filter.</summary>
        public string? Search { get; set; }

        /// <summary>Current type filter.</summary>
        public RecordType? Type { get; set; }

        /// <summary>Zone picker options.</summary>
        public List<SelectListItem> ZoneOptions { get; set; } = [];

        /// <summary>Type filter options.</summary>
        public List<SelectListItem> TypeOptions { get; set; } = [];

        /// <summary>Filtered rows.</summary>
        public List<RecordRow> Records { get; set; } = [];

        /// <summary>Selected zone name (null = all zones).</summary>
        public string? ZoneName { get; set; }

        /// <summary>Total records in the selected zone.</summary>
        public int RecordCount { get; set; }

        /// <summary>NS records in the selected zone.</summary>
        public int NsCount { get; set; }
    }

    /// <summary>Record create/edit form (DataAnnotations mirror the server rules for instant feedback).</summary>
    public class RecordFormViewModel
    {
        /// <summary>Owning zone id.</summary>
        public int ZoneId { get; set; }

        /// <summary>Owning zone name (display only).</summary>
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>Record id (null on create).</summary>
        public int? Id { get; set; }

        /// <summary>Owner name (@ = apex).</summary>
        [Required(ErrorMessage = "Give the record a name (@ for the zone apex).")]
        [StringLength(63, ErrorMessage = "Record names cannot exceed 63 characters.")]
        [RegularExpression(
            DnsRules.RecordNamePattern,
            ErrorMessage = "Use @, letters, digits, hyphens, dots (leading _ allowed, e.g. _dmarc)."
        )]
        public string Name { get; set; } = string.Empty;

        /// <summary>Record type.</summary>
        [Required(ErrorMessage = "Pick a record type.")]
        public RecordType? Type { get; set; }

        /// <summary>TTL in seconds.</summary>
        [Required(ErrorMessage = "TTL is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TTL must be a positive number of seconds.")]
        public int? Ttl { get; set; }

        /// <summary>RDATA value.</summary>
        [Required(ErrorMessage = "Record data cannot be empty.")]
        [StringLength(1000, ErrorMessage = "Record data cannot exceed 1000 characters.")]
        public string Data { get; set; } = string.Empty;

        /// <summary>Zone picker options (create page).</summary>
        public List<SelectListItem> ZoneOptions { get; set; } = [];
    }

    /// <summary>Record delete confirmation.</summary>
    public class RecordDeleteViewModel
    {
        /// <summary>Record id.</summary>
        public int Id { get; set; }

        /// <summary>Owning zone id (return target).</summary>
        public int ZoneId { get; set; }

        /// <summary>Owning zone name.</summary>
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>Owner name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Derived FQDN.</summary>
        public string Fqdn { get; set; } = string.Empty;

        /// <summary>Record type.</summary>
        public RecordType Type { get; set; }

        /// <summary>TTL in seconds.</summary>
        public int Ttl { get; set; }

        /// <summary>RDATA value.</summary>
        public string Data { get; set; } = string.Empty;
    }
}
