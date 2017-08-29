using System.Data;
using FluentFTP;

namespace ParserRnP
{
    public interface IParser
    {
        void Parsing();
        DataTable GetRegions();
        void GetListFileArch(string arch, string pathParse);
        void GetListFileArch(string arch, string pathParse, string region, int regionId);
        void GetListFileArch(string arch, string pathParse, string region, int regionId, string purchase);
        string GetArch44(string arch, string pathParse);
    }
}