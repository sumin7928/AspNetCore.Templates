using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ApiWebServer.Common.TaskManager
{
    public class TaskPool
    {
        private List<Task<bool>> _taskList;
        private ILogger _logger;

        public TaskPool( ILogger logger )
        {
            _taskList = new List<Task<bool>>();
            _logger = logger;
        }

        public void AddTask( Task<bool> task )
        {
            _taskList.Add(task);
        }

        public bool WaitAll(int timeoutMilliSeconds = 2500)
        {
            CancellationTokenSource TokenSource = new CancellationTokenSource(timeoutMilliSeconds);
            bool bResult = true;
            if (_taskList.Count == 0)
            {
                return bResult;
            }

            try
            {
                Task.WaitAll(_taskList.ToArray(), TokenSource.Token);
            }
            catch (AggregateException e)
            {
                // Display information about each exception. 

                StringBuilder ErrorSB = new StringBuilder("TaskPool AggregateException thrown with the following inner exceptions: ");

                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        ErrorSB.AppendFormat(" TaskCanceledException: Task {0}", ((TaskCanceledException)v).Task.Id);
                    else
                        ErrorSB.AppendFormat(" Exception: {0}", v.GetType().Name);
                }

                _logger.LogError(ErrorSB.ToString());
                bResult = false;
            }
            catch (OperationCanceledException ex)
            {
                // Task was canceled while running.
                _logger.LogWarning("TaskPool OperationCanceledException : {0}", ex.Message);
                bResult = false;
            }
            finally
            {
                TokenSource.Dispose();

                foreach (var task in _taskList)
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        task.Dispose();
                    }
                }
            }

            return bResult;
        }

        /*TaskPool Controller에서 사용방법
        TaskPool taskManager = new TaskPool(log);
        taskManager.AddTask(Task.Factory.StartNew(() => gameDB.USP_GS_GM_SINGLE_SEASON_INFO_R1(webSession.TokenInfo.Pcid, out singleInfo)));
        taskManager.AddTask(Task.Factory.StartNew(() => gameDB.USP_GS_GM_SINGLE_SEASON_INFO_R1(webSession.TokenInfo.Pcid, out singleInfo)));
        if (false == taskManager.WaitAll())
        {
            return webService.End(ErrorCode.ERROR_DB);
        }*/
}
}
