namespace DelegateQueryable
{
    public interface QueryInfo
    {
        int? Take { get; }
        int Skip { get; }
    }
}