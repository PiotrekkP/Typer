namespace Typer.Infrastructure.Options;

public class VipOptions
{
    public const string SectionName = "Vip";

    /// <summary>E-maile użytkowników z flagą VIP (UserProfile.VipUser + rola Vip).</summary>
    public string[] VipEmails { get; set; } = [];
}
