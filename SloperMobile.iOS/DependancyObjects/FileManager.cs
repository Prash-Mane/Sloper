using System;
using System.IO;
using System.Reflection;
using SloperMobile.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileManager))]
namespace SloperMobile.iOS
{
    public class FileManager : IFileManager
    {
        public byte[] ReadAllBytes(string filePath)
        {
            if (File.Exists(filePath))
                return File.ReadAllBytes(filePath);

            var docsPath = GetDocsFilePath(filePath);
            if (File.Exists(docsPath))
                return File.ReadAllBytes(docsPath);

            //try to get from PCL embedded resource
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var cFilename = $"SloperMobile.Embedded.{fileName}";
                var appassembly = typeof(App).GetTypeInfo().Assembly;
                Stream stream = appassembly.GetManifestResourceStream(cFilename);
                using (var sr = new StreamReader(stream))
                {
                    var buffer = default(byte[]);
                    using (var memStream = new MemoryStream())
                    {
                        sr.BaseStream.CopyTo(memStream);
                        buffer = memStream.ToArray();
                    }
                    return buffer;
                }
            }
            catch { }

            return null;
        }

        public string ReadAllText(string filePath)
        {
            if (File.Exists(filePath))
                return File.ReadAllText(filePath);

            var docsPath = GetDocsFilePath(filePath);
            if (File.Exists(docsPath))
                return File.ReadAllText(docsPath);

            //try to get from PCL embedded resource
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var cFilename = $"SloperMobile.Embedded.{fileName}";
                var appassembly = typeof(App).GetTypeInfo().Assembly;
                Stream stream = appassembly.GetManifestResourceStream(cFilename);
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
            catch { }

            return null;
        }

        public void WriteAllText(string content, string fileName)
        {
            var fullPath = GetDocsFilePath(fileName);
            File.WriteAllText(fullPath, content);
        }

        string GetDocsFilePath(string fileName) 
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(folder, fileName);
        }
    }
}
