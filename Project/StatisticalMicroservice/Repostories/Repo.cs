using AutoMapper;
using CloudinaryDotNet;
using StatisticalMicroservice.DBContexts;
using StatisticalMicroservice.DBContexts.Entities;

namespace StatisticalMicroservice.Repostories
{
    public class Repo : IRepo
    {
        #region declaration and initialization
        private readonly StatisticalDbcontext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;


        public Repo(IConfiguration configuration, StatisticalDbcontext db, IMapper mapper)
        {
            _configuration = configuration;
            _db = db;
            _mapper = mapper;
        }
        #endregion
        public IEnumerable<Statistical> Statisticals => _db.Statistical.ToList();
    }
}
