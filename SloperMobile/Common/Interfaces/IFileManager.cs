using System;
namespace SloperMobile
{
    public interface IFileManager
    {
        string ReadAllText(string filePath);
        byte[] ReadAllBytes(string filePath);
        void WriteAllText(string content, string fileName);
    }
}
