namespace rANS_Coder
{
    interface IWriter
    {
        void ContinueWriteToFile(byte byte_out);
        void ContinueWriteToFile(byte[] byte_out);
        void ContinueWriteToFile(ushort[] UInt16_out);
        void WriteFile(string FileName, byte byte_out);
        void WriteFile(string FileName, string Extension, byte[] byte_out);
    }
}