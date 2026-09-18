using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Validation;
using FluentValidation;

namespace DnsZoneRecordManager.Validation
{
    /// <summary>Shape validation for <see cref="CreateZoneCommand"/> (uniqueness is enforced by its handler).</summary>
    public class CreateZoneValidator : AbstractValidator<CreateZoneCommand>
    {
        /// <summary>Creates the zone shape rules (valid FQDN, ≤253 chars).</summary>
        public CreateZoneValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Give the zone a name, e.g. nahuexolab.com.")
                .MaximumLength(253)
                .WithMessage("Zone names cannot exceed 253 characters.")
                .Matches(DnsRules.HostnamePattern)
                .WithMessage("Use letters, digits, hyphens and dots only (no leading or trailing hyphen/dot).");
        }
    }

    /// <summary>Shape validation for <see cref="RenameZoneCommand"/>.</summary>
    public class RenameZoneValidator : AbstractValidator<RenameZoneCommand>
    {
        /// <summary>Creates the rename shape rules (same as create; existence is enforced by its handler).</summary>
        public RenameZoneValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Give the zone a name, e.g. nahuexolab.com.")
                .MaximumLength(253)
                .WithMessage("Zone names cannot exceed 253 characters.")
                .Matches(DnsRules.HostnamePattern)
                .WithMessage("Use letters, digits, hyphens and dots only (no leading or trailing hyphen/dot).");
        }
    }

    /// <summary>Shape validation for <see cref="CreateRecordCommand"/> (limits and cross-record rules live in its handler).</summary>
    public class CreateRecordValidator : AbstractValidator<CreateRecordCommand>
    {
        /// <summary>Creates the record shape rules (name, known type, positive TTL, type-fitting data).</summary>
        public CreateRecordValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Give the record a name (@ for the zone apex).")
                .MaximumLength(63)
                .WithMessage("Record names cannot exceed 63 characters.")
                .Matches(DnsRules.RecordNamePattern)
                .WithMessage("Use @, letters, digits, hyphens, dots (leading _ allowed, e.g. _dmarc).");
            RuleFor(c => c.Type).IsInEnum().WithMessage("Type must be A, AAAA, CNAME, NS or TXT.");
            RuleFor(c => c.Ttl).GreaterThan(0).WithMessage("TTL must be a positive number of seconds.");
            RuleFor(c => c.Data)
                .NotEmpty()
                .WithMessage("Record data cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Record data cannot exceed 1000 characters.")
                .Must((command, data) => RecordDataValidators.IsValid(command.Type, data))
                .WithMessage("Data does not fit the record type (A = IPv4, AAAA = IPv6, CNAME/NS = hostname).");
        }
    }

    /// <summary>Shape validation for <see cref="UpdateRecordCommand"/>.</summary>
    public class UpdateRecordValidator : AbstractValidator<UpdateRecordCommand>
    {
        /// <summary>Creates the update shape rules (same as create; existence is enforced by its handler).</summary>
        public UpdateRecordValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Give the record a name (@ for the zone apex).")
                .MaximumLength(63)
                .WithMessage("Record names cannot exceed 63 characters.")
                .Matches(DnsRules.RecordNamePattern)
                .WithMessage("Use @, letters, digits, hyphens, dots (leading _ allowed, e.g. _dmarc).");
            RuleFor(c => c.Type).IsInEnum().WithMessage("Type must be A, AAAA, CNAME, NS or TXT.");
            RuleFor(c => c.Ttl).GreaterThan(0).WithMessage("TTL must be a positive number of seconds.");
            RuleFor(c => c.Data)
                .NotEmpty()
                .WithMessage("Record data cannot be empty.")
                .MaximumLength(1000)
                .WithMessage("Record data cannot exceed 1000 characters.")
                .Must((command, data) => RecordDataValidators.IsValid(command.Type, data))
                .WithMessage("Data does not fit the record type (A = IPv4, AAAA = IPv6, CNAME/NS = hostname).");
        }
    }
}
