# qBittorrentCleanup

Small utility to delete torrents(and files) from qBittorrent.

USAGE:
```
  --not-dry-run: Go ahead and delete stuff
          -days: How many days back to check (Default: 100)
 --not-only-rar: Delete all torrents that fit the day-limit (Default: Deletes torrents with *.rar files)
--only-torrents: Delete only torrents, and not files (Default: false)
         --help: This text :)
```

You'll need to create a secrets.cs file:
```
public class Secrets
{
    public static string Hostname = *url*;
    public static string APIUsername = *username*;
    public static string APIPassword = *password*;
}
```