using System;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebInstaller.Model;

namespace WebInstaller
{
	public class FileHelper
	{
		public async Task<byte[]?> Files(Manifest oldManifest, string archiveType, string files)
        {
			files = Path.GetFullPath(files);
			ManifestHelper manifestHelper = new ManifestHelper();
			Manifest newManifest = manifestHelper.Load(files);
			Changeset changeset = manifestHelper.Compare(oldManifest, files);

			if (changeset.Changes.Count == 0)
				return null;

			if (archiveType.Equals("zip"))
				return await CreateZip(changeset, files, newManifest);
			else if (archiveType.Equals("tar"))
				return await CreateTar(changeset, files);
			else
				throw new Exception("Unknown archive type");
        }

		protected async Task<byte[]> CreateZip(Changeset changeset, string files, Manifest newManifest)
        {
			MemoryStream memoryStream = new();

			// New and modified files
			ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create);
			foreach (FileChange fileChange in changeset.Changes.Where(
				w => w.ChangeType.Equals(ChangeType.A)
				|| w.ChangeType.Equals(ChangeType.M)))
            {
				string path = files + fileChange.FullName;
				ZipArchiveEntry zipArchiveEntry =
					zipArchive.CreateEntryFromFile(path, fileChange.FullName, CompressionLevel.Fastest);
			}
			// New Manifest
			ZipArchiveEntry manifestEntry = zipArchive.CreateEntry("/manifest.json", CompressionLevel.Fastest);
			using (Stream entryStream = manifestEntry.Open())
            {
				using MemoryStream memStream = new(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newManifest)));
				await memStream.CopyToAsync(entryStream);
            }
			manifestEntry.ExternalAttributes = manifestEntry.ExternalAttributes | (Convert.ToInt32("664", 8) << 16);
			// Files to delete
			ZipArchiveEntry deleteEntry = zipArchive.CreateEntry("/removed.json", CompressionLevel.Fastest);
			using (Stream entryStream = deleteEntry.Open())
            {
				Changeset deleteChangeset = new() { Changes = new List<FileChange>() };
				((List<FileChange>)deleteChangeset.Changes).AddRange(changeset.Changes.Where(w => w.ChangeType.Equals(ChangeType.D)));
				using MemoryStream memStream = new(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deleteChangeset)));
				await memStream.CopyToAsync(entryStream);
			}
			deleteEntry.ExternalAttributes = deleteEntry.ExternalAttributes | (Convert.ToInt32("664", 8) << 16);

			zipArchive.Dispose();

			return memoryStream.ToArray();
		}

		protected async Task<byte[]> CreateTar(Changeset changeset, string files)
        {
			throw new NotImplementedException();
        }
	}
}
