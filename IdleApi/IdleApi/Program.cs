using IdleApi.Job;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
// 如果你要监听所有网络接口（允许外部访问），可以使用：
 builder.WebHost.UseUrls("http://0.0.0.0:5000"); 
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddQuartz(q =>
{

    var jobKey = new JobKey("idle");
    q.AddJob<IdleJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("idleJob")
          .WithCronSchedule("0 0 0-8 * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


var app = builder.Build();

Console.WriteLine($"当前启动时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
