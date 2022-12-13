using miroppb;
using Newtonsoft.Json;
using qBittorrentCleanup;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

Dictionary<string, string> _args= new Dictionary<string, string>();
bool DryRun = true;
int Days = 100;
StringBuilder text= new StringBuilder();
long sizeDeleted = 0;
bool RarFiles = true;
bool OnlyTorrents = false;

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
            DryRun = false;
            Console.WriteLine("**ACTUALLY DELETING FILES**");
            break;
        case "-days":
            Days = int.Parse(arg.Value);
            break;
        case "--not-only-rar":
            RarFiles = false;
            Console.WriteLine("Using all torrents, not just ones with .rar files");
            break;
        case "--only-torrents":
            OnlyTorrents = true;
            Console.WriteLine("Deleting only torrent files, not contents");
            break;
        case "--help":
            Console.WriteLine("USAGE:\n\n  --not-dry-run: Go ahead and delete stuff\n          -days: How many days back to check (Default: 100)" +
                "\n --not-only-rar: Delete all torrents that fit the day-limit (Default: Deletes torrents with *.rar files)" +
                "\n--only-torrents: Delete only torrents, and not files (Default: false)\n         --help: This text :)\n");
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("Unknown argument used\n");
            Console.WriteLine("USAGE:\n\n  --not-dry-run: Go ahead and delete stuff\n          -days: How many days back to check (Default: 100)" +
                "\n --not-only-rar: Delete all torrents that fit the day-limit (Default: Deletes torrents with *.rar files)" +
                "\n--only-torrents: Delete only torrents, and not files (Default: false)\n         --help: This text :)\n");
            Environment.Exit(0);
            break;
    }
}

API api = new(Secrets.Hostname, Secrets.APIUsername, Secrets.APIPassword);

//actual stuff
Console.WriteLine($"Going back {Days} days");

if (DryRun)
    Console.WriteLine("\n**NOT ACTAULLY DELETING FILES**");
Console.WriteLine();

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

List<clsTorrent> FindTorrentsToDelete(List<clsTorrent> torrents)
{
    List<clsTorrent> result = new List<clsTorrent>();
    foreach (clsTorrent torrent in torrents)
    {
        if ((DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(torrent.added_on).DateTime).Days > Days)
        {
            //check contents
            List<Content>? content = GetContentOfTorrent(torrent.hash!);
            if (RarFiles)
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

async void DeleteTorrents(List<clsTorrent> torrents, bool deleteFiles)
{
    //split into groups of 5 and then run the commands for each group "hash|hash|hash" &deleteFiles = false/true
    List<IEnumerable<clsTorrent>> listOfTorrents = new List<IEnumerable<clsTorrent>>();
    for (int i = 0; i < torrents.Count; i += 5) //split into lists of 5 each
        listOfTorrents.Add(torrents.Skip(i).Take(5));
    foreach (IEnumerable<clsTorrent> a in listOfTorrents)
    {
        List<string> names = new List<string>();
        a.ToList().ForEach(x => names.Add($"{x.name} => {FormatBytes(x.size)}"));

        Console.WriteLine($"Deleting:\n{String.Join("\n", names)}");
        string hashes = String.Join("|", a.Select(x => x.hash!).ToList());
        string res = await api.Post($"torrents/delete", new Dictionary<string, string>() { { "hashes", hashes }, { "deleteFiles", deleteFiles.ToString() } });
    }
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

List<clsTorrent> TorrentsToDelete = FindTorrentsToDelete(ListOfTorrents!);
if (TorrentsToDelete.Any())
{
    if (!DryRun)
    {
        Console.WriteLine($"Preparing to delete {TorrentsToDelete.Count} torrents");
        libmiroppb.Log($"Preparing to delete {TorrentsToDelete.Count} torrents");
        if (!OnlyTorrents) TorrentsToDelete.ForEach(x => sizeDeleted += x.size);

        DeleteTorrents(TorrentsToDelete, !OnlyTorrents);

        Console.WriteLine($"Deleted {TorrentsToDelete.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
        libmiroppb.Log($"Deleted {TorrentsToDelete.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
    }
    else
    {
        if (!OnlyTorrents) TorrentsToDelete.ForEach(x => sizeDeleted += x.size);
        Console.WriteLine($"Would have deleted {TorrentsToDelete.Count} torrents, ammounting to {FormatBytes(sizeDeleted)}");
        Console.Write("Show which ones? y/[n]");
        string r = Console.ReadLine()!;
        if (r.ToLower() == "y")
            TorrentsToDelete.ForEach(x => Console.WriteLine($"{x.name} => {FormatBytes(x.size)}"));
    }
}
else
    Console.WriteLine("There's no torrents to delete");



