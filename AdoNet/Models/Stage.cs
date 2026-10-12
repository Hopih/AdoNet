namespace AdoNet;

public class Stage
{
    public Guid Id { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public string Title { get; set; }
    
    public int Cost { get; set; }
    
    public string Status { get; set; } 
}