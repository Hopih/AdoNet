namespace AdoNet;

public class Project
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Address { get; set; }

    public DateOnly StartDate { get; set; }

    public decimal Budget { get; set; }

    // Заполняются только при чтении: имя владельца (JOIN users)
    // и сумма этапов (функция get_project_total_cost)
    public string? UserName { get; set; }

    public decimal TotalCost { get; set; }
}
