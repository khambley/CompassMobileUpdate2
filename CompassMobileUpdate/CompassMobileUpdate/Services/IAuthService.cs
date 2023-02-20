using System;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;

namespace CompassMobileUpdate.Services
{
	public interface IAuthService
	{
        Task<AuthResponse> GetAPIToken();
    }
}

