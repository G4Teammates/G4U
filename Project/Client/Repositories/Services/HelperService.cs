using Client.Repositories.Interfaces;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Client.Models;

namespace Client.Repositories.Services
{
	public class HelperService : IHelperService
	{
		private readonly Cloudinary _cloudinary = new Cloudinary(
			new Account(
			cloud: ConfigKeyModel.CloudinaryName,
			apiKey: ConfigKeyModel.CloudinaryKey,
			apiSecret: ConfigKeyModel.CloudinarySecret));


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
