using DockerDemoApi.Domain.Attributes;

using System.ComponentModel.DataAnnotations;

namespace DockerDemoApi.Domain
{
	public class AuthRequest
	{
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Password(8, 50)]
        public string Password { get; set; }
    }
}
