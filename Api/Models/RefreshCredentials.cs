using System.ComponentModel.DataAnnotations;

namespace Api.Models {
    public class RefreshCredentials {
        [Required (AllowEmptyStrings = false)]
        public string UserId { get; set; }

        [Required (AllowEmptyStrings = false)]
        public string RefreshToken { get; set; }
    }
}