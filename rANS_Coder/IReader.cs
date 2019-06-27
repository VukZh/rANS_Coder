namespace rANS_Coder
{
    interface IReader
    {
        bool FlagRNS { get; set; }
        byte[] GetExtension { get; }

        byte[] ReadFile(string FileName);
    }
}