namespace DnsZoneRecordManager.Results
{
    /// <summary>Machine-readable failure kinds. Controllers map these to responses deterministically
    /// (NotFound → 404, Conflict → 409, else 400); no string matching on messages.</summary>
    public enum ErrorKind
    {
        /// <summary>Shape validation failed (FluentValidation message).</summary>
        Validation = 0,

        /// <summary>Requested zone or record does not exist.</summary>
        NotFound = 1,

        /// <summary>Duplicate zone or record (A6).</summary>
        Conflict = 2,

        /// <summary>Business rule breached: NS floor (A1), record ceiling (A2), or CNAME exclusivity.</summary>
        RuleViolation = 3,
    }

    /// <summary>One typed handler failure: a kind for control flow plus a guided message for the UI (A4/A5).</summary>
    /// <param name="Kind">Failure kind.</param>
    /// <param name="Message">Human-readable message.</param>
    public sealed record ServiceError(ErrorKind Kind, string Message);

    /// <summary>Outcome of a command/query: data on success, typed errors on failure.
    /// Handlers never throw for domain failures; unexpected exceptions propagate to the
    /// production exception handler (ProblemDetails, no internals leaked).</summary>
    /// <typeparam name="T">Payload type.</typeparam>
    public sealed class ServiceResult<T>
    {
        private ServiceResult(T? data, List<ServiceError> errors)
        {
            Data = data;
            Errors = errors;
        }

        /// <summary>True when <see cref="Errors"/> is empty.</summary>
        public bool Success => Errors.Count == 0;

        /// <summary>Payload on success; default on failure.</summary>
        public T? Data { get; }

        /// <summary>Typed failures (empty on success).</summary>
        public List<ServiceError> Errors { get; }

        /// <summary>True when any failure has the given kind.</summary>
        /// <param name="kind">Kind to look for.</param>
        /// <returns>True when present.</returns>
        public bool HasError(ErrorKind kind)
        {
            return Errors.Any(e => e.Kind == kind);
        }

        /// <summary>Creates a successful result.</summary>
        /// <param name="data">Payload.</param>
        /// <returns>Successful result.</returns>
        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>(data, []);
        }

        /// <summary>Creates a failed result with one error.</summary>
        /// <param name="kind">Failure kind.</param>
        /// <param name="message">Guided message.</param>
        /// <returns>Failed result.</returns>
        public static ServiceResult<T> Fail(ErrorKind kind, string message)
        {
            return new ServiceResult<T>(default, [new ServiceError(kind, message)]);
        }

        /// <summary>Creates a failed result with one error per message (e.g. validator output).</summary>
        /// <param name="kind">Failure kind for every message.</param>
        /// <param name="messages">Guided messages.</param>
        /// <returns>Failed result.</returns>
        public static ServiceResult<T> Fail(ErrorKind kind, IEnumerable<string> messages)
        {
            return new ServiceResult<T>(default, messages.Select(m => new ServiceError(kind, m)).ToList());
        }

        /// <summary>Creates a failed result from already-typed errors (e.g. guard output).</summary>
        /// <param name="errors">Typed errors.</param>
        /// <returns>Failed result.</returns>
        public static ServiceResult<T> Fail(IEnumerable<ServiceError> errors)
        {
            return new ServiceResult<T>(default, new List<ServiceError>(errors));
        }
    }
}
