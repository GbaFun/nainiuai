using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Wrap
{
    public class QueueTest
    {

        public static void Test()
        {
            // 创建一个队列，限制为每秒最多处理5个项目
            var queue = new RateControlledQueue<string>(2, TimeSpan.FromSeconds(1));

            // 订阅处理事件
            queue.ItemProcessed += (sender, item) =>
            {
                Console.WriteLine($"处理项目: {item} 在 {DateTime.Now:HH:mm:ss.fff}");
            };

            // 添加一些项目到队列
            for (int i = 1; i <= 200; i++)
            {
                queue.Enqueue($"项目 {i}");
                Console.WriteLine($"已加入队列: 项目 {i}");
                //Thread.Sleep(100); // 模拟项目到达的间隔
            }

            // 等待处理完成
            while (queue.Count > 0)
            {
                Thread.Sleep(100);
            }

            queue.Stop();

        }
    }
}
