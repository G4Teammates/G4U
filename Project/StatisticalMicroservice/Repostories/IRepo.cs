using StatisticalMicroservice.DBContexts.Entities;

namespace StatisticalMicroservice.Repostories
{
    public interface IRepo
    {
        IEnumerable<Statistical> Statisticals { get; }
    }
}
