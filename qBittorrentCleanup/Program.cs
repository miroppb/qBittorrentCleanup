using miroppb;
using Newtonsoft.Json;
using qBittorrentCleanup;
using System.Diagnostics;
using System.Text;

Dictionary<string, string> _args= new Dictionary<string, string>();
bool dry_run = true;
int days = 100;
StringBuilder text= new StringBuilder();
long sizeDeleted = 0;
bool rarFiles = true;
bool torrentsOnly = false;

int i = 0;
while (i < args.Length)
{
    if (args[i].StartsWith("--"))
    {
        _args.Add(args[i], "");
        i++;
    }
    else if (args[i].StartsWith("-"))
    {
        _args.Add(args[i], args[i+1]);
        i = i + 2;
    }
}
    

foreach (KeyValuePair<string, string> arg in _args)
{
    switch (arg.Key)
    {
        case "--not-dry-run":
            dry_run = false;
            break;
        case "-days":
            days = int.Parse(arg.Value);
            break;
        case "--not-only-rar":
            rarFiles = false;
            break;
        case "--torrents-only":
            torrentsOnly = true;
            break;
        case "--help":
            Console.WriteLine("USAGE:\n\n  --not-dry-run: Go ahead and delete stuff\n          -days: How many days back to check (Default: 100)" +
                "\n --not-only-rar: Delete all torrents that fit the day-limit (Default: Deletes torrents with *.rar files)" +
                "\n--torrents-only: Delete only torrents, and not files (Default: false)\n         --help: This text :)\n");
            Environment.Exit(0);
            break;
        case null:
            Console.WriteLine("Unknown argument used");
            break;
    }
}

API api = new(Secrets.APIUrl, Secrets.APIUsername, Secrets.APIPassword);

//actual stuff
Process GetProcess(string param)
{
    Process p = new Process();
    p.StartInfo = new ProcessStartInfo()
    {
        FileName = "qbt",
        Arguments = $"{param}",
        WindowStyle = ProcessWindowStyle.Hidden,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        WorkingDirectory = Directory.GetCurrentDirectory(),
    };
    p.OutputDataReceived += P_OutputDataReceived;
    return p;
}

void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
{
    text.Append(e.Data);
}

List<clsTorrent>? GetTorrents()
{
    string json = api.Get("torrents/info", null).Result;

    return JsonConvert.DeserializeObject<List<clsTorrent>>(json);
}

List<Content>? GetContentOfTorrent(string hash)
{
    string json = api.Post("torrents/files", new Dictionary<string, string>() { { "hash", hash } }).Result;

    return JsonConvert.DeserializeObject<List<Content>>(json);
}

List<clsTorrent>? FindTorrentsToDelete(List<clsTorrent> torrents)
{
    List<clsTorrent> result = new List<clsTorrent>();
    foreach (clsTorrent torrent in torrents)
    {
        if ((DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(torrent.added_on).DateTime).Days > days)
        {
            //check contents
            List<Content>? content = GetContentOfTorrent(torrent.hash!);
            if (rarFiles)
            {
                if (content!.Any(x => x.Name.EndsWith(".rar")))
                    result.Add(torrent);
            }
            else
                result.Add(torrent);
        }
    }
    return result;
}

void DeleteTorrent(string hash)
{
    Process p = GetProcess($"torrent delete {hash}{(!torrentsOnly ? " -f" : "")}");
    p.Start();
    p.BeginOutputReadLine();
    p.WaitForExit();

    string ret = text.ToString();
    text.Clear();
    Console.Write(ret);
}

string FormatBytes(long bytes)
{
    string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
    int i;
    double dblSByte = bytes;
    for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
    {
        dblSByte = bytes / 1024.0;
    }

    return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
}

libmiroppb.Log("Welcome to qBittorent Cleaner :)");

Console.WriteLine("Getting list of torrents...");
libmiroppb.Log("Getting list of torrents...");

List<clsTorrent>? ListOfTorrents = GetTorrents();

Console.WriteLine("Checking torrents to delete (could take a while)...");
libmiroppb.Log("Checking torrents to delete (could take a while)...");

List<clsTorrent>? TorrentsToDelete = FindTorrentsToDelete(ListOfTorrents!);

if (!dry_run)
{
    Console.WriteLine($"Preparing to delete {TorrentsToDelete!.Count} torrents");
    libmiroppb.Log($"Preparing to delete {TorrentsToDelete!.Count} torrents");
    TorrentsToDelete.ForEach(x => sizeDeleted += x.size);

    foreach (clsTorrent torrent in TorrentsToDelete!)
    {
        Console.WriteLine($"Deleting: {torrent.name}");
        libmiroppb.Log($"Deleting: {torrent.name}");

        DeleteTorrent(torrent.hash!);
    }

    Console.WriteLine($"Deleted {TorrentsToDelete!.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
    libmiroppb.Log($"Deleted {TorrentsToDelete!.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
}
else
{
    TorrentsToDelete!.ForEach(x => sizeDeleted += x.size);
    Console.WriteLine($"Would have deleted {TorrentsToDelete!.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
}


