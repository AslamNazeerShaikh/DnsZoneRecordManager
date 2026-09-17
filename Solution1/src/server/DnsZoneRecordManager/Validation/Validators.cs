using DnsZoneRecordManager.Models;
using FluentValidation;

namespace DnsZoneRecordManager.Validation
{
    /// <summary>Shape validation for <see cref="DnsZone"/> (uniqueness is enforced by <see cref="Services.IZoneService"/>).</summary>
    public class ZoneValidator : AbstractValidator<DnsZone>
    {
        /// <summary>Creates the zone shape rules (valid FQDN, ≤253 chars).</summary>
        public ZoneValidator()
        {
            RuleFor(z => z.Name)
                .NotEmpty()
                .WithMessage("Give the zone a name, e.g. nahuexolab.com.")
                .MaximumLength(253)
                .WithMessage("Zone names cannot exceed 253 characters.")
                .Matches(DnsRules.HostnamePattern)
                .WithMessage(
                    "Use letters, digits, hyphens and dots only (no leading or trailing hyphen/dot)."
                );
        }
    }

    /// <summary>Shape validation for <see cref="DnsRecord"/> (limits and cross-record rules live in <see cref="Services.IRecordService"/>).</summary>
    public class RecordValidator : AbstractValidator<DnsRecord>
    {
        /// <summary>Creates the record shape rules (name, known type, positive TTL, type-fitting data).</summary>
        public RecordValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .WithMessage("Give the record a name (@ for the zone apex).")
                .MaximumLength(63)
                .WithMessage("Record names cannot exceed 63 characters.")
                .Matches(DnsRules.RecordNamePattern)
                .WithMessage(
                    "Use @, letters, digits, hyphens, dots (leading _ allowed, e.g. _dmarc)."
                );
            RuleFor(r => r.Type).IsInEnum().WithMessage("Type must be A, AAAA, CNAME, NS or TXT.");
            RuleFor(r => r.Ttl)
                .GreaterThan(0)
                .WithMessage("TTL must be a positive number of seconds.");
            RuleFor(r => r.Data)
                .NotEmpty()
                .WithMessage("Record data cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Record data cannot exceed 1000 characters.")
                .Must((record, data) => RecordDataValidators.IsValid(record.Type, data))
                .WithMessage(
                    "Data does not fit the record type (A = IPv4, AAAA = IPv6, CNAME/NS = hostname)."
                );
        }
    }
}
