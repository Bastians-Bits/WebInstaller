using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WebInstaller;
using WebInstaller.Model;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var configuration = app.Configuration.GetSection("WebInstaller").Get<WebInstallerSettings>();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/settings", () =>
    {
        return Results.Ok(configuration);
    });
}

app.MapGet("/installer/{os}", async (string os) =>
{
    var installer = new InstallerHelper(os);
    return Results.File(
        await installer.Installer(configuration.Installer),
        configuration.MimeTypes.GetValueOrDefault(os, "text/plain"),
        installer.DownloadName);
});

app.MapGet("/manifest", () =>
{
    return Results.Ok(new ManifestHelper().Load(configuration.Files));
});

app.MapGet("/compare", ([FromBody] Manifest manifest) =>
{
    return Results.Ok(new ManifestHelper().Compare(manifest, configuration.Files));
});

app.MapGet("files/{archiveType}", async ([FromBody] Manifest manifest, string archiveType) =>
{
    byte[]? archive = await new FileHelper().Files(manifest, archiveType, configuration.Files);

    if (archive == null)
        return Results.NoContent();

    return Results.File(archive, MediaTypeNames.Application.Zip, $"changes.{archiveType}");
} );

app.Run();