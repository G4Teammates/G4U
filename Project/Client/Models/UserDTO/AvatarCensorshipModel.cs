using Client.Models.ProductDTO;

namespace Client.Models.UserDTO
{
    public class AvatarCensorshipModel
    {
        public Stream AvatarFile { get; set; }
        public CensorshipModel Censorship { get; set; }
    }
}
