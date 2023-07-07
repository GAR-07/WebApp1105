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
using Microsoft.EntityFrameworkCore;

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
        [Route("UploadFiles")]
        public async Task<ActionResult> UploadFiles()
        {
            var formCollection = await Request.ReadFormAsync();
            var files = formCollection.Files;
            var message = CheckTypeValidation(files);
            if (message != "Ок")
            { 
                return Ok(new { message }); 
            }
            string[] fileName = new string[files.Count];
            string[] filePath = new string[files.Count];
            string[][] contentType = new string[files.Count][];
            for (var i = 0; i < files.Count; i++)
            {
                var folderName = Path.Combine("Resources", GetContentType(files[i].ContentType), "Original", SelectFolder());
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                fileName[i] = ContentDispositionHeaderValue.Parse(files[i].ContentDisposition).FileName.Trim('"');
                var fullPath = RemoveFileNameCollision(Path.Combine(pathToSave, fileName[i]));
                await using (FileStream fileStream = new(fullPath, FileMode.Create, FileAccess.Write))
                {
                    await files[i].CopyToAsync(fileStream);
                }
                contentType[i] = files[i].ContentType.Split('/');
                filePath[i] = Path.Combine(folderName, fullPath.Split(@"\").Last());
            }
            return Ok(new
            {
                fileName,
                filePath,
                contentType,
                message = "Загрузка завершена"
            });
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("SaveFiles")]
        public async Task<IActionResult> SaveFiles([FromBody] FileCreateViewModel[] files)
        {
            if (ModelState.IsValid)
            {
                string message = "Ошибка сохранения!";
                foreach (var model in files)
                {
                    message = model.fileType[0] switch
                    {
                        "image" => await ResizeNewImage(model),
                        "video" => await ResizeNewVideoAsync(model),
                        _ => "Неверно выбран формат файла, не сохранено!",
                    };
                }
                await _dbContext.SaveChangesAsync();
                return Ok(new { message });
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("SaveBook")]
        public async Task<IActionResult> SaveBook([FromBody] BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.title == "")
                {
                    return Ok(new { message = "Имя книги не задано, сохранение невозможно!" });
                }
                var folderName = Path.Combine("Resources", "Books", SelectFolder(), model.title);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                else
                {
                    pathToSave = RemoveFolderNameCollision(pathToSave);
                }
                var newImagePath = Path.Combine(Directory.GetCurrentDirectory(), model.coverPath);
                byte[] coverByte = SaveImage(pathToSave, newImagePath);

                var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), model.bookFilePath);
                byte[] bookFileByte = await SaveBookFileAsync(pathToSave, newFilePath);

                Data.Models.Book dbBook = new()
                {
                    UserId = Guid.Parse(model.userId),
                    Genre = model.genre,
                    Author = model.author,
                    Title = model.title,
                    Annotation = model.annotation,
                    BookFilePath = Path.Combine(folderName, model.bookFilePath.Split(@"\").Last()),
                    BookFileData = bookFileByte,
                    WritingDate = model.writingDate,
                    PublicationDate = model.publicationDate,
                    CoverPath = Path.Combine(folderName, model.coverPath.Split(@"\").Last()),
                    CoverData = coverByte,
                    NumberOfPages = model.numberOfPages,
                    Publisher = model.publisher,
                };
                _dbContext.Add(dbBook);
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "Сохранено"});
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("EditBook")]
        public async Task<IActionResult> EditBook([FromBody] BookViewModel[] model)
        {
            if (ModelState.IsValid || model.Length == 2)
            {
                var oldBook = model[0];
                var newBook = model[1];
                var dbBook = await _dbContext.Books.Where(p => p.Id == oldBook.id).FirstOrDefaultAsync();
                if (dbBook == null)
                {
                    return Ok(new { message = "Книга не найдена в БД" });
                }
                var folderName = oldBook.bookFilePath[..oldBook.bookFilePath.LastIndexOf(@"\")];
                if (oldBook.coverPath != newBook.coverPath)
                {
                    System.IO.File.Delete(oldBook.coverPath);
                    var newImagePath = Path.Combine(Directory.GetCurrentDirectory(), newBook.coverPath);
                    dbBook.CoverPath = Path.Combine(folderName, newBook.coverPath.Split(@"\").Last());
                    dbBook.CoverData = SaveImage(folderName, newImagePath);
                }
                if (oldBook.bookFilePath != newBook.bookFilePath)
                {
                    System.IO.File.Delete(oldBook.bookFilePath);
                    var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), newBook.bookFilePath);
                    dbBook.BookFilePath = Path.Combine(folderName, newBook.bookFilePath.Split(@"\").Last());
                    dbBook.BookFileData = await SaveBookFileAsync(folderName, newFilePath);
                }
                dbBook.UserId = Guid.Parse(newBook.userId);
                dbBook.Genre = newBook.genre;
                dbBook.Author = newBook.author;
                dbBook.Title = newBook.title;
                dbBook.Annotation = newBook.annotation;
                dbBook.WritingDate = newBook.writingDate;
                dbBook.PublicationDate = newBook.publicationDate;
                dbBook.NumberOfPages = newBook.numberOfPages;
                dbBook.Publisher = newBook.publisher;
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "Изменения сохранены" });
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage([FromBody] int[] ids)
        {
            foreach (int id in ids)
            {
                var image = await _dbContext.Images.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (image == null)
                {
                    return Ok(new { message = "Изображение не найдено в БД" });
                }
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), image.ImgPath));
                _dbContext.Remove(image);
            }
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Удалено" });
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("DeleteVideo")]
        public async Task<IActionResult> DeleteVideo([FromBody] int[] ids)
        {
            foreach (int id in ids)
            {
                var video = await _dbContext.Videos.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (video == null)
                {
                    return Ok(new { message = "Видео не найдено в БД" });
                }
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), video.VideoPath));
                _dbContext.Remove(video);
            }
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Удалено" });
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("DeleteBook")]
        public async Task<IActionResult> DeleteBook([FromBody] int id)
        {
            var book = await _dbContext.Books.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (book == null)
            {
                return Ok(new { message = "Книга не найдена в БД" });
            } 
            var folderName = book.BookFilePath[..book.BookFilePath.LastIndexOf(@"\")];
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), folderName), true);
            _dbContext.Remove(book);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Удалено" });
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("GetPersonalImages")]
        public IActionResult GetPersonalImages([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Images.Select(p => p.UserId).Where(p => p.ToString() == data.userId)
                    .ToList().Count - data.pageSize * 3 * data.page;
                var imagesList = _dbContext.Images.Where(p => p.UserId.ToString() == data.userId)
                    .Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize * 3 + startItem : data.pageSize * 3).ToList();
                return Ok(PrepareImagesForDelivery(imagesList));
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("GetPersonalVideos")]
        public IActionResult GetPersonalVideos([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Videos.Select(p => p.UserId).Where(p => p.ToString() == data.userId)
                    .ToList().Count - data.pageSize * 3 * data.page;
                var videosList = _dbContext.Videos.Where(p => p.UserId.ToString() == data.userId)
                    .Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize * 3 + startItem : data.pageSize * 3);
                return Ok(PrepareVideosForDelivery(videosList));
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost, Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("GetPersonalBooks")]
        public IActionResult GetPersonalBooks([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Books.Select(p => p.UserId).Where(p => p.ToString() == data.userId)
                    .ToList().Count - data.pageSize * data.page;
                var booksList = _dbContext.Books.Where(p => p.UserId.ToString() == data.userId)
                    .Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize + startItem : data.pageSize);
                return Ok(PrepareBooksForDelivery(booksList));
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost]
        [Route("GetImages")]
        public IActionResult GetImages([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Images.Select(p => p.Id).ToList().Count - data.pageSize * 3 * data.page;
                var imagesList = _dbContext.Images.Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize * 3 + startItem : data.pageSize * 3);
                return Ok(PrepareImagesForDelivery(imagesList));
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost]
        [Route("GetVideos")]
        public IActionResult GetVideos([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Videos.Select(p => p.Id).ToList().Count - data.pageSize * 3 * data.page;
                var videosList = _dbContext.Videos.Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize * 3 + startItem : data.pageSize * 3);
                return Ok(PrepareVideosForDelivery(videosList));
            }
            return BadRequest("Invalid model object");
        }

        [HttpPost]
        [Route("GetBooks")]
        public IActionResult GetBooks([FromBody] ItemsRequestModel data)
        {
            if (ModelState.IsValid)
            {
                var startItem = _dbContext.Books.Select(p => p.Id).ToList().Count - data.pageSize * data.page;
                var booksList = _dbContext.Books.Skip(startItem < 0 ? 0 : startItem)
                    .Take(startItem < 0 ? data.pageSize + startItem : data.pageSize);
                return Ok(PrepareBooksForDelivery(booksList));
            }
            return BadRequest("Invalid model object");
        }

        private static List<ImageViewModel> PrepareImagesForDelivery(IQueryable<Data.Models.Image> imagesList)
        {
            List<int> imgIds = new();
            List<int> imgWidths = new();
            List<int> imgHeights = new();
            List<string> imgPaths = new();
            List<ImageViewModel> response = new();
            foreach (Data.Models.Image img in imagesList)
            {
                imgIds.Add(img.Id);
                imgPaths.Add(img.ImgPath);
                imgWidths.Add(img.ImgWidth);
                imgHeights.Add(img.ImgHeight);
                if (img.ImgWidth == 900)
                {
                    ImageViewModel model = new()
                    {
                        id = imgIds.ToArray(),
                        userId = img.UserId.ToString(),
                        imgName = img.ImgName,
                        imgPath = imgPaths.ToArray(),
                        imgWidth = imgWidths.ToArray(),
                        imgHeight = imgHeights.ToArray(),
                        title = img.Title,
                        description = img.Description
                    };
                    imgIds.Clear();
                    imgPaths.Clear();
                    imgWidths.Clear();
                    imgHeights.Clear();
                    response = response.Prepend(model).ToList();
                }
            }
            return response;
        }

        private static List<ImageViewModel> PrepareImagesForDelivery(List<Data.Models.Image> imagesList)
        {
            List<int> imgIds = new();
            List<int> imgWidths = new();
            List<int> imgHeights = new();
            List<string> imgPaths = new();
            List<ImageViewModel> response = new();
            foreach (Data.Models.Image img in imagesList)
            {
                imgIds.Add(img.Id);
                imgPaths.Add(img.ImgPath);
                imgWidths.Add(img.ImgWidth);
                imgHeights.Add(img.ImgHeight);
                if (img.ImgWidth == 900)
                {
                    ImageViewModel model = new()
                    {
                        id = imgIds.ToArray(),
                        userId = img.UserId.ToString(),
                        imgName = img.ImgName,
                        imgPath = imgPaths.ToArray(),
                        imgWidth = imgWidths.ToArray(),
                        imgHeight = imgHeights.ToArray(),
                        title = img.Title,
                        description = img.Description
                    };
                    imgIds.Clear();
                    imgPaths.Clear();
                    imgWidths.Clear();
                    imgHeights.Clear();
                    response = response.Prepend(model).ToList();
                }
            }
            return response;
        }

        private static List<VideoViewModel> PrepareVideosForDelivery(IQueryable<Video> videosList)
        {
            List<int> videoIds = new();
            List<int> videoWidths = new();
            List<int> videoHeights = new();
            List<string> videoPaths = new();
            List<VideoViewModel> response = new();
            foreach (Video video in videosList)
            {
                videoIds.Add(video.Id);
                videoPaths.Add(video.VideoPath);
                videoWidths.Add(video.VideoWidth);
                videoHeights.Add(video.VideoHeight);
                if (video.VideoWidth == 900)
                {
                    VideoViewModel model = new()
                    {
                        id = videoIds.ToArray(),
                        userId = video.UserId.ToString(),
                        videoName = video.VideoName,
                        videoPath = videoPaths.ToArray(),
                        videoWidth = videoWidths.ToArray(),
                        videoHeight = videoHeights.ToArray(),
                        title = video.Title,
                        description = video.Description
                    };
                    videoIds.Clear();
                    videoPaths.Clear();
                    videoWidths.Clear();
                    videoHeights.Clear();
                    response = response.Prepend(model).ToList();
                }
            }
            return response;
        }

        private static List<BookViewModel> PrepareBooksForDelivery(IQueryable<Book> booksList)
        {
            List<BookViewModel> response = new();
            foreach (Book book in booksList)
            {
                BookViewModel model = new()
                {
                    id = book.Id,
                    userId = book.UserId.ToString(),
                    genre = book.Genre,
                    author = book.Author,
                    title = book.Title,
                    annotation = book.Annotation,
                    bookFilePath = book.BookFilePath,
                    writingDate = book.WritingDate,
                    publicationDate = book.PublicationDate,
                    coverPath = book.CoverPath,
                    numberOfPages = book.NumberOfPages,
                    publisher = book.Publisher,
                };
                response = response.Prepend(model).ToList();
            }
            return response;
        }

        public Task<string> ResizeNewImage(FileCreateViewModel model)
        {
            try
            {
                var filePathSplit = model.filePath.Split(@"\");
                var originalPath = Path.Combine(Directory.GetCurrentDirectory(), model.filePath);
                var fileName = filePathSplit[4];
                var imgFormat = GetImageFormat(originalPath);
                System.Drawing.Image originalImg = new Bitmap(originalPath);
                for (int i = 0; i < imgSizes.Length / 2; i++)
                {
                    var folderName = Path.Combine("Resources", "Images", $"{imgSizes[i, 0]}x{imgSizes[i, 1]}", filePathSplit[3]);
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var fullPath = RemoveFileNameCollision(Path.Combine(pathToSave, model.fileName));
                    var imgPath = Path.Combine(folderName, fullPath.Split(@"\").Last());
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }
                    var newImg = new Bitmap(originalImg, new Size(imgSizes[i, 0], imgSizes[i, 1]));
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
                        ImgName = model.fileName,
                        ImgData = imgByte,
                        ImgPath = imgPath,
                        ImgWidth = imgSizes[i, 0],
                        ImgHeight = imgSizes[i, 1],
                        Title = model.title,
                        Description = model.description
                    };
                    _dbContext.Add(dbImage);
                }
                originalImg.Dispose();
                return Task.FromResult("Сохранено");
            }
            catch
            {
                return Task.FromResult("Ошибка сохранения изображения " + model.fileName);
            }
        }

        public async Task<string> ResizeNewVideoAsync(FileCreateViewModel model)
        {
            try
            {
                var filePathSplit = model.filePath.Split(@"\");
                var originalPath = Path.Combine(Directory.GetCurrentDirectory(), model.filePath);
                var fileName = filePathSplit[4];
                var ffmpegPath = Path.Combine(@"C:\ffmpeg-n6.0-latest-win64-gpl-6.0", "ffmpeg.exe");
                for (int i = 0; i < videoSizes.Length / 2; i++)
                {
                    var folderName = Path.Combine("Resources", "Videos", $"{videoSizes[i, 0]}x{videoSizes[i, 1]}", filePathSplit[3]);
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    var fullPath = RemoveFileNameCollision(Path.Combine(pathToSave, model.fileName));
                    var videoPath = Path.Combine(folderName, fullPath.Split(@"\").Last());
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }
                    ProcessStartInfo processInfo = new(ffmpegPath, " -i \"" + originalPath +
                        $"\" -aspect 1:1 -vf scale={videoSizes[i, 0]}x{videoSizes[i, 1]} \"" + fullPath + "\"")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using (var process = Process.Start(processInfo))
                    {
                        await process.WaitForExitAsync();
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
                        VideoName = model.fileName,
                        VideoData = videoByte,
                        VideoPath = videoPath,
                        VideoWidth = videoSizes[i, 0],
                        VideoHeight = videoSizes[i, 1],
                        Title = model.title,
                        Description = model.description
                    };
                    _dbContext.Add(dbVideo);
                }
                return "Сохранено";
            }
            catch
            {
                return "Ошибка сохранения видео " + model.fileName;
            }
        }

        private static string CheckTypeValidation(IFormFileCollection files)
        {
            var operationType = ContentDispositionHeaderValue.Parse(files[0].ContentDisposition).Name.Trim('"');
            if (operationType == "files.File")
            {
                return ValidationFiles(files);
            }
            else if (operationType == "files.Book")
            {
                return ValidationBook(files);
            }
            return "Неожиданное значение ContentDisposition.Name";
        }

        private static string ValidationFiles(IFormFileCollection files)
        {
            if (files.Count > 10)
            {
                return "Нельзя загружать более 10 файлов за один раз!";
            }
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    return "Файл " + file.FileName + " имеет нулевой размер!";
                }
                if (file.Length > 100000000)
                {
                    return "Файл " + file.FileName + " слишком большой!";
                }
                var type = GetContentType(file.ContentType);
                if (type == "Forbidden" || type == "BookFiles")
                {
                    return "Неверный формат файла!";
                }
            }
            return "Ок";
        }

        private static string ValidationBook(IFormFileCollection files)
        {
            if (files.Count != 1)
            {
                return "Можно загружать только по одному файлу!";
            }
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    return "Файл " + file.FileName + " имеет нулевой размер!";
                }
                if (file.Length > 100000000)
                {
                    return "Файл " + file.FileName + " слишком большой!";
                }
                var type = GetContentType(file.ContentType);
                if (!(type == "Books" || type == "Images") || file.ContentType == "image/gif")
                {
                    return "Неверный формат файла!";
                }
            }
            return "Ок";
        }

        private static string RemoveFileNameCollision(string fullPath)
        {
            var collisionCount = 0;
            var fileName = fullPath.Split(@"\").Last();
            var pathToSave = fullPath[..fullPath.LastIndexOf(@"\")];
            while (System.IO.File.Exists(fullPath))
            {
                collisionCount++;
                var newFileName = $"{fileName[..fileName.LastIndexOf('.')]}({collisionCount}).{fileName.Split('.').Last()}";
                fullPath = Path.Combine(pathToSave, newFileName);
            }
            return fullPath;
        }

        private static string RemoveFolderNameCollision(string fullPath)
        {
            var collisionCount = 0;
            var pathToFolder = fullPath[..fullPath.LastIndexOf(@"\")];
            var folderName = fullPath.Split(@"\").Last();
            while (Directory.Exists(fullPath))
            {
                collisionCount++;
                fullPath = Path.Combine(pathToFolder, $"{folderName}({collisionCount})");
            }
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        private static byte[] SaveImage(string pathToSave, string newImagePath)
        {
            System.Drawing.Image originalImg = new Bitmap(newImagePath);
            var newImage = new Bitmap(originalImg, new Size(imgSizes[1, 0], imgSizes[1, 1]));
            newImage.Save(Path.Combine(pathToSave, newImagePath.Split(@"\").Last()), GetImageFormat(newImagePath));

            byte[] coverByte;
            using (MemoryStream memoryStream = new())
            {
                newImage.Save(memoryStream, GetImageFormat(newImagePath));
                coverByte = memoryStream.ToArray();
            }
            originalImg.Dispose();
            newImage.Dispose();
            return coverByte;
        }

        private static async Task<byte[]> SaveBookFileAsync(string pathToSave, string newFilePath)
        {
            System.IO.File.Move(newFilePath, Path.Combine(pathToSave, newFilePath.Split(@"\").Last()));

            byte[] bookFileByte;
            using (FileStream fileStream = new(
                Path.Combine(pathToSave, newFilePath.Split(@"\").Last()), FileMode.Open, FileAccess.Read))
            {
                bookFileByte = new byte[fileStream.Length];
                await fileStream.ReadAsync(bookFileByte);
                fileStream.Close();
            }
            return bookFileByte;
        }

        private static string SelectFolder()
        {
            Random rnd = new();
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
                "image/gif" => "Images",
                "video/mp4" => "Videos",
                "text/plain" => "Books",
                "application/octet-stream" => "Books",
                _ => "Forbidden",
            };
        }

        private static readonly string[] hexValues = new string[]
        { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        private static readonly int[,] imgSizes = new int[3, 2] {
            { 100, 100 },
            { 300, 300 },
            { 900, 900 }
        };

        private static readonly int[,] videoSizes = new int[3, 2] {
            { 100, 100 },
            { 300, 300 },
            { 900, 900 }
        };
    }
}