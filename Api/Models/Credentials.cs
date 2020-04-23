using System.ComponentModel.DataAnnotations;

namespace Api.Models {
    public class Credentials {
        [Required (AllowEmptyStrings = false)]
        public string User { get; set; }

        [Required (AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}