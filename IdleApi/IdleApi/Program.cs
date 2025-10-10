using FreeSql;
using FreeSql.Sqlite;
using IdleApi.Job;
using IdleApi.Service;
using IdleApi.Service.Interface;
using IdleAuto.Db;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
// �����Ҫ������������ӿڣ������ⲿ���ʣ�������ʹ�ã�
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

#region ע��һЩ����
//builder.Services.AddTransient<IEquip, EquipService>();
#endregion

// �� Program.cs �� Startup.cs ������



// ע��Ϊ��������
builder.Services.AddSingleton<IFreeSql>(FreeDb.Sqlite);




AppInitData.LoadData();
var app = builder.Build();

Console.WriteLine($"��ǰ����ʱ��:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
