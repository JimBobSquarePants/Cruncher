// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncHelper.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides helper methods to call awaitable methods from a synchronous environment.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides helper methods to call awaitable methods from a synchronous environment.
    /// <see href="http://stackoverflow.com/a/25097498/427899"/>
    /// </summary>
    internal static class AsyncHelper
    {
        /// <summary>
        /// The task factory.
        /// </summary>
        private static readonly TaskFactory MyTaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        /// <summary>
        /// Executes an async <see cref="Task{T}"/> method which has a TResult return type synchronously
        /// </summary>
        /// <param name="func">
        /// The <see cref="Func{T}"/> delegate to run.
        /// </param>
        /// <typeparam name="TResult">
        /// <see cref="Type"/> that is the result of the task.
        /// </typeparam>
        /// <returns>
        /// The <see cref="TResult"/>.
        /// </returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return MyTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        /// <summary>
        /// Executes an async <see cref="Task{T}"/> method which has a void return value synchronously.
        /// </summary>
        /// <param name="func">
        /// The <see cref="Func{T}"/> delegate to run.
        /// </param>
        public static void RunSync(Func<Task> func)
        {
            MyTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
