using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ryancdotnet.FileManifestTool;
using MD5Hash;
using System.Text;

namespace Ryancdotnet.FileManifestCompareTool;

class Program
{
    static void Main(string[] args)
    {
        List<string> manifestFiles = new List<string>();

        string inputLocation = "";

        do
        {
            Console.WriteLine("Enter a manifest file location or a directory containing .manifest files (enter 'd' to finish adding):");
            inputLocation = Console.ReadLine();

            if (Directory.Exists(inputLocation))
            {
                //It's a dir
                foreach (string manifestFile in Directory.EnumerateFiles(inputLocation, "*.manifest"))
                {
                    if (CheckIfManifestFile(manifestFile))
                    {
                        Console.WriteLine($"Added manifest to working set: {manifestFile}");
                        manifestFiles.Add(manifestFile);
                    }
                }
            }
            else if (File.Exists(inputLocation) && CheckIfManifestFile(inputLocation))
            {
                Console.WriteLine($"Added manifest to working set: {inputLocation}");
                manifestFiles.Add(inputLocation);
            }
        } while (inputLocation.ToLower() != "d");

        Console.WriteLine("Enter output location for reports:");
        string outputFolder = Console.ReadLine();

        //Load up all the manifests into memory
        List<FileData> foundFiles = manifestFiles.Select(mf => LoadFileDataFromManifestFile(mf)).SelectMany((a, b) => a).ToList();

        //Build hashsets
        Dictionary<string, List<FileData>> sameFileNameSet = GenerateGroupedDictionary(foundFiles, ff => Path.GetFileName(ff.File));
        Dictionary<string, List<FileData>> sameBasicMetaHashSet = GenerateGroupedDictionary(foundFiles, ff => ff.BasicMetaHash);
        Dictionary<string, List<FileData>> sameFullMetaHashSet = GenerateGroupedDictionary(foundFiles, ff => ff.FullMetaHash);
        Dictionary<string, List<FileData>> sameSizeSet = GenerateGroupedDictionary(foundFiles, ff => ff.Size.ToString());
        Dictionary<string, List<FileData>> sameHashSet = GenerateGroupedDictionary(GetDuplicateFileDatas(sameSizeSet).Select(fd =>
        {
            fd.Hash = GetFileHash(fd.File);
            return fd;
        }).ToList(), ff => ff.Hash);

        //Write out results
        File.WriteAllLines(Path.Combine(outputFolder, "same-names.manifests-compare"), GetDuplicateFileDatas(sameFileNameSet).Select(fd => $"{Path.GetFileName(fd.File)} | {fd.Size} | {fd.File} "));
        File.WriteAllLines(Path.Combine(outputFolder, "same-basic-meta-hash.manifests-compare"), GetDuplicateFileDatas(sameBasicMetaHashSet).Select(fd => $"{fd.BasicMetaHash} | {fd.Size} | {fd.File}"));
        File.WriteAllLines(Path.Combine(outputFolder, "same-full-meta-hash.manifests-compare"), GetDuplicateFileDatas(sameFullMetaHashSet).Select(fd => $"{fd.FullMetaHash} | {fd.Size} | {fd.File}"));
        File.WriteAllLines(Path.Combine(outputFolder, "same-size.manifests-compare"), GetDuplicateFileDatas(sameSizeSet).Select(fd => $"{fd.Size} | {fd.File}"));
        File.WriteAllLines(Path.Combine(outputFolder, "same-hash.manifests-compare"), GetDuplicateFileDatas(sameHashSet).Select(fd => $"{fd.Hash} | {fd.Size} | {fd.File}"));
    }

    private static string GetFileHash(string file)
    {
        string currentHash = "";
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        int offset = 0;

        using (FileStream br = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            long bytesRead = 0;
            do
            {
                bytesRead = br.Read(buffer, offset, bufferSize);
                currentHash = MD5Hash.Hash.GetMD5(currentHash + Encoding.UTF8.GetString(buffer));
            }
            while (bytesRead == bufferSize);
        }

        return currentHash;
    }

    private static IEnumerable<FileData> GetDuplicateFileDatas(Dictionary<string, List<FileData>> groupedFileDatas) => groupedFileDatas.Where(g => g.Value.Count > 1).SelectMany(kvp => kvp.Value);

    private static Dictionary<string, List<FileData>> GenerateGroupedDictionary(List<FileData> fileDatas, Func<FileData, string> keyGetter)
    {
        Dictionary<string, List<FileData>> results = new Dictionary<string, List<FileData>>();

        foreach (FileData fileData in fileDatas)
        {
            string key = keyGetter(fileData);
            
            if (results.ContainsKey(key))
            {
                results[key].Add(fileData);
            }
            else
            {
                results.Add(key, new List<FileData>{ fileData });
            }
        }

        return results;
    }

    private static bool CheckIfManifestFile(string file)
    {
        using (StreamReader streamReader = new StreamReader(File.OpenRead(file)))
        {
            streamReader.ReadLine();

            try
            {
                Version fileManifestVersion = Version.Parse(streamReader.ReadLine());

                return true;
            }
            catch
            {
                //Gulp - not a manifest!
                return false;
            }
        }
    }

    private static List<FileData> LoadFileDataFromManifestFile(string file)
        => File.ReadAllLines(file).Skip(2).Select(l => l.Split(" | ")).Select(s => new FileData
        {
            BasicMetaHash = s[0],
            FullMetaHash = s[1],
            FileNumber = Int64.Parse(s[2]),
            File = Path.Combine(s[4], s[3]),
            Size = Int64.Parse(s[5]),
            Created = DateTime.Parse(s[6]),
            LastModified = DateTime.Parse(s[7])
        }).ToList();
}
