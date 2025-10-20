using IdleApi.Model;
using IdleApi.Model.Exceptions;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace IdleApi.Utils
{
    public static class TaskExecutor
    {
        // 控制总并发数量的信号量
        private static readonly SemaphoreSlim _totalConcurrencySemaphore = new SemaphoreSlim(3);

        // 跟踪每个账号是否已经有任务在执行
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _accountSemaphores =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// 执行带有并发控制的异步操作（带返回值）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="accountName">账号标识</param>
        /// <param name="taskFunc">异步任务方法</param>
        /// <returns>任务结果</returns>
        public static async Task<T> ExecuteWithConcurrencyControl<T>(
            string accountName,
            Func<Task<T>> taskFunc)
        {
            // 获取总并发信号量（可能抛出异常，如线程中断）
            await _totalConcurrencySemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                // 获取或创建账号信号量
                var accountSemaphore = _accountSemaphores.GetOrAdd(accountName, _ => new SemaphoreSlim(1));

                // 获取账号信号量（可能抛出异常）
                await accountSemaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    // 执行任务并捕获异常
                    return await taskFunc().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 记录任务执行异常
                    LogError(accountName, ex);
                    throw; // 重新抛出异常，由调用方处理
                }
                finally
                {
                    // 释放账号信号量（可能抛出异常，如计数已为0）
                    try
                    {
                        accountSemaphore.Release();
                    }
                    catch (System.ArgumentOutOfRangeException aorex)
                    {
                        // 记录信号量释放异常（如重复释放）
                        LogError(accountName, aorex);
                    }

                    // 尝试移除账号信号量（避免内存泄漏）
                    _accountSemaphores.TryRemove(accountName, out _);
                }
            }
            catch (Exception ex)
            {
                // 记录总并发控制异常（如信号量被释放）
                LogError(accountName, ex);
                throw; // 重新抛出异常，由调用方处理
            }
            finally
            {
                // 释放总并发信号量（可能抛出异常，如计数已为0）
                try
                {
                    _totalConcurrencySemaphore.Release();
                }
                catch (System.ArgumentOutOfRangeException aorex)
                {
                    // 记录总并发信号量释放异常
                    LogError(accountName, aorex);
                }
            }
        }

        /// <summary>
        /// 执行带有并发控制的异步操作（无返回值）
        /// </summary>
        /// <param name="accountName">账号标识</param>
        /// <param name="taskFunc">异步任务方法</param>
        /// <returns>任务</returns>
        public static async Task ExecuteWithConcurrencyControl(
            string accountName,
            Func<Task> taskFunc)
        {
            // 将无返回值任务包装为返回 object 的任务，复用带返回值的逻辑
            await ExecuteWithConcurrencyControl<object>(accountName, async () =>
            {
                await taskFunc().ConfigureAwait(false);
                return null; // 无返回值
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 记录错误日志（示例：可根据实际需求替换为数据库/文件日志）
        /// </summary>
        private static void LogError(string accountName, Exception ex)
        {
            // 示例：使用控制台输出（实际项目中替换为日志框架，如 Serilog、NLog）
            Console.WriteLine($"【账号：{accountName}】执行任务异常：{ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"堆栈信息：{ex.StackTrace}");
            IdleException? e = ex as IdleException;

            // 示例：保存到数据库（需实现 ErrorLog 类及 DbUtil 类）
            var errorLog = new ErrorLog
            {
                AccountName = accountName,
                Msg = ex.Message
            
               
            };
            if (e != null)
            {
                if (e.Role != null)
                {
                    errorLog.RoleId = e.Role.RoleId;
                    errorLog.RoleName = e.Role.RoleName;
                    errorLog.Msg = e.Msg;
                }
            }
            DbUtil.InsertOrUpdate(errorLog);
        }
    }

   

 
}