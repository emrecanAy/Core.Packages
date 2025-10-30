namespace Core.CrossCuttingConcerns.Logging;
public class LogDetail
{
    public string FullName { get; set; }
    public string Method { get; set; }
    public string User { get; set; }
    public List<LogParameter> Parameters { get; set; }

    public LogDetail()
    {
        FullName = string.Empty;
        Method = string.Empty;
        User = string.Empty;
        Parameters = new List<LogParameter>();
    }
    public LogDetail(string fullName, string method, string user, List<LogParameter> parameters)
    {
        FullName = fullName;
        Method = method;
        User = user;
        Parameters = parameters;
    }
}
