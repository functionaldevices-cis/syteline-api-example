using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace syteline_api_example.Models;

public class APIMethodResponse
{
    public bool Success
    {
        get; set;
    } = false;

    public string Message
    {
        get; set;
    } = "";

    public string BatchStartTimestamp
    {
        get; set;
    } = "";

}
