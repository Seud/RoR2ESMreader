using System.IO;

namespace ESMReader
{
    class ESMReader
    {
        static void Main(string[] args)
        {
            // Open the file
            string fileName = @"C:\Users\Julien\Desktop\ESM.dat";
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            // Check ESM size
            long totalLength = new FileInfo(fileName).Length;
            if(totalLength % 4 != 0) {
                Logger.Error("ESM.dat byte count wrong ! (Should be divisible by 4)");
            }

            // Main loop
            long fourBytes = totalLength / 4;
            EntityStateManager esm = new EntityStateManager();
            for(long i = 0; i < fourBytes; i++)
            {
                byte[] bytes = br.ReadBytes(4);
                esm.Read(bytes);
            }

            // Pull results
            esm.Output(fileName);
        }
    }
}
