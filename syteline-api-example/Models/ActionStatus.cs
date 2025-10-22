using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace syteline_api_example.Models;
public class ActionStatus
{
    public int CountTotal
    {
        get; set;
    }

    public int CountCompleted
    {
        get; set;
    }

    public int CountBatch
    {
        get; set;
    }

    public bool Success
    {
        get; set;
    }

    public string Status
    {
        get; set;
    }

    public string ErrorCode
    {
        get; set;
    }

    public string ErrorMessage
    {
        get; set;
    }

    public string? BatchStartTimestamp
    {
        get; set;
    }

    public List<Dictionary<string, string>> RecordDetails
    {
        get; set;
    }
    public string CompletionAmount => (this.Status) switch
    {
        "Complete" => "",
        "Initializing" => "",
        _ => (this.CountTotal > 0 ? $" ({Math.Round((((decimal)this.CountCompleted / this.CountTotal) * 100), 2)}%)" : $" ({this.CountCompleted} records)"),
    };

    public ActionStatus(int CountTotal = 0, int CountCompleted = 0, int CountBatch = 0, bool Success = false, string Status = "", string ErrorCode = "", string ErrorMessage = "", List<Dictionary<string, string>>? RecordDetails = null, string? BatchStartTimestamp = "")
    {
        this.CountTotal = CountTotal;
        this.CountCompleted = CountCompleted;
        this.CountBatch = CountBatch;
        this.Success = Success;
        this.Status = Status;
        this.ErrorCode = ErrorCode;
        this.ErrorMessage = ErrorMessage;
        this.RecordDetails = RecordDetails ?? [];
        this.BatchStartTimestamp = BatchStartTimestamp;
    }

}
