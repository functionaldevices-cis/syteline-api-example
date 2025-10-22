using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace syteline_api_example.Models;

public class APILoadCollectionResponse : APIMethodResponse
{

    public string Config
    {
        get; set;
    }

    public List<Dictionary<string, object?>> Items
    {
        get; set;
    }

    public string Bookmark
    {
        get; set;
    }

    public bool MoreRowsExist
    {
        get; set;
    }

    public APILoadCollectionResponse(bool success, string config = "", string bookmark = "", bool moreRowsExist = false, string message = "", List<Dictionary<string, object?>>? items = null, string? batchStartTimestamp = "")
    {
        this.Config = config;
        this.Items = items ?? [];
        this.Bookmark = bookmark;
        this.MoreRowsExist = moreRowsExist;
        this.Success = success;
        this.Message = message;
        this.BatchStartTimestamp = batchStartTimestamp ?? "";
    }

}
