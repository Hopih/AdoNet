namespace AdoNet;

public class Stage
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public required string Title { get; set; }

    public decimal Cost { get; set; }

    public required string Status { get; set; }

    // Заполняется только при чтении (JOIN projects)
    public string? ProjectAddress { get; set; }
}
