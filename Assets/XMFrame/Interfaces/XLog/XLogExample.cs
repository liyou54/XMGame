using XM;
using System.IO;
using UnityEngine;

namespace XM.Example
{
    /// <summary>
    /// 日志系统使用示例
    /// </summary>
    public static class XLogExample
    {
        public static void Example()
        {
            // ========== 基本日志输出 ==========
            XLog.Debug("这是一条Debug日志（灰色）");
            XLog.Info("这是一条Info日志（白色）");
            XLog.Warning("这是一条Warning日志（黄色）");
            XLog.Error("这是一条Error日志（红色）");
            XLog.Fatal("这是一条Fatal日志（深红色）");

            // ========== 格式化日志输出（1-8个参数） ==========
            // 1个参数
            XLog.DebugFormat("玩家 {0} 已登录", "张三");
            XLog.InfoFormat("当前等级: {0}", 10);

            // 2个参数
            XLog.DebugFormat("玩家 {0} 的等级是 {1}", "张三", 10);
            XLog.WarningFormat("坐标 ({0}, {1}) 超出范围", 100, 200);

            // 3个参数
            XLog.InfoFormat("玩家 {0} 在 ({1}, {2}) 位置", "张三", 10.5f, 20.3f);
            XLog.ErrorFormat("错误代码 {0}: {1} 在 {2}", 404, "未找到", "服务器");

            // 4个参数
            XLog.DebugFormat("玩家 {0} 使用技能 {1} 对 {2} 造成 {3} 点伤害", 
                "张三", "火球术", "怪物A", 150);

            // 5个参数
            XLog.InfoFormat("任务 {0}: {1} 完成进度 {2}/{3}, 奖励: {4}", 
                "任务001", "收集物品", 5, 10, "经验值100");

            // 6个参数
            XLog.DebugFormat("战斗记录: {0} 对 {1} 使用 {2}, 造成 {3} 伤害, 剩余HP: {4}/{5}", 
                "玩家A", "敌人B", "技能C", 50, 450, 500);

            // 7个参数
            XLog.InfoFormat("系统消息: {0} 在 {1} 时间执行了 {2} 操作, 参数: {3}, {4}, {5}, {6}", 
                "用户A", "2024-01-01", "购买", "物品1", "数量2", "价格100", "金币");

            // 8个参数
            XLog.DebugFormat("完整日志: 时间={0}, 用户={1}, 操作={2}, 参数1={3}, 参数2={4}, 参数3={5}, 参数4={6}, 结果={7}", 
                "2024-01-01 12:00:00", "用户A", "操作B", "值1", "值2", "值3", "值4", "成功");

            // ========== 日志级别控制 ==========
            // 设置日志级别（只输出Warning及以上级别的日志）
            XLog.CurrentLogLevel = LogLevel.Warning;
            XLog.Debug("这条Debug日志不会被输出");
            XLog.Warning("这条Warning日志会被输出");

            // 禁用日志
            XLog.Enabled = false;
            XLog.Error("这条Error日志不会被输出");

            // 重新启用日志
            XLog.Enabled = true;
            XLog.CurrentLogLevel = LogLevel.Debug;

            // ========== 颜色控制 ==========
            // 启用颜色输出（默认启用）
            XLog.EnableColor = true;
            XLog.Info("这条日志会显示颜色");

            // 禁用颜色输出
            XLog.EnableColor = false;
            XLog.Info("这条日志不会显示颜色");

            // 重新启用颜色
            XLog.EnableColor = true;

            // ========== 文件输出 ==========
            // 设置日志文件输出路径
            string logPath = Path.Combine(Application.persistentDataPath, "Logs", "game.log");
            XLog.LogFilePath = logPath;
            XLog.Info("这条日志会同时输出到控制台和文件");

            // 禁用文件输出（设置为null或空字符串）
            XLog.LogFilePath = null;
            XLog.Info("这条日志只会输出到控制台");

            // 或者设置为空字符串
            XLog.LogFilePath = "";
            XLog.Info("这条日志也只会输出到控制台");

            // ========== 多线程示例 ==========
            // 每个线程都会使用自己的StringBuilder，无需担心线程安全问题
            System.Threading.Thread thread1 = new System.Threading.Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    XLog.DebugFormat("线程1: 消息 {0}", i);
                }
            });

            System.Threading.Thread thread2 = new System.Threading.Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    XLog.InfoFormat("线程2: 消息 {0}", i);
                }
            });

            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            XLog.Info("多线程日志输出完成");
        }
    }
}
