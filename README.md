# XADev.DataCollector

Provides an easy data collector for custom JSON data output.

Use this to produce specific workflow-based output without a mess.
## Examples:

```cs
// use as select in linq query:
var data = (
    from x in this.Context.Cases
    where x.Active == true
    select Collector.FromObject(x)
             // Include an object
            .Include("CaseType")
             // Include an object property with it's child
            .Include("Contacts.Address")
            .Append("TotalPaid", (from g 
                                  in x.Payments 
                                  select x.Amount).Sum())
            .Append("Special", false)
            .Append("Tags", (from g 
                              in x.Tags 
                              where g.Active = true 
                              select x))
            .Append("Notes", (from g 
                              in x.Notes 
                              select Collector.FromObject(x).Include("CreatedBy"))
);


```
