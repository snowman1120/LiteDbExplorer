using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace LiteDbExplorer.Framework.Commands
{
    /// <summary>
    ///     This class provides a single method that allows other classes to dynamically execute
    ///     command methods on objects. The execution is performed using mechanisms that provide
    ///     better performance than using reflection.
    /// </summary>
    public static class CommandExecutionManager
    {   
        private static readonly ReaderWriterLockSlim _Lock = new ReaderWriterLockSlim();

        private static readonly Dictionary<CommandExecutionProviderKey, ICommandExecutionProvider> _ExecutionProviders
            = new Dictionary<CommandExecutionProviderKey, ICommandExecutionProvider>();

        private static object _disconnectedItemSentinelValue = null;
        
        /// <summary>
        ///     Attempts to dynamically execute the method indicated by canExecuteMethodName 
        ///     and, if necessary, the method indicated by executedMethodName on the provided
        ///     target object.
        /// </summary>
        /// <param name="target">
        ///     The object on which the command methods are to executed.
        /// </param>
        /// <param name="parameter">
        ///     The command parameter.
        /// </param>
        /// <param name="execute">
        ///     True if the execute method should be executed; otherwise only the can execute
        ///     method is called.
        /// </param>
        /// <param name="executedMethodName">
        ///     The name of the method on the target object that contains the execution logic for
        ///     the command.
        /// </param>
        /// <param name="canExecuteMethodName">
        ///     The name of the method on the target object that contains the can execute logic for
        ///     the command. 
        /// </param>
        /// <param name="canExecute">
        ///     The return value of the call to the can execute method. If canExecuteMethodName is
        ///     null, true is returned.
        /// </param>
        /// <returns>
        ///     True if the command logic was successfully executed; otherwise false.
        /// </returns>
        public static bool TryExecuteCommand(object target, object parameter, bool execute, string executedMethodName, string canExecuteMethodName, out bool canExecute)
        {
            if (target != null && !string.IsNullOrEmpty(executedMethodName))
            {
                var executionProvider = GetCommandExecutionProvider(target, canExecuteMethodName, executedMethodName);
                if (executionProvider != null)
                {
                    canExecute = executionProvider.InvokeCanExecuteMethod(target, parameter);
                    if (canExecute && execute)
                    {
                        executionProvider.InvokeExecutedMethod(target, parameter);
                    }

                    return true;
                }
            }
            canExecute = false;
            return false;
        }
        
        private static ICommandExecutionProvider GetCommandExecutionProvider(object target, string canExecuteMethodName, string executedMethodName)
        {
            if (target == _disconnectedItemSentinelValue)
            {
                return null;
            }

            var key = new CommandExecutionProviderKey(target.GetType(), canExecuteMethodName, executedMethodName);
            ICommandExecutionProvider executionProvider = null;
            _Lock.EnterUpgradeableReadLock();
            try
            {
                if (!_ExecutionProviders.TryGetValue(key, out executionProvider))
                {
                    _Lock.EnterWriteLock();
                    try
                    {
                        if (!_ExecutionProviders.TryGetValue(key, out executionProvider))
                        {
                            var executionProviderType = typeof(CommandExecutionProvider<>).MakeGenericType(key.TargetType);
                            var executionProviderCtor = executionProviderType.GetConstructor(new Type[] { typeof(Type), typeof(string), typeof(string) });
                            try
                            {
                                executionProvider = (ICommandExecutionProvider)executionProviderCtor.Invoke(new object[] { key.TargetType, key.CanExecuteMethodName, key.ExecutedMethodName });
                            }
                            catch (TargetInvocationException)
                            {
                                //
                                // Thanks to Mark Bergan for finding this issue!
                                // Unfortunately we have some nastiness around a performance optimization in C# 4.0.
                                // Because we are listening to DataContext events we may end up being provided a DataContext
                                // value that is an internal place holder for the DataContext of disconnected containers WPF.
                                // There is no easy way to detect this object, hence the reflection. Basically we just want to
                                // ignore any disconnected DataContext items. The best documentation I have found is located in
                                // Answer 10 on the forum located here:
                                // http://www.go4answers.com/Example/disconnecteditem-causing-it-115624.aspx

                                if (_disconnectedItemSentinelValue == null)
                                {
                                    var targetType = target.GetType();
                                    if (targetType.FullName == "MS.Internal.NamedObject")
                                    {
                                        var nameField = targetType.GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic);
                                        if (nameField != null)
                                        {
                                            if ((string)nameField.GetValue(target) == "DisconnectedItem")
                                            {
                                                _disconnectedItemSentinelValue = target;
                                            }
                                        }
                                    }
                                }
                                if (target != _disconnectedItemSentinelValue)
                                {
                                    throw;
                                }
                            }

                            _ExecutionProviders.Add(key, executionProvider);
                        }
                    }
                    finally
                    {
                        _Lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _Lock.ExitUpgradeableReadLock();
            }
            return executionProvider;
        }

        /// <summary>
        ///     Represents a unique combination of target object Type, can execute name and executed
        ///     method name. This key is used to cache ICommandExecutionProvider implementations
        ///     that are specifically tailored to the combination these three values.
        /// </summary>
        private struct CommandExecutionProviderKey
        {
            public Type TargetType { get; private set; }

            public string CanExecuteMethodName { get; private set; }

            public string ExecutedMethodName { get; private set; }

            public CommandExecutionProviderKey(Type targetType, string canExecuteMethodName, string executedMethodName)
                : this()
            {
                TargetType = targetType;
                CanExecuteMethodName = canExecuteMethodName;
                ExecutedMethodName = executedMethodName;
            }
        }

        /// <summary>
        ///     Represents an object that is capable of executing a specific CanExecute method and
        ///     Execute method for a specific Type on any object of the specific type.
        /// </summary>
        private interface ICommandExecutionProvider
        {
            Type TargetType { get; }

            string CanExecuteMethodName { get; }

            string ExecutedMethodName { get; }

            bool InvokeCanExecuteMethod(object target, object parameter);

            void InvokeExecutedMethod(object target, object parameter);
        }

        /// <summary>
        ///     Represents an object that is capable of executing a specific CanExecute method and
        ///     Execute method for a specific type on any object of the specific type.
        /// </summary>
        /// <typeparam name="TTarget">The target Type</typeparam>
        private class CommandExecutionProvider<TTarget> : ICommandExecutionProvider
        {
            //
            // Because the method signatures are well-known, unbounded delegates can be used to
            // execute the command methods. Unbounded delegates have a significant performance
            // advantage over "standard" reflection method calls.
            private delegate void ExecutedDelegate(TTarget @target);
            private delegate void ExecutedWithParamDelegate(TTarget @target, object parameter);
            private delegate bool CanExecuteDelegate(TTarget @target);
            private delegate bool CanExecuteWithParamDelegate(TTarget @target, object parameter);

            private readonly CanExecuteDelegate _canExecute;
            private readonly CanExecuteWithParamDelegate _canExecuteWithParam;
            private readonly ExecutedDelegate _executed;
            private readonly ExecutedWithParamDelegate _executedWithParam;

            public Type TargetType { get; private set; }

            public string CanExecuteMethodName { get; private set; }

            public string ExecutedMethodName { get; private set; }

            public bool InvokeCanExecuteMethod(object target, object parameter)
            {
                if (_canExecute != null)
                {
                    return _canExecute((TTarget)target);
                }
                
                if (_canExecuteWithParam != null)
                {
                    return _canExecuteWithParam((TTarget)target, parameter);
                }

                return false;
            }

            public void InvokeExecutedMethod(object target, object parameter)
            {
                if (_executed != null)
                {
                    _executed((TTarget)target);
                }
                else
                {
                    _executedWithParam?.Invoke((TTarget)target, parameter);
                }
            }

            public CommandExecutionProvider(Type targetType, string canExecuteMethodName, string executedMethodName)
            {
                TargetType = targetType;
                CanExecuteMethodName = canExecuteMethodName;
                ExecutedMethodName = executedMethodName;

                var canExecuteMethodInfo = GetMethodInfo(CanExecuteMethodName);
                if (canExecuteMethodInfo != null && canExecuteMethodInfo.ReturnType == typeof(bool))
                {
                    if (canExecuteMethodInfo.GetParameters().Length == 0)
                    {
                        _canExecute = (CanExecuteDelegate)
                            Delegate.CreateDelegate(typeof(CanExecuteDelegate), null, canExecuteMethodInfo);
                    }
                    else
                    {
                        _canExecuteWithParam = (CanExecuteWithParamDelegate)
                            Delegate.CreateDelegate(typeof(CanExecuteWithParamDelegate), null, canExecuteMethodInfo);
                    }
                }
                if (_canExecute == null && _canExecuteWithParam == null)
                {
                    throw new Exception(
                        $"Method {CanExecuteMethodName} on type {typeof(TTarget)} does not have a valid method signature. The method must have one of the following signatures: 'public bool CanExecute()' or 'public bool CanExecute(object parameter)'");
                }

                var executedMethodInfo = GetMethodInfo(ExecutedMethodName);
                if (executedMethodInfo != null && executedMethodInfo.ReturnType == typeof(void))
                {
                    if (executedMethodInfo.GetParameters().Length == 0)
                    {
                        _executed = (ExecutedDelegate)
                            Delegate.CreateDelegate(typeof(ExecutedDelegate), null, executedMethodInfo);
                    }
                    else
                    {
                        _executedWithParam = (ExecutedWithParamDelegate)
                            Delegate.CreateDelegate(typeof(ExecutedWithParamDelegate), null, executedMethodInfo);
                    }
                }

                if (_executed == null && _executedWithParam == null)
                {
                    throw new Exception(
                        $"Method {ExecutedMethodName} on type {typeof(TTarget)} does not have a valid method signature. The method must have one of the following signatures: 'public void Executed()' or 'public void Executed(object parameter)'");
                }
            }

            private MethodInfo GetMethodInfo(string methodName)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    return null;
                }

                return typeof(TTarget).GetMethod(methodName, new Type[] { typeof(object) })
                    ?? typeof(TTarget).GetMethod(methodName, new Type[0]);
            }
        }
    }
}
