# 使用 .NET 8 ASP.NET Core 官方运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# 设置时区为中国上海（东八区）
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
    echo $TZ > /etc/timezone

# 设置工作目录
WORKDIR /app

# 复制发布内容
COPY . .

# 暴露端口
EXPOSE 5000

# 启动应用
ENTRYPOINT ["dotnet", "IdleApi.dll"]