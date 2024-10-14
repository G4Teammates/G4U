using Client.Repositories.Interfaces;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace Client.Repositories.Services
{
	public class HelperService : IHelperService
	{
		private readonly Cloudinary _cloudinary = new Cloudinary(
			new Account(
			cloud: "dlhkn3owt",
			apiKey: "847543842481479",
			apiSecret: "A423SkMxBPtdjhgBVSrEdBghpBc"));


		public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
		{
			ImageUploadResult result = new();
			try
			{
				var UploadParam = new ImageUploadParams
				{
					File = new FileDescription(fileName, imageStream),
					Overwrite = true
				};
				result = await _cloudinary.UploadAsync(UploadParam);
				return result?.SecureUrl.AbsoluteUri ?? "Update fail, please try again";
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
	}
}
