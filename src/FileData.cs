using System;

namespace Ryancdotnet.FileManifestTool;

public class FileData
{
    public long FileNumber { get; set; }
    public string File { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime Created { get; set; }
    public long Size { get; set; }

    /// <summary>FullMetaHash is a hash of just the metadata properties of the file, ie. FileName + LastModified + Size</summary>
    public string FullMetaHash { get; set; }
    
    /// <summary>BasicMetaHash is a hash of just the metadata properties of the file, ie. FileName + Size</summary>
    public string BasicMetaHash { get; set; }

    public string Hash { get; set; }
}