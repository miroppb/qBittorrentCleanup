namespace qBittorrentCleanup
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class clsTorrent
    {
        public string? hash { get; set; }
        public string name { get; set; } = "";
        public string magnet_uri { get; set; } = "";
        public long size { get; set; }
        public double progress { get; set; }
        public int dlspeed { get; set; }
        public int upspeed { get; set; }
        public int priority { get; set; }
        public int num_seeds { get; set; }
        public int num_complete { get; set; }
        public int num_leechs { get; set; }
        public int num_incomplete { get; set; }
        public double ratio { get; set; }
        public int eta { get; set; }
        public string state { get; set; } = "";
        public bool seq_dl { get; set; }
        public bool f_l_piece_prio { get; set; }
        public string category { get; set; } = "";
        public string tags { get; set; } = "";
        public bool super_seeding { get; set; }
        public bool force_start { get; set; }
        public string save_path { get; set; } = "";
        public int added_on { get; set; }
        public int completion_on { get; set; }
        public string tracker { get; set; } = "";
        public int dl_limit { get; set; }
        public int up_limit { get; set; }
        public long downloaded { get; set; }
        public long uploaded { get; set; }
        public long downloaded_session { get; set; }
        public long uploaded_session { get; set; }
        public long amount_left { get; set; }
        public long completed { get; set; }
        public double ratio_limit { get; set; }
        public int seen_complete { get; set; }
        public int last_activity { get; set; }
        public int time_active { get; set; }
        public bool auto_tmm { get; set; }
        public long total_size { get; set; }
        public int seeding_time { get; set; }
        public string content_path { get; set; } = "";
        public long availability { get; set; }
        public string download_path { get; set; } = "";
        public string infohash_v1 { get; set; } = "";
        public string infohash_v2 { get; set; } = "";
        public int max_ratio { get; set; }
        public int max_seeding_time { get; set; }
        public int seeding_time_limit { get; set; }
        public int trackers_count { get; set; }
    }

    public class PieceRange
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class Content
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public double Progress { get; set; }
        public int Priority { get; set; }
        public bool IsSeeding { get; set; }
        public PieceRange? PieceRange { get; set; }
    }
}
