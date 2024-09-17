using UserMicroservice.Models;
using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.Utility
{
    public class SearchCriteriaBuilder
    {
        private readonly SearchCriteria _searchCriteria = new SearchCriteria();

        public SearchCriteriaBuilder SetDisplay(string? display)
        {
            _searchCriteria.DisplayName = display;
            return this;
        }

        public SearchCriteriaBuilder SetStatus(UserStatus status)
        {
            _searchCriteria.Status = status;
            return this;
        }

        public SearchCriteriaBuilder SetEmail(string? email)
        {
            _searchCriteria.Email = email?.ToUpper();
            return this;
        }

        public SearchCriteriaBuilder SetPhoneNumber(string? phoneNumber)
        {
            _searchCriteria.PhoneNumber = phoneNumber;
            return this;
        }

        public SearchCriteriaBuilder SetUsername(string? username)
        {
            _searchCriteria.Username = username?.ToUpper();
            return this;
        }

        public SearchCriteria Build()
        {
            return _searchCriteria;
        }
    }

}
