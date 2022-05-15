using WebInstaller.Model;

namespace WebInstaller
{
	public class ManifestHelper
	{
		public Manifest Load(string files)
        {
			string path = Path.GetFullPath(files);

			if (!Directory.Exists(path))
				throw new Exception("Files directory does not exist");

			Manifest manifest = new();
            manifest.Files = new List<Model.File>();

            foreach (string file in GetFiles(path))
            {
                FileInfo fileInfo = new(file);
                Model.File manifestFile = new(fileInfo.FullName[path.Length..], fileInfo.Length, fileInfo.LastWriteTime);
                manifest.Files.Add(manifestFile);
            }

			return manifest;
        }

        public Changeset Compare(Manifest oldManifest, string files)
        {
            if (oldManifest == null)
                oldManifest = new();

            if (oldManifest.Files == null)
                oldManifest.Files = new List<Model.File>();

            Manifest newManifest = Load(files);

            Changeset changeset = new();
            changeset.Changes = new List<FileChange>();

            IEnumerable<FileChange> modified = newManifest.Files.
                Where(w =>
                    oldManifest.Files.Any(a => a.FullName.Equals(w.FullName) && a.Size != w.Size | a.DateOfModification != w.DateOfModification)
                )
                .Select(s => new FileChange(s.FullName, s.Size, s.DateOfModification, ChangeType.M));

            IEnumerable<FileChange> added    = newManifest.Files.Where(w => !oldManifest.Files.Any(a => a.FullName == w.FullName))
                .Select(s => new FileChange(s.FullName, s.Size, s.DateOfModification, ChangeType.A));

            IEnumerable<FileChange> deleted  = oldManifest.Files.Where(w => !newManifest.Files.Any(a => a.FullName == w.FullName))
                .Select(s => new FileChange(s.FullName, s.Size, s.DateOfModification, ChangeType.D));

            ((List<FileChange>)changeset.Changes).AddRange(modified);
            ((List<FileChange>)changeset.Changes).AddRange(added);
            ((List<FileChange>)changeset.Changes).AddRange(deleted);

            return changeset;
        }

        // https://stackoverflow.com/questions/929276/how-to-recursively-list-all-the-files-in-a-directory-in-c
        static IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}
