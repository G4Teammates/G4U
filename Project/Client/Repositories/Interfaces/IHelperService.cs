using CloudinaryDotNet;

namespace Client.Repositories.Interfaces
{
	public interface IHelperService
	{
		public Task<string> UploadImageAsync(Stream imageStream, string fileName);
	}
}
