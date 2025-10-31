using Core.CrossCuttingConcerns.Serilog.ConfigurationModels;
using Core.CrossCuttingConcerns.Serilog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;
using Serilog;

namespace Core.CrossCuttingConcerns.Serilog.Logger;
public class MsSqlLogger : LoggerServiceBase
{
    public MsSqlLogger(IConfiguration configuration)
    {
        MsSqlConfiguration logConfiguration =
           configuration.GetSection("SerilogLogConfigurations:MsSqlLogConfiguration").Get<MsSqlConfiguration>()
           ?? throw new Exception(SerilogMessages.NullOptionsMessage);

        var sinkOptions = new MSSqlServerSinkOptions()
        {
            TableName = logConfiguration.TableName,
            AutoCreateSqlTable = logConfiguration.AutoCreateSqlTable

        };

        var columnOptions = new ColumnOptions();

        global::Serilog.Core.Logger serilogConfig = new LoggerConfiguration().WriteTo
            .MSSqlServer(
            connectionString: logConfiguration.ConnectionString, 
            sinkOptions: sinkOptions, 
            columnOptions: columnOptions)
            .CreateLogger();

        Logger = serilogConfig;
    }
}
