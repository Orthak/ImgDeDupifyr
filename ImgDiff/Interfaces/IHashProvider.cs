using System.Threading.Tasks;

namespace ImgDiff.Interfaces
{
    public interface IHashProvider
    {
        Task<string> Generate(byte[] inputBytes);
    }
}
