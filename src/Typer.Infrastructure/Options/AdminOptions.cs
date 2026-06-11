namespace Typer.Infrastructure.Options;

public class AdminOptions
{
    public const string SectionName = "Admin";

    public string[] AdminEmails { get; set; } = [];
}
