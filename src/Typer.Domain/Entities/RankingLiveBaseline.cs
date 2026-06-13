using Typer.Domain.Common;

namespace Typer.Domain.Entities;

/// <summary>
/// Snapshot punktów rankingu z momentu rozpoczęcia bieżącej sesji meczów na żywo.
/// </summary>
public class RankingLiveBaseline : BaseEntity
{
    public bool VipOnly { get; set; }
    public bool IsActive { get; set; }
    public string PointsJson { get; set; } = "{}";
    public DateTime CapturedAtUtc { get; set; }
}
