﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PD.Api.DataTypes;
using PD.CLI.CORE.Core;

namespace PD.CLI.CORE.Api {

    public interface IInternalDemonizedProcess : IRunningDemonizedProcess, IPasswordedDemonizedProcess {

        int? ProcessId { get; }

        Task Start();

        Task Stop();

        Task Restart();

        Task Hide();

        Task Show();

    }

    public class InternalDemonizedProcess : RunningDemonizedProcess, IInternalDemonizedProcess {

        private readonly SemaphoreSlim _event = new SemaphoreSlim(0,1);

        private readonly object statusLocker = new object();

        private ISettings _settings;

        private Process process;

        public InternalDemonizedProcess( ISettingsFactory settings ) { _settings = settings.Get(); }

        public override ProcessPriorityClass? CurrentPriority => IsRunning() ? process.PriorityClass : (ProcessPriorityClass?) null;

        public virtual string Key { get; set; }

        public virtual int? ProcessId => IsRunning() ? process.Id:(int?) null;

        public async Task Start() {
            if ( Status != Status.NotRunning ) return;
            lock ( statusLocker ) {
                if ( Status != Status.NotRunning ) return;
                Status = Status.Starting;
            }
            StartInternal().ConfigureAwait( false );//sic!
            lock ( statusLocker ) Status = Status.Running;
        }

        public async Task Stop() {
            if ( Status != Status.Running ) return;
            lock ( statusLocker ) {
                if ( Status != Status.Running ) return;
                Status = Status.Stopping;
            }
            await StopInternal().ConfigureAwait( false );
            lock ( statusLocker ) Status = Status.NotRunning;
        }

        public async Task Restart() {
            if ( Status != Status.Running ) return;
            lock ( statusLocker ) {
                if ( Status != Status.Running ) return;
                Status = Status.Restarting;
            }
            await StopInternal().ConfigureAwait( false );
            await StartInternal().ConfigureAwait( false );
            lock ( statusLocker ) Status = Status.Running;
        }

        
        //https://msdn.microsoft.com/en-us/library/windows/desktop/ms633548(v=vs.85).aspx

        public async Task Hide() => SendCommandToMainWindow(0);

        public async Task Show() => SendCommandToMainWindow( 5 );

        private void SendCommandToMainWindow( int nCmdShow ) {
            if ( !IsRunning() ) return;
            if ( process.MainWindowHandle == IntPtr.Zero ) return;
            ShowWindow( process.MainWindowHandle, nCmdShow );
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        private bool IsRunning() {
            return process != null && !process.HasExited;//check
        }

        private async Task StopInternal() {
            if ( !IsRunning() ) return;
            try {
                process.Kill();
            }
            catch ( Exception e ) {
                //Console.WriteLine( e );
                //todo: log
            }
        }

        private async Task StartInternal() {
            process = new Process()
            {
                StartInfo = {
                        Arguments = Arguments,
                        FileName = Path,
                        CreateNoWindow = HideOnStart,
                        WindowStyle = HideOnStart ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                        UseShellExecute = false,
                    }
            };
            for ( int i = 0; i < _settings.RestartLimit; i++ ) {
                if (Status == Status.Running) { 
                    try {
                        process.Exited += (a, b) => _event.Release();
                        await _event.WaitAsync().ConfigureAwait( false );
                        process.Start();
                    }
                    catch(Exception ex) {
                        _event.Release();//
                    }
                    if ( !Autorestart ) {
                        break;
                    }
                }
            }
        }
    }

}