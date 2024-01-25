using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Mvc;
using System;


namespace DropboxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DropboxController : Controller
    {
        private readonly string dropboxToken = "";

        [HttpPost("listFilesAndFolders")]
        [ProducesResponseType(typeof(List<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListFilesAndFolders(ListFilesAndFolders data)
        {
            try
            {

                List<string> files = new();
                List<string> folders = new();

                using (var dbx = new DropboxClient(dropboxToken))
                {
                    var list = await dbx.Files.ListFolderAsync(data.FolderName);
                    

                    foreach (var item in list.Entries.Where(i => i.IsFolder))
                    {
                        folders.Add(item.Name);
                    }

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                    {
                        var fileName = item.Name;
                        var fileExtension = Path.GetExtension(fileName); 
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                        files.Add(fileNameWithoutExtension + " - " + fileExtension); 
                    }
                }

                var result = new
                {
                    Folders = folders,
                    Files = files
                };

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("uploadFile")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile([FromBody] FileUploadRequest request)
        {
            try
            {
                if (!System.IO.File.Exists(request.File))
                {
                    return NotFound("File not found at the specified path.");
                }

                using (var dbx = new DropboxClient(dropboxToken))
                {
                    using (var mem = new MemoryStream(System.IO.File.ReadAllBytes(request.File)))
                    {
                        var updated = await dbx.Files.UploadAsync(
                            "/" + request.Folder + "/" + request.Filename,
                            WriteMode.Overwrite.Instance,
                            body: mem);

                        var sharedLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(
                            "/" + request.Folder + "/" + request.Filename);

                        string url = sharedLink.Url;

                        return Ok(new { SharedUrl = url });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("downloadFile")]
        public async Task<IActionResult> DownloadFile(FileDownloadRequest request)
        {
            try
            {
                var folder = request.Folder;
                var file = request.FileName;

                using (var dbx = new DropboxClient(dropboxToken))
                {
                    using (var response = await dbx.Files.DownloadAsync(folder + "/" + file))
                    {
                        var s = response.GetContentAsByteArrayAsync();
                        s.Wait();
                        var d = s.Result;
                        System.IO.File.WriteAllBytes(file, d);
                    }
                }

                return Ok("Downloaded");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



    }
}
