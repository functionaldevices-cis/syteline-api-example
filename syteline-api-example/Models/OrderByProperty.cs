using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace syteline_api_example.Models;

public class OrderByProperty
{

    public string PropertyName { get; set; }

    public bool SortDesc { get; set; }

    private string? ColumnName { get; set; }

    public string OrderBy => this.PropertyName + (this.SortDesc ? " DESC" : "");
   
    public string OrderByLabel => this.PropertyName + (this.SortDesc ? " (DESC)" : " (ASC)");

    public OrderByProperty(string PropertyName, bool SortDesc = false, string? ColumnName = null)
    {
        this.PropertyName = PropertyName;
        this.SortDesc = SortDesc;
        this.ColumnName = ColumnName;
    }

}