using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using WebApp1105.API.Models;
using WebApp1105.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp1105.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private const string AuthSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme +
            "," + JwtBearerDefaults.AuthenticationScheme;

        public StorageController(
            ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static ImageFormat FindImageFormat(string imgType)
        {
            return imgType switch
            {
                ".jpg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                _ => ImageFormat.Jpeg,
            };
        }

        private static string FindContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => "image",
                "image/png" => "image",
                "image/bmp" => "image",
                _ => "textFile",
            };
        }

        public int[,] imgSizes = new int[3, 2] {
            { 100, 100 },
            { 300, 300 },
            { 900, 900 }
        };

        [HttpPost, DisableRequestSizeLimit]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("UploadFile")]
        public async Task<ActionResult> UploadFile()
        {
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files["files.File"];
            if (file.Length > 0 && FindContentType(file.ContentType) == "image")
            {
                var folderName = Path.Combine("Resources", "Images", "Original", SelectFolder());
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                var antiDouble = 0;
                var newFileName = fileName;
                while (System.IO.File.Exists(fullPath))
                {
                    antiDouble++;
                    string fileMainName = "";
                    var fileNameSplit = fileName.Split('.');
                    for (int i = 0; i < fileNameSplit.Length - 1; i++)
                    {
                        fileMainName += fileNameSplit[i];
                    }
                    newFileName = $"{fileMainName}({antiDouble}).{fileNameSplit[fileNameSplit.Length-1]}";
                    fullPath = Path.Combine(pathToSave, newFileName);
                }
                var dbPath = Path.Combine(folderName, newFileName);
                using (FileStream fileStream = new(fullPath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                return Ok(new { dbPath });
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("SaveImage")]
        public async Task<IActionResult> SaveImage([FromBody] ImageCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var imgPathSplit = model.imgPath.Split(@"\");
                var originalPath = Path.Combine(Directory.GetCurrentDirectory(), model.imgPath);
                var imgFormat = FindImageFormat(Path.GetExtension(originalPath));
                var imgName = imgPathSplit[4];
                Image originalImg = new Bitmap(originalPath);
                for (int i = 0; i < imgSizes.Length / 2; i++)
                {
                    var folderName = Path.Combine("Resources", "Images", $"{imgSizes[i, 0]}x{imgSizes[i, 1]}", imgPathSplit[3]);
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var fullPath = Path.Combine(pathToSave, imgName);
                    var imgPath = Path.Combine(folderName, imgName);
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }
                    Bitmap newImg = new Bitmap(originalImg, new Size(imgSizes[i, 0], imgSizes[i, 1]));
                    newImg.Save(fullPath, imgFormat);

                    byte[] imgByte;
                    using (MemoryStream memoryStream = new())
                    {
                        newImg.Save(memoryStream, imgFormat);
                        imgByte = memoryStream.ToArray();
                    }
                    newImg.Dispose();
                    Data.Models.Image dbImage = new()
                    {
                        UserId = Guid.Parse(model.userId),
                        ImgName = imgName,
                        ImgData = imgByte,
                        ImgPath = imgPath,
                        ImgWidth = imgSizes[i, 0],
                        ImgHeight = imgSizes[i, 1],
                        Title = model.title,
                        Description = model.description
                    };
                    _dbContext.Add(dbImage);
                    await _dbContext.SaveChangesAsync(); //попробовать вынести из цикла
                }
                originalImg.Dispose();
                return Ok();
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost]
        [Route("GetImage")]
        public async Task<IActionResult> GetImage()
        {
            if (ModelState.IsValid)
            {
                List<int> imgWidths = new();
                List<int> imgHeights = new();
                List<string> imgPaths = new();
                List<ImageViewModel> response = new();
                var imageList = _dbContext.Images;
                foreach (Data.Models.Image img in imageList)
                {
                    imgPaths.Add(img.ImgPath);
                    imgWidths.Add(img.ImgWidth);
                    imgHeights.Add(img.ImgHeight);
                    if (img.ImgWidth == 900)
                    {
                        ImageViewModel image = new()
                        {
                            userId = img.UserId.ToString(),
                            imgName = img.ImgName,
                            imgPath = imgPaths.ToArray(),
                            imgWidth = imgWidths.ToArray(),
                            imgHeight = imgHeights.ToArray(),
                            title = img.Title,
                            description = img.Description
                        };
                        imgPaths.Clear();
                        imgWidths.Clear();
                        imgHeights.Clear();

                        response = response.Prepend(image).ToList();
                    }
                }
                return Ok(response);
            }
            return BadRequest("Invalid model object");
        }

        private string SelectFolder()
        {
            Random rnd = new();
            string[] hexValues = new string[]
                { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            return hexValues[rnd.Next(0, 15)] + hexValues[rnd.Next(0, 15)];
        }
    }
}