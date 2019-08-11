## Metadata Public API Generator

MetadataPublicApiGeneratorhas no dependencies and simply creates a string the represents the public API. Any approval library can be used to approve the generated public API.

## How do I use it

> Install-package MetadataPublicApiGenerator

``` csharp
var publicApi = ApiGenerator.GeneratePublicApi(typeof(Library).Assembly);
```

### Manual

``` csharp
[Fact]
public void my_assembly_has_no_public_api_changes()
{
    var publicApi = ApiGenerator.GeneratePublicApi(typeof(Library).Assembly);

    var approvedFilePath = "PublicApi.approved.txt";
    if (!File.Exists(approvedFilePath))
    {
        // Create a file to write to.
        using (var sw = File.CreateText(approvedFilePath)) { }
    }

    var approvedApi = File.ReadAllText(approvedFilePath);

    Assert.Equal(approvedApi, publicApi);
}
```

### Shoudly

> Install-package Shouldly

``` csharp
[Fact]
public void my_assembly_has_no_public_api_changes()
{
    var publicApi = ApiGenerator.GeneratePublicApi(typeof(Library).Assembly);

    //Shouldly
    publicApi.ShouldMatchApproved();
}
```
