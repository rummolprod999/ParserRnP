using System.Data;
using FluentFTP;

namespace ParserRnP
{
    public interface IParser
    {
        void Parsing();
        DataTable GetRegions();
        void GetListFileArch(string Arch, string PathParse);
        void GetListFileArch(string Arch, string PathParse, string region, int region_id);
        void GetListFileArch(string Arch, string PathParse, string region, int region_id, string purchase);
        string GetArch44(string Arch, string PathParse);
    }
}