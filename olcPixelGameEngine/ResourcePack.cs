using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace olc
{
    public class ResourcePack
    {
        //ResourcePack();
        bool AddFile(string sFile)
        {
            string file = sFile; // makeposix

            if (File.Exists(file))
            {
                sResourceFile e = new sResourceFile
                {
                    nSize = (int)new FileInfo(file).Length,
                    nOffset = 0 // Unknown at this stage			
                };
                mapFiles[file] = e;
                return true;
            }
            return false;
        }
        bool LoadPack(string sFile, string sKey)
        {
            if (!File.Exists(sFile)) return false;
            br = new BinaryReader(new FileStream(sFile, FileMode.Open));
            if (!br.BaseStream.CanRead) return false;

            int nIndexSize = br.ReadInt32();
            char[] buffer = br.ReadChars(nIndexSize);
            char[] decoded = scramble(buffer, sKey);

            int pos = 0;
            Action<char[], int> read = (char[] dst, int size) =>
            {
                Array.Copy(decoded, pos, dst, 0, size);
                pos += size;
            };
            Func<byte> get = () =>
            {
                char[] c = new char[1];
                read(c, 1);
                return (byte)c[0];
            };


            // 2) Read Map
            int nMapEntries = br.ReadInt32();
            for (int i = 0; i < nMapEntries; i++)
            {
                int nFilePathSize = get();

                char[] c = new char[nFilePathSize];
                for (int j = 0; j < nFilePathSize; j++)
                {
                    c[j] = (char)get();
                }
                string sFileName = new string(c);

                char[] cnSize = new char[sizeof(int)];
                read(cnSize, sizeof(int));

                char[] cnOffset = new char[sizeof(Int32)];
                read(cnOffset, sizeof(int));

                sResourceFile e = new sResourceFile();
                e.nSize = BitConverter.ToInt32(Encoding.GetEncoding("UTF-8").GetBytes(cnSize));
                e.nOffset = BitConverter.ToInt32(Encoding.GetEncoding("UTF-8").GetBytes(cnOffset));
                mapFiles.Add(sFileName, e);
            }
            // Don't close base file! we will provide a stream
            // pointer when the file is requested
            return true;
        }

        BinaryReader br;
        bool SavePack(string sFile, string sKey)
        {
            // Create/Overwrite the resource file
            //std::ofstream ofs(sFile, std::ofstream::binary);
            BinaryWriter bw = new BinaryWriter(new FileStream(sFile, FileMode.Create));
            // if (!ofs.is_open()) return false;
            if (!bw.BaseStream.CanWrite) return false;

            

            // Iterate through map
            int nIndexSize = 0; // Unknown for now
            //ofs.write((char*)&nIndexSize, sizeof(uint32_t));
            bw.Write(nIndexSize);

            //uint32_t nMapSize = uint32_t(mapFiles.size());
            int nMapSize = mapFiles.Count;

            //ofs.write((char*)&nMapSize, sizeof(uint32_t));
            bw.Write(nMapSize);


            // for (auto & e : mapFiles)
            foreach (var e in mapFiles)
            {
                // Write the path of the file
                //size_t nPathSize = e.first.size();
                int nPathSize = e.Key.Length;

                //ofs.write((char*)&nPathSize, sizeof(uint32_t));
                bw.Write(nPathSize);

                //ofs.write(e.first.c_str(), nPathSize);
                bw.Write(e.Key);

                // Write the file entry properties
                //ofs.write((char*)&e.second.nSize, sizeof(uint32_t));
                bw.Write(e.Value.nSize);

                //ofs.write((char*)&e.second.nOffset, sizeof(uint32_t));
                bw.Write(e.Value.nOffset);
            }

            // 2) Write the individual Data
            //std::streampos offset = ofs.tellp();
            long offset = bw.BaseStream.Position;

            //nIndexSize = (uint32_t)offset;
            nIndexSize = (int)offset;

            //for (auto & e : mapFiles)
            foreach (var e in mapFiles) 
            {

                // Store beginning of file offset within resource pack file
                //e.second.nOffset = (uint32_t)offset;
                e.Value.nOffset = (int)offset;

                // Load the file to be added
                //std::vector<uint8_t> vBuffer(e.second.nSize);
                List<byte> vBuffer = new List<byte>(e.Value.nSize);

                //std::ifstream i(e.first, std::ifstream::binary);
                BinaryReader i = new BinaryReader(new FileStream(e.Key, FileMode.Open));

                //i.read((char*)vBuffer.data(), e.second.nSize);
                vBuffer.AddRange(i.ReadBytes(e.Value.nSize));
                //i.close();
                i.Close();

                // Write the loaded file into resource pack file
                //ofs.write((char*)vBuffer.data(), e.second.nSize);
                bw.Write(vBuffer.ToArray());
                
                //offset += e.second.nSize;
                offset += e.Value.nSize;
            }

            // 3) Scramble Index
            //std::vector<char> stream;
            List<char> stream = new List<char>();

            //auto write = [&stream](const char* data, size_t size) {
            //    size_t sizeNow = stream.size();
            //    stream.resize(sizeNow + size);
            //    memcpy(stream.data() + sizeNow, data, size);
            //};
            Action<char[], int> writeC = (char[] data, int size) =>
            {
                int sizeNow = stream.Count;
                stream.Capacity = sizeNow + size;
                Array.Copy(data, stream.ToArray(), size);
            };
            Action<byte[], int> writeB = (byte[] data, int size) =>
            {
                int sizeNow = stream.Count;
                stream.Capacity = sizeNow + size;
                Array.Copy(data, stream.ToArray(), size);
            };


            // Iterate through map
            //write((char*)&nMapSize, sizeof(uint32_t));
            writeB(BitConverter.GetBytes(nMapSize),sizeof(int));

            //for (auto & e : mapFiles)
            foreach (var e in mapFiles)
            {
                // Write the path of the file
                //size_t nPathSize = e.first.size();
                int nPathSize = e.Key.Length;

                //write((char*)&nPathSize, sizeof(uint32_t));
                writeB(BitConverter.GetBytes(nPathSize), sizeof(int));

                //write(e.first.c_str(), nPathSize);
                writeC(e.Key.ToCharArray(), nPathSize);

                // Write the file entry properties
                //write((char*)&e.second.nSize, sizeof(uint32_t));
                writeB(BitConverter.GetBytes(e.Value.nSize), sizeof(int));
                //write((char*)&e.second.nOffset, sizeof(uint32_t));
                writeB(BitConverter.GetBytes(e.Value.nOffset), sizeof(int));
            }

            //std::vector<char> sIndexString = scramble(stream, sKey);
            var sIndexString = scramble(stream.ToArray(), sKey);
            
            //uint32_t nIndexStringLen = uint32_t(sIndexString.size());
            int nIndexStringLen = sIndexString.Length;

            // 4) Rewrite Map (it has been updated with offsets now)
            // at start of file
            //ofs.seekp(0, std::ios::beg);
            bw.Seek(0, SeekOrigin.Begin);

            //ofs.write((char*)&nIndexStringLen, sizeof(uint32_t));
            bw.Write(nIndexStringLen);

            //ofs.write(sIndexString.data(), nIndexStringLen);
            bw.Write(sIndexString);

            //ofs.close();
            bw.Close();
            
            return true;
        }


        ResourceBuffer GetFileBuffer(string sFile)
        {
            return new ResourceBuffer(br, mapFiles[sFile].nOffset, mapFiles[sFile].nSize);
        }

        bool Loaded()
        {
            return br.BaseStream.CanRead;
        }

        Dictionary<string, sResourceFile> mapFiles;        

        char[] scramble(char[] data, string key)
        {
            if (string.IsNullOrEmpty(key)) return data;
            List<char> o = new List<char>();
            int c = 0;
            foreach (var s in data) o.Add((char)(s ^ key[(c++) % key.Length]));
            return o.ToArray();
        }

        //string makeposix(string path);


    }
}