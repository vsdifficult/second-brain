
namespace SecondBrain.BuildingBlocks.Core.Contracts.Pagination;  

public class PagedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<T> Items { get; set; }
}