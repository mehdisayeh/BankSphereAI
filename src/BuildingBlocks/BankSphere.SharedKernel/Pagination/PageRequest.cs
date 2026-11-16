
namespace BankSphere.SharedKernel;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 25, string Sort = "date", bool Descending = true)
{
    public void Validate()
    {
        Guard.Against(PageNumber < 1 || PageNumber > 100000 || PageSize < 1 || PageSize > 100, "pagination.invalid", "Page must be 1..100000; page size must be 1..100.");
        Guard.Against(Sort is not ("date" or "amount"), "pagination.sort", "Sort must be date or amount.");
    }
}
