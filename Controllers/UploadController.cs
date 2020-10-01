using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Drop.Controllers
{
    public class UploadController : Controller
    {
        private readonly IConfiguration _configuration;

        public UploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("Error", error);
            }

            return View();
        }

        private async Task<string> GetFilePath(string original)
        {
            return await Task.Run(() =>
            {
                if (original == null)
                {
                    throw new ArgumentNullException(nameof(original));
                }

                string configDirectory = _configuration.GetValue("UploadDirectory", "uploads");

                string fullDirectory = Path.GetFullPath(configDirectory);

                if (!Directory.Exists(fullDirectory))
                {
                    Directory.CreateDirectory(fullDirectory);
                }

                string path = Path.Combine(fullDirectory, original);

                if (!System.IO.File.Exists(path)) return path;

                string nameWithoutExt = Path.GetFileNameWithoutExtension(original);

                if (string.IsNullOrWhiteSpace(nameWithoutExt)) nameWithoutExt = "blank name";

                string format = Path.Combine(fullDirectory, nameWithoutExt) + " ({0})" + Path.GetExtension(original);

                int i = 1;
                while (System.IO.File.Exists(path = string.Format(format, i)))
                {
                    i++;
                }

                return path;
            });
        }

        private async Task SaveText(string text)
        {
            string path = await GetFilePath(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".txt");

            await System.IO.File.WriteAllTextAsync(path, text);
        }

        private async Task SaveFile(IFormFile file)
        {
            string path = await GetFilePath(file.FileName);

            await using FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);

            await file.CopyToAsync(stream);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(string password, string text, IFormFileCollection files)
        {
            if (password != _configuration["UploadPassword"])
            {
                return RedirectToAction("Index", new { error = "Bad password" });
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                await SaveText(text);
            }

            if (files?.Count > 0)
            {
                foreach (IFormFile file in files)
                {
                    await SaveFile(file);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
