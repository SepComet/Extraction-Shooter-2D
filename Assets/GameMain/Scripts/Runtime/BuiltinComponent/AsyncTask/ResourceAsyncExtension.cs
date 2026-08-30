using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using ResourceApplySuccessEventArgs = UnityGameFramework.Runtime.ResourceApplySuccessEventArgs;
using ResourceUpdateAllCompleteEventArgs = UnityGameFramework.Runtime.ResourceUpdateAllCompleteEventArgs;
using ResourceVerifySuccessEventArgs = UnityGameFramework.Runtime.ResourceVerifySuccessEventArgs;

namespace SepCore.AsyncTask
{
    /// <summary>
    /// Resource 异步扩展方法
    /// </summary>
    public static class ResourceAsyncExtension
    {
        /// <summary>
        /// 异步等待资源更新完成
        /// </summary>
        /// <param name="resourceComponent">资源组件</param>
        /// <param name="timeout">超时时间（秒），0表示不超时</param>
        /// <returns>更新完成事件</returns>
        public static UniTask<ResourceUpdateAllCompleteEventArgs> WaitForResourceUpdateCompleteAsync(this ResourceComponent resourceComponent,
            float timeout = 0f)
        {
            return AsyncTaskHelper.WaitEventAsync<ResourceUpdateAllCompleteEventArgs>(
                ResourceUpdateAllCompleteEventArgs.EventId,
                null,
                timeout
            );
        }

        /// <summary>
        /// 异步等待资源验证完成
        /// </summary>
        /// <param name="resourceComponent">资源组件</param>
        /// <param name="timeout">超时时间（秒），0表示不超时</param>
        /// <returns>验证成功事件</returns>
        public static UniTask<ResourceVerifySuccessEventArgs> WaitForResourceVerifyCompleteAsync(this ResourceComponent resourceComponent,
            float timeout = 0f)
        {
            return AsyncTaskHelper.WaitEventAsync<ResourceVerifySuccessEventArgs>(
                ResourceVerifySuccessEventArgs.EventId,
                null,
                timeout
            );
        }

        /// <summary>
        /// 异步等待资源应用完成
        /// </summary>
        /// <param name="resourceComponent">资源组件</param>
        /// <param name="timeout">超时时间（秒），0表示不超时</param>
        /// <returns>应用成功事件</returns>
        public static UniTask<ResourceApplySuccessEventArgs> WaitForResourceApplyCompleteAsync(this ResourceComponent resourceComponent,
            float timeout = 0f)
        {
            return AsyncTaskHelper.WaitEventAsync<ResourceApplySuccessEventArgs>(
                ResourceApplySuccessEventArgs.EventId,
                null,
                timeout
            );
        }
    }
}