using Api.Utils;
using Microsoft.AspNetCore;

var builder = WebHost.CreateDefaultBuilder(args)
    .UseStartup<Startup>();
builder.Build().Run();
