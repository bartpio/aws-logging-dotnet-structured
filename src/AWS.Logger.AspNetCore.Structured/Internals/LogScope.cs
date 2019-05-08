using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

namespace AWS.Logger.AspNetCore.Structured.Internals
{
    /// <summary>
    /// logscope tracker
    /// it's rather (really a lot) like https://github.com/aws/aws-logging-dotnet/blob/858b71127bd9146fe7fe75bdeca03e0874ae7027/src/AWS.Logger.AspNetCore/AWSLogScope.cs
    /// </summary>
    public class LogScope
    {
        /// <summary>
        /// operationscoped singleton via AsyncLocal
        /// </summary>
        /// <typeparam name="LogScope"></typeparam>
        /// <returns></returns>
        private static readonly AsyncLocal<LogScope> Instance = new AsyncLocal<LogScope>();

        /// <summary>
        /// category name
        /// </summary>
        /// <value></value>
        public string CategoryName { get; }
        
        /// <summary>
        /// misc state (think TState)
        /// </summary>
        /// <value></value>
        public object State { get; }

        /// <summary>
        /// Construct a new stateholder
        /// </summary>
        /// <param name="categoryName">category name</param>
        /// <param name="state">misc state (think TState)</param>
        internal LogScope(string categoryName, object state)
        {
            CategoryName = categoryName;
            State = state;
        }

        /// <summary>
        /// Provide access to parent
        /// </summary>
        /// <value></value>
        public LogScope Parent { get; private set; }

        /// <summary>
        /// Provide access to singleton
        /// </summary>
        /// <value></value>
        public static LogScope Current
        {
            set => Instance.Value = value;
            get => Instance.Value;
        }

        /// <summary>
        /// push, forming a chain of objects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IDisposable Push(string name, object state)
        {
            var current = Current;
            Current = new LogScope(name, state) { Parent = current };
            return new DisposableScope();
        }

        /// <summary>
        /// stringform
        /// </summary>
        /// <returns>the string form of the state</returns>
        public override string ToString()
        {
            return State?.ToString();
        }

        /// <summary>
        /// disposable scope
        /// </summary>
        private class DisposableScope : IDisposable
        {
            bool _isDisposed = false;

            /// <summary>
            /// effect disposal
            /// </summary>
            public void Dispose()
            {
                if (!_isDisposed)
                {
                    Current = Current.Parent;
                    this._isDisposed = true;
                }
            }
        }

        /// <summary>
        /// pull scopelist, by following the chain of objects
        /// </summary>
        /// <returns>enumerable of scopes</returns>
        public IEnumerable<object> EnumerateScopes()
        {
            var current = this;
            var sanityset = new HashSet<object>(); //sanity-set protects against circular just in case (Likely no need)
            while (current != null)
            {
                if (sanityset.Add(current))
                {
                    yield return current.State;
                }

                current = current.Parent;
            }
        }
    }
}
