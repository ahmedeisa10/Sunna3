using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tamkeen.Infrastructure.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder);
        Task<string> SaveIdCard(IFormFile file, string side);
        void DeleteImage(string url);
    }
}
