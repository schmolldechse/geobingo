using System;
using System.Collections.Generic;
using GeoBingo.Contracts.Common;

namespace GeoBingo.GameModes.Abstractions;

public sealed class GameModeDomainException : Exception
{
    public GameModeDomainException(
        ErrorCode code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public ErrorCode Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
