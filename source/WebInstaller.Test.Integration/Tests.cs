using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using Newtonsoft.Json;
using WebInstaller.Model;
using Xunit;

namespace WebInstaller.Test.Integration;

public class Tests
{
    [Fact(DisplayName = "Test to get a manifest")]
    public async void Test_Manifest()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();
        var manifest = await client.GetFromJsonAsync<Manifest>("/manifest");

        Assert.Equal(7, manifest?.Files.Count);
    }

    [Fact(DisplayName = "Test for the comparator to register added items")]
    public async void Test_Compare_Added()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        Manifest? currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");
        currentManifest?.Files.RemoveAt(0);

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/compare"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var changelist = await (await client.SendAsync(requestMessage)).Content.ReadFromJsonAsync<Changeset>();

        Assert.Equal(ChangeType.A, changelist?.Changes[0].ChangeType);
    }

    [Fact(DisplayName = "Test for the comparator to register modified items")]
    public async void Test_Compare_Modifed()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

#pragma warning disable CS8600
        Manifest currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");
#pragma warning restore CS8600
        currentManifest?.Files.Insert(0, new File(currentManifest.Files[0].FullName, currentManifest.Files[0].Size + 10L, currentManifest.Files[0].DateOfModification));

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/compare"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var changelist = await (await client.SendAsync(requestMessage)).Content.ReadFromJsonAsync<Changeset>();

        Assert.Equal(ChangeType.M, changelist?.Changes[0].ChangeType);
    }

    [Fact(DisplayName = "Test for the comparator to register deleted items")]
    public async void Test_Compare_Deleted()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        Manifest? currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");
        currentManifest?.Files.Add(new File("Test99", 1024, DateTime.Now));

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/compare"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var changelist = await (await client.SendAsync(requestMessage)).Content.ReadFromJsonAsync<Changeset>();

        Assert.Equal(ChangeType.D, changelist?.Changes[0].ChangeType);
    }

    [Fact(DisplayName = "Test for the comparator to register an empty manifest")]
    public async void Test_Compare_Empty()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/compare")
        };

        var changelist = await (await client.SendAsync(requestMessage)).Content.ReadFromJsonAsync<Changeset>();

        Assert.Equal(7, changelist?.Changes.Count);
    }

    [Fact(DisplayName = "Test to get the linux installation script")]
    public async void Test_Install_Linux()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        var responseMessage = await client.GetAsync("/installer/linux");

        Assert.True(responseMessage.IsSuccessStatusCode);
        Assert.Equal("application/x-sh", responseMessage.Content.Headers.ContentType?.ToString());
    }

    [Fact(DisplayName = "Test to throw an exception if the installation architectur is unkown")]
    public async void Test_Install_Unknown()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        await Assert.ThrowsAsync<Exception>(async () => await client.GetAsync("/installer/error"));
    }

    [Fact(DisplayName = "Test there are no changed files to archive")]
    public async void Test_Files_NoChanges()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        Manifest? currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/files/zip"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var responseMessage = await client.SendAsync(requestMessage);

        Assert.Equal(HttpStatusCode.NoContent, responseMessage.StatusCode);
    }

    [Fact(DisplayName = "Test there are changed files to archive")]
    public async void Test_Files_Ok()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        Manifest? currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");
        currentManifest?.Files.RemoveAt(0);

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/files/zip"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var responseMessage = await client.SendAsync(requestMessage);

        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
    }

    [Fact(DisplayName = "Test to throw an exception on unsupported archive types")]
    public async void Test_Files_NotImplemented()
    {
        await using var server = new ServerApplication();
        var client = server.CreateClient();

        Manifest? currentManifest = await client.GetFromJsonAsync<Manifest>("/manifest");
        currentManifest?.Files.RemoveAt(0);

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri("/files/tar"),
            Content = new StringContent(JsonConvert.SerializeObject(currentManifest), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        await Assert.ThrowsAsync<NotImplementedException>(async () => await client.SendAsync(requestMessage));
    }
}
