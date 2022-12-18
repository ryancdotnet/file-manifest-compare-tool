# File Manifest Compare Tool
Assist with comparing the files inside a .manifest file or across multiple .manifest files generated with the [File Manifest Tool](https://github.com/ryancdotnet/file-manifest-tool).

# Building
```
dotnet build src
```

# Running
```
.\src\bin\Debug\net7.0\FileManifestCompareTool.exe
```

## Outputs
Create 5 report files:
|File|Contents|
|-|-|
| same-basic-meta-hash.manifests-compare | Provides a list of all files that have the same basic meta hash (FileName + Size) |
| same-full-meta-hash.manifests-compare | Provies a list of all files that have the same full meta hash (FileName + Size + LastModifedDate) |
| same-hash.manifests-compare | Provides a list of all files that have the same data hash (NOTE: Only files with duplicate sizes are hashed) |
| same-names.manifests-compare | Provides a list of all files that have the same name |
| same-size.manifests-compare | Provides a list of all files that have the same size |