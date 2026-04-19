namespace Transport;

/// <summary>
/// Represents the configured note retention policy.
/// </summary>
/// <param name="DaysToKeep">How many days notes should be kept. A negative value means unlimited retention.</param>
public sealed record RetentionPolicyResponse(int DaysToKeep);
