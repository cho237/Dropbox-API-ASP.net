using System.ComponentModel.DataAnnotations;

namespace DropboxApi
{
    public class ListFilesAndFolders 
    {
        public string? FolderName { get; set; }
    }

    public class FileUploadRequest 
    {
        public string File { get; set; }
        public string Folder { get; set; }
        public string Filename { get; set; }

    }

    public class FileDownloadRequest
    {
        public string Folder { get; set; }
        public string FileName { get; set; }
    }

}
