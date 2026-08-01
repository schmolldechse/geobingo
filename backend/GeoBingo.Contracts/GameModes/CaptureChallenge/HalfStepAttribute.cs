using System;
using System.ComponentModel.DataAnnotations;

namespace GeoBingo.Contracts.GameModes.CaptureChallenge;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal sealed class HalfStepAttribute : ValidationAttribute
{
    public HalfStepAttribute() : base("The value must use increments of 0.5.")
    {
    }

    public override bool IsValid(object? value) => value is null || value is decimal decimalValue && decimalValue % 0.5m == 0m;
}
