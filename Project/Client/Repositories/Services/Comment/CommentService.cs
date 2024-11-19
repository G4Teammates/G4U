using Client.Models;
using Client.Models.ComentDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Comment;
using Client.Utility;

namespace Client.Repositories.Services.Comment
{
    public class CommentService : ICommentService
    {

        public readonly IBaseService _baseService;
        public CommentService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseModel> CreateCommentAsync(CreateCommentDTOModel Comment, string userId)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(Comment.Content), "content");
            formData.Add(new StringContent(Comment.Status.ToString()), "status");
            formData.Add(new StringContent(Comment.UserName), "userName");
            formData.Add(new StringContent(Comment.ProductId), "productId");

            // Chỉ thêm ParentId nếu nó không phải là null
            if (!string.IsNullOrEmpty(Comment.ParentId))
            {
                formData.Add(new StringContent(Comment.ParentId), "parentId");
            }

            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Comment/"+ userId
            });
        }


        public async Task<ResponseModel> DeleteCommentAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Comment/" + id
            });
        }

        public async Task<ResponseModel> GetAllCommentAsync(int pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Comment?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> GetByIdAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Comment/" + id
            });
        }

        public async Task<ResponseModel> GetByproductId(string productId, int page, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/get={productId}?page=" + page.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> GetListByIdAsync(string id, int page, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/List/{id}?page=" + page.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel?> SearchCmtAsync(string searchString, int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/search={searchString}?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> UpdateCommentAsync(CommentDTOModel Comment)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(Comment.Id), "id");
            formData.Add(new StringContent(Comment.Content), "content");
            formData.Add(new StringContent(Comment.NumberOfLikes.ToString()), "numberoflikes");
            formData.Add(new StringContent(Comment.Status.ToString()), "status");
            formData.Add(new StringContent(Comment.UserName), "userName");
            formData.Add(new StringContent(Comment.ProductId), "productId");
            formData.Add(new StringContent(Comment.CreatedAt.ToString()), "createat");
            formData.Add(new StringContent(Comment.UpdatedAt.ToString()), "updateat");
            formData.Add(new StringContent(Comment.NumberOfDisLikes.ToString()), "numberofdislikes");

            // Chỉ thêm ParentId nếu nó không phải là null
            if (!string.IsNullOrEmpty(Comment.ParentId))
            {
                formData.Add(new StringContent(Comment.ParentId), "parentId");
            }

            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Comment"
            });
        }
        public async Task<ResponseModel> GetByParentIdAsync(string parentId, int? pageNumber, int pageSize)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/ParentId/{parentId}?page=" + pageNumber.ToString() + "&pageSize=" + pageSize.ToString()
            });
        }

        public async Task<ResponseModel> IncreaseLike(string commentId, UserLikesModel userLikes)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = userLikes,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/IncreaseLike/{commentId}"
            });
        }

        public async Task<ResponseModel> DecreaseLike(string commentId, UserDisLikesModel userDisLikes)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = userDisLikes,
                Url = $"{StaticTypeApi.APIGateWay}/Comment/DecreaseLike/{commentId}"
            });
        }
    }
}
