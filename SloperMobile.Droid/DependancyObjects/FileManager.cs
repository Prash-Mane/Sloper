using System;
using System.IO;
using Android.Content.Res;
using SloperMobile.Droid;
using Xamarin.Forms;
//using Android.Content.Res;
using System.Runtime.Remoting.Contexts;
using Android.Util;
using Android.Graphics.Drawables;
using Java.IO;
using Android.Graphics;
using System.Reflection;

[assembly: Dependency(typeof(FileManager))]
namespace SloperMobile.Droid
{
    public class FileManager : IFileManager
    {
        public byte[] ReadAllBytes(string filePath)
        {
            if (System.IO.File.Exists(filePath))
                return System.IO.File.ReadAllBytes(filePath);

            var docsFilePath = GetDocsFilePath(filePath);
            if (System.IO.File.Exists(docsFilePath))
                return System.IO.File.ReadAllBytes(docsFilePath);

            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            //try to get from resource - drawable
            try
            {
                return GetFromDrawable(fileName);
            }
            catch { }

            //try to get from assets
            try
            {
                using (var sr = new StreamReader(Android.App.Application.Context.Assets.Open(filePath)))
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

            //try to get from PCL embedded resource
            try
            {
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
            if (System.IO.File.Exists(filePath))
                return System.IO.File.ReadAllText(filePath);

            var docsFilePath = GetDocsFilePath(filePath);
            if (System.IO.File.Exists(docsFilePath))
                return System.IO.File.ReadAllText(docsFilePath);

            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            //try to get from resource - drawable
            try
            {
                var bytes = GetFromDrawable(fileName);
                return System.Text.Encoding.Default.GetString(bytes);
            }
            catch { }

            //try to get from assets
            try
            {
                using (var sr = new StreamReader(Android.App.Application.Context.Assets.Open(filePath)))
                {
                    return sr.ReadToEnd();
                }
            }
            catch { }

            //try to get from PCL embedded resource
            try
            {
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
            var dirPath = Android.App.Application.Context.FilesDir + "/MyFolder";
            if(!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var filepath = $"{dirPath}/{fileName}";

            if (!System.IO.File.Exists(filepath))
            {
                var newfile = new Java.IO.File(dirPath, fileName);
                using (FileOutputStream outfile = new FileOutputStream(newfile))
                {
                    outfile.Write(System.Text.Encoding.ASCII.GetBytes(content));
                    outfile.Close();
                }
            }
        }

        byte[] GetFromDrawable(string fileName)
        { 
            var id = (int)typeof(Resource.Drawable).GetField(fileName).GetValue(null);
            var dr = Android.App.Application.Context.GetDrawable(id);
            var bitmap = ((BitmapDrawable)dr).Bitmap;
            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            return stream.ToArray();
        }

        string GetDocsFilePath(string fileName)
        {
            var folder = Android.App.Application.Context.FilesDir + "/MyFolder";
            return System.IO.Path.Combine(folder, fileName);
        }
    }
}
