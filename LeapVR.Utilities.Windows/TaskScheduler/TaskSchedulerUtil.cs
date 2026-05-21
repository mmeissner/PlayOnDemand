#region Licence
/****************************************************************
 *  Filename: TaskSchedulerUtil.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Documents;
using Microsoft.Win32.TaskScheduler;
using NLog;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace LeapVR.Utilities.Windows.TaskScheduler
{
    public static partial class TaskSchedulerUtil
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool CreateUserLogOnTask(ITaskSchedulerAction action, string taskName)
        {
            string user = WindowsIdentity.GetCurrent().Name;
            Logger.Info($"Trying to create task with Name={taskName} of type={action.GetType()} under User={user} with IsAdmin={IsAdministrator()} ");
            using (TaskService ts = new TaskService())
            {
                try
                {
                    TaskFolder tf = ts.RootFolder;

                    // Create a new task definition and assign properties
                    TaskDefinition td = ts.NewTask();
                    td.Principal.UserId = user;
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    td.RegistrationInfo.Author = "LeapVR";
                    td.RegistrationInfo.Description = "Start LeapVR.Launcher on User Logon";
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.Enabled = true;
                    //td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                    td.Settings.Hidden = false;
                    td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                    td.Settings.RunOnlyIfIdle = false;
                    td.Settings.RunOnlyIfNetworkAvailable = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    //Win 7 and above settings
                    td.Principal.RunLevel = TaskRunLevel.Highest; //.LUA;
                    td.RegistrationInfo.Source = "LeapVR.Utilities.Windows";
                    td.RegistrationInfo.URI = "http://www.example.com";
                    td.RegistrationInfo.Version = new Version(1, 0);
                    td.Settings.AllowDemandStart = true;
                    td.Settings.AllowHardTerminate = true;
                    td.Settings.Compatibility = TaskCompatibility.V2;
                    td.Settings.MultipleInstances = TaskInstancesPolicy.StopExisting;
                    td.Settings.StartWhenAvailable = true;
                    td.Settings.RestartCount = 5;
                    //IMPORTANT: Interval can not be smaller than 1 Minute
                    td.Settings.RestartInterval = TimeSpan.FromSeconds(100);

                    Action taskAction = null;
                    switch (action)
                    {
                        case ExecuteAppAction executeAppAction:
                            taskAction = executeAppAction.ToExecAction();
                            break;
                    }
                    if(taskAction == null)
                    {
                        throw new ArgumentOutOfRangeException($"The action of the Type={action.GetType()} is currently not supported!");
                    }
                    // Create a trigger that fires 15 minutes after the current user logs on and
                    // then every 1000 seconds after that
                    LogonTrigger lTrigger = td.Triggers.Add(new LogonTrigger());
                    //Win 7 and above settings
                    //lTrigger.Delay = TimeSpan.FromMinutes(0);
                    lTrigger.UserId = user;
                    // Create an action which opens a log file in notepad
                    td.Actions.Add(taskAction);

                    // Register the task definition (saves it) in the security context of the
                    // interactive user
                    tf.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate,null, null,
                        TaskLogonType.InteractiveToken, null);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error during Task creation with Name={taskName}");
                    return false;
                }
            }
        }
        public static List<ITaskInfo> GetAllTaskInfos()
        {
            var retval = new List<ITaskInfo>();
            void EnumFolderTasks(TaskFolder fld)
            {
                foreach(Task task in fld.Tasks)
                    ActOnTask(task);
                foreach(TaskFolder sfld in fld.SubFolders)
                    EnumFolderTasks(sfld);
            }

            void ActOnTask(Task t) { retval.Add(new TaskInfo(t)); }
            using(TaskService ts = new TaskService())
            {
                EnumFolderTasks(ts.RootFolder);
            }
            return retval;
        }

        public static bool GetTaskInfo(string taskName,out ITaskInfo taskInfo)
        {
            taskInfo = null;
            bool EnumFolderTasks(TaskFolder fld, out ITaskInfo foundTask)
            {
                foreach(Task task in fld.Tasks)
                {
                    if(ActOnTask(task))
                    {
                        foundTask = new TaskInfo(task);
                        return true;
                    }
                }
                foreach(TaskFolder sfld in fld.SubFolders)
                {
                    if(EnumFolderTasks(sfld,out foundTask))return true;
                }

                foundTask = null;
                return false;
            }

            bool ActOnTask(Task t)
            {
                if(t.Name.Equals(taskName)) return true;
                return false;
            }
            using (TaskService ts = new TaskService())
            {
                return EnumFolderTasks(ts.RootFolder,out taskInfo);
            }
        }

        public static bool DeleteTask(ITaskInfo task)
        {
            return DeleteTask(task.Name);
        }

        public static bool DeleteTask(string taskName)
        {
            try
            {
                using(TaskService ts = new TaskService())
                {
                    // Retrieve the task, change the trigger and re-register it.
                    // A taskName by itself assumes the root folder (e.g. "MyTask")
                    // A taskName can include folders (e.g. "MyFolder\MySubFolder\MyTask")
                    Task t = ts.GetTask(taskName);
                    if(t == null) return true;
                    // Check to make sure account privileges allow task deletion
                    var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    if(!principal.IsInRole(WindowsBuiltInRole.Administrator))
                        throw new Exception(
                            $"Cannot delete task with your current identity '{identity.Name}' permissions level." +
                            "You likely need to run this application 'as administrator' even if you are using an administrator account.");

                    // Remove the task we just created
                    ts.RootFolder.DeleteTask(taskName);
                    return true;
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}