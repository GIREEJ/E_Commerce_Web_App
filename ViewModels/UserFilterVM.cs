using ECommerceWebApp.Models;

public class UserFilterVM
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortField { get; set; }
    public string SortOrder { get; set; } // "asc" or "desc"
    public IEnumerable<User>? Users { get; set; }
    public int TotalPages { get; set; }
}
