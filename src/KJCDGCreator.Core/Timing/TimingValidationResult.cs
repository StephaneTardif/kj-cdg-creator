namespace KJCDGCreator.Core.Timing;

public sealed record TimingValidationResult(bool IsValid, IReadOnlyList<string> Errors);
