using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using WebApp1105.API.Models;
using WebApp1105.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using WebApp1105.API.Data.Models;

#pragma warning disable CA1416
#pragma warning disable CS8602
#pragma warning disable CS8604

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

        [HttpPost, DisableRequestSizeLimit]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("UploadFile")]
        public async Task<ActionResult> UploadFile()
        {
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files["files.File"];
            var contentType = GetContentType(file.ContentType);
            if (file.Length > 0)
            {
                var folderName = Path.Combine("Resources", contentType, "Original", SelectFolder());
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                var antiDouble = 0;
                var newFileName = fileName;
                while (System.IO.File.Exists(fullPath)) //возможно, стоит избавиться от newFileName?
                {
                    antiDouble++;
                    string fileMainName = string.Empty;
                    var fileNameSplit = fileName.Split('.');
                    for (int i = 0; i < fileNameSplit.Length - 1; i++)
                    {
                        fileMainName += fileNameSplit[i];
                    }
                    newFileName = $"{fileMainName}({antiDouble}).{fileName.Split('.').Last()}";
                    fullPath = Path.Combine(pathToSave, newFileName);
                }
                var dbPath = Path.Combine(folderName, newFileName);
                await using (FileStream fileStream = new(fullPath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }
                return Ok(new
                {
                    dbPath,
                    contentType = file.ContentType.Split('/')
                });
            }
            return BadRequest("Invalid model object");
        }


        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("SaveFile")]
        public async Task<IActionResult> SaveFile([FromBody] FileCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var filePathSplit = model.filePath.Split(@"\");
                var originalPath = Path.Combine(Directory.GetCurrentDirectory(), model.filePath);
                var fileName = filePathSplit[4];

                if (model.fileType[0] == "image")
                {
                    var imgFormat = GetImageFormat(originalPath);
                    System.Drawing.Image originalImg = new Bitmap(originalPath);
                    for (int i = 0; i < imgSizes.Length / 2; i++)
                    {
                        var folderName = Path.Combine("Resources", "Images", $"{imgSizes[i, 0]}x{imgSizes[i, 1]}", filePathSplit[3]);
                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var imgPath = Path.Combine(folderName, fileName);
                        if (!Directory.Exists(pathToSave))
                        {
                            Directory.CreateDirectory(pathToSave);
                        }

                        Bitmap newImg = new Bitmap(originalImg, new Size(imgSizes[i, 0], imgSizes[i, 1]));
                        newImg.Save(fullPath, imgFormat);

                        byte[] imgByte;
                        using(MemoryStream memoryStream = new())
                        {
                            newImg.Save(memoryStream, imgFormat);
                            imgByte = memoryStream.ToArray();
                        }
                        newImg.Dispose();
                        Data.Models.Image dbImage = new()
                        {
                            UserId = Guid.Parse(model.userId),
                            ImgName = fileName,
                            ImgData = imgByte,
                            ImgPath = imgPath,
                            ImgWidth = imgSizes[i, 0],
                            ImgHeight = imgSizes[i, 1],
                            Title = model.title,
                            Description = model.description
                        };
                        _dbContext.Add(dbImage);
                    }
                    await _dbContext.SaveChangesAsync();
                    originalImg.Dispose();
                }
                else if (model.fileType[0] == "video")
                {
                    var ffmpegPath = Path.Combine(@"C:\ffmpeg-n6.0-latest-win64-gpl-6.0", "ffmpeg.exe"); //изменено без тестирования!
                    for (int i = 0; i < videoSizes.Length / 2; i++)
                    {
                        var folderName = Path.Combine("Resources", "Videos", $"{videoSizes[i, 0]}x{videoSizes[i, 1]}", filePathSplit[3]);
                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var videoPath = Path.Combine(folderName, fileName);
                        if (!Directory.Exists(pathToSave))
                        {
                            Directory.CreateDirectory(pathToSave);
                        }

                        var processInfo = new ProcessStartInfo(ffmpegPath, " -i \"" + originalPath + 
                            $"\" -aspect 1:1 -vf scale={videoSizes[i, 0]}x{videoSizes[i, 1]} \"" + fullPath + "\"")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        using (Process process = Process.Start(processInfo))
                        {
                            process.WaitForExit();
                            process.Close();
                        }

                        byte[] videoByte;
                        using (FileStream fileStream = new(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            videoByte = new byte[fileStream.Length];
                            await fileStream.ReadAsync(videoByte);
                            fileStream.Close();
                        }

                        Video dbVideo = new()
                        {
                            UserId = Guid.Parse(model.userId),
                            VideoName = fileName,
                            VideoData = videoByte,
                            VideoPath = videoPath,
                            VideoWidth = videoSizes[i, 0],
                            VideoHeight = videoSizes[i, 1],
                            Title = model.title,
                            Description = model.description
                        };
                        _dbContext.Add(dbVideo);
                    }
                    await _dbContext.SaveChangesAsync();
                }
                return Ok();
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost]
        [Route("GetImage")]
        public IActionResult GetImage()
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

        [HttpPost]
        [Route("GetVideo")]
        public IActionResult GetVideo()
        {
            if (ModelState.IsValid)
            {
                List<int> videoWidths = new();
                List<int> videoHeights = new();
                List<string> videoPaths = new();
                List<VideoViewModel> response = new();
                var videoList = _dbContext.Videos;
                foreach (Video video in videoList)
                {
                    videoPaths.Add(video.VideoPath);
                    videoWidths.Add(video.VideoWidth);
                    videoHeights.Add(video.VideoHeight);
                    if (video.VideoWidth == 900)
                    {
                        VideoViewModel image = new()
                        {
                            userId = video.UserId.ToString(),
                            videoName = video.VideoName,
                            videoPath = videoPaths.ToArray(),
                            videoWidth = videoWidths.ToArray(),
                            videoHeight = videoHeights.ToArray(),
                            title = video.Title,
                            description = video.Description
                        };
                        videoPaths.Clear();
                        videoWidths.Clear();
                        videoHeights.Clear();

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

        private static ImageFormat GetImageFormat(string imgPath)
        {
            return Path.GetExtension(imgPath) switch
            {
                ".gif" => ImageFormat.Gif,
                ".jpg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                _ => throw new NotImplementedException(),
            };
        }

        private static string GetContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => "Images",
                "image/png" => "Images",
                "image/bmp" => "Images",
                "video/mp4" => "Videos",
                _ => "File",
            };
        }

        public int[,] imgSizes = new int[3, 2] {
            { 100, 100 },
            { 300, 300 },
            { 900, 900 }
        };

        public int[,] videoSizes = new int[3, 2] {
            { 100, 100 },
            { 300, 300 },
            { 900, 900 }
        };
    }
}