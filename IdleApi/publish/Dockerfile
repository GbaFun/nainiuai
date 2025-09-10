# 使用 .NET 8 的 ASP.NET Core 运行时镜像（官方维护，轻量，已包含运行时）
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# 设置工作目录
WORKDIR /app

# 将发布文件夹中的内容复制到容器内的 /app
COPY ./publish/ .

# 暴露端口（根据你的应用实际使用情况，比如 80 或 5000 / 8080）
EXPOSE 5000
# 设置容器启动时运行的命令
ENTRYPOINT ["dotnet", "IdleApi.dll"]