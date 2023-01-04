using CliWrap;
using Microsoft.AspNetCore.Mvc;

namespace rembg_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RemoveBackgroundController : ControllerBase
    {
        private readonly ILogger<RemoveBackgroundController> _logger;

        public RemoveBackgroundController(ILogger<RemoveBackgroundController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "RemoveBackground")]
        public async Task<IActionResult> PostAsync([FromForm] ImageModel model)
        {
            if (model?.Image == null)
            {
                return base.BadRequest("Image not present in request");
            }

            var extension = Path.GetExtension(model.FileName);
            if (string.IsNullOrEmpty(extension))
            {
                return base.BadRequest("Unable to determine extension from FileName");
            }

            if (extension != ".jpeg" && extension != ".jpg" && extension != ".png")
            {
                return base.BadRequest("Only jpeg, or png images are supported");
            }

            var filePath = Path.GetTempPath() + Guid.NewGuid().ToString() + extension;

            using (var stream = System.IO.File.Create(filePath))
            {
                await model.Image.CopyToAsync(stream);
            }

            var outputFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + extension;
            var workingDir = Path.GetDirectoryName(outputFilePath) ?? Path.GetTempPath();

            try
            {
                // Execute rembg cli
                var result = await Cli.Wrap("rembg")
                    .WithArguments($"i -m {model.ModelType ?? "u2net"} {filePath} {outputFilePath}")
                    .WithWorkingDirectory(workingDir)
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            if (System.IO.File.Exists(outputFilePath)) 
            { 
                var image = System.IO.File.OpenRead(outputFilePath);
                try
                {
                    return File(image, "image/jpeg");
                }
                finally
                {
                    Response.OnCompleted(async () =>
                    {
                        System.IO.File.Delete(filePath);
                        System.IO.File.Delete(outputFilePath);
                    });
                }
            }
            else
            {
                return Problem("Output file was not found");
            }
        }
    }
}