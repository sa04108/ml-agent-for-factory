using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merlin
{
    /// <summary>
    /// 비동기 처리를 일반화하기 위한 래핑 인터페이스
    /// </summary>
    public interface IAsyncOperation
    {
        object Requester { get; }
        bool IsDone { get; }
        float Progress { get; }
        bool Succeeded { get; }

        void Release();
    }

    public interface IAsyncOperationWrapper : IAsyncOperation
    {
        IAsyncOperationWrapper OnComplete(Action onComplete);
    }

    /// <typeparam name="O">Output: 이 비동기 작업의 결과 타입</typeparam>
    public interface IAsyncOperationWrapper<O> : IAsyncOperation
    {
        IAsyncOperationWrapper<O> OnComplete(Action<O> onComplete);
    }

    /// <summary>
    /// AsyncOperationHandle의 래핑 클래스
    /// </summary>
    public class AsyncOperationHandleWrapper : IAsyncOperationWrapper
    {
        private AsyncOperationHandle handle;

        public AsyncOperationHandleWrapper(object requester, AsyncOperationHandle handle)
        {
            Requester = requester;
            this.handle = handle;
        }

        public object Requester { get; protected set; }
        public bool IsDone => handle.IsDone;
        public float Progress => handle.PercentComplete;
        public bool Succeeded => handle.Status == AsyncOperationStatus.Succeeded;

        public void Release()
        {
            handle.Release();
        }

        public IAsyncOperationWrapper OnComplete(Action onComplete)
        {
            handle.Completed += req => onComplete();

            return this;
        }
    }

    /// <summary>
    /// AsyncOperationHandle의 래핑 클래스
    /// </summary>
    /// <typeparam name="O">Output: 이 비동기 작업의 결과 타입</typeparam>
    public class AsyncOperationHandleWrapper<O> : AsyncOperationHandleWrapper, IAsyncOperationWrapper<O>
    {
        private AsyncOperationHandle<O> handle;

        public AsyncOperationHandleWrapper(object requester, AsyncOperationHandle<O> handle)
            : base(requester, handle)
        {
            this.handle = handle;
        }

        public IAsyncOperationWrapper<O> OnComplete(Action<O> onComplete)
        {
            handle.Completed += req => onComplete(handle.Result);

            return this;
        }
    }

    /// <summary>
    /// ResourceRequest의 래핑 클래스
    /// </summary>
    /// <typeparam name="O">Output: 이 비동기 작업의 결과 타입</typeparam>
    public class ResourceRequestWrapper<O> : IAsyncOperationWrapper<O> where O : UnityEngine.Object
    {
        private ResourceRequest request;

        public ResourceRequestWrapper(object requester, ResourceRequest request)
        {
            Requester = requester;
            this.request = request;
        }

        public object Requester { get; private set; }
        public bool IsDone => request.isDone;
        public float Progress => request.progress;
        public bool Succeeded => request.asset != null;

        public void Release()
        {
            Resources.UnloadAsset(request.asset);
        }

        public IAsyncOperationWrapper<O> OnComplete(Action<O> onComplete)
        {
            request.completed += req => onComplete(request.asset as O);

            return this;
        }
    }
}