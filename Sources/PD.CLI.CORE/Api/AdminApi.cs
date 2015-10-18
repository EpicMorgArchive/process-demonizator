﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PD.Api;
using PD.Api.DataTypes;
using PD.CLI.CORE.Core;

namespace PD.CLI.CORE.Api {

    public class AdminApi : IAdminApi {

        public AdminApi( IAdminProcessMethods process, ISettingsMethods settings, ILogMethods log ) {
            Process = process;
            Settings = settings;
            Log = log;
        }

        public IAdminProcessMethods Process { get; }
        public ISettingsMethods Settings { get; }
        public ILogMethods Log { get; }

    }

    public class AdminProcessMethods : IAdminProcessMethods {

        private readonly ISettings _settings; 
        private readonly IProcessManager _processRepository;

        public AdminProcessMethods( ISettingsFactory settings, IProcessManager processRepository ) {
            _settings = settings.Get();
            _processRepository = processRepository;
        }

        public async Task<IEnumerable<IDemonizedProcessBase>> List() => (await _processRepository.List().ConfigureAwait(false));

        public async Task<IEnumerable<IRunningDemonizedProcess>> ListFull() => (await _processRepository.ListFull().ConfigureAwait( false ));

        public async Task Edit( IPasswordedDemonizedProcess model ) {
            if ( model == null ) {
                throw new ArgumentNullException(nameof( model ));
            }
            var existing = await _processRepository.Get( model.Id ).ConfigureAwait( false );
            //todo: update model
        }

        public async Task<int> Create( IPasswordedDemonizedProcess model ) => await _processRepository.Create(model).ConfigureAwait(false);

        public async Task<IRunningDemonizedProcess> Get( int id ) => await _processRepository.Get( id ).ConfigureAwait( false ) ;

        public async Task Start( int id ) => await(await _processRepository.Get(id).ConfigureAwait(false)).Start().ConfigureAwait(false);

        public async Task Stop( int id ) => await(await _processRepository.Get(id).ConfigureAwait(false)).Stop().ConfigureAwait(false);

        public async Task Restart( int id ) => await(await _processRepository.Get(id).ConfigureAwait(false)).Restart().ConfigureAwait(false);

        public async Task Delete( int id ) => await _processRepository.Delete( id ).ConfigureAwait( false );

        public async Task Show( int id ) => await(await _processRepository.Get(id).ConfigureAwait(false)).Show().ConfigureAwait(false);

        public async Task Hide( int id ) => await ( await _processRepository.Get( id ).ConfigureAwait( false ) ).Hide().ConfigureAwait( false );

    }

    public class SettingsMethods : ISettingsMethods {

        private ISettingsManager _repository;

        public SettingsMethods( ISettingsManager repository ) {
            _repository = repository;
        }


        public async Task<ISettings> GetSettins() => _repository.Settings;

        public async Task SetSettins( ISettings settings ) {
            if ( settings == null ) {
                throw new ArgumentNullException();
            }
            _repository.Settings = (ISettingsPassword) ( settings );//todo: actual mapping
        }

        public async Task SetKey( string key ) => _repository.Settings.Password = key;

    }

    public class LogMethods : ILogMethods {

        private ILogManager _repository;


        public LogMethods( ILogManager repository ) { _repository = repository; }


        public async Task<IEnumerable<string>> Show(int tailCount) => await _repository.Show(tailCount).ConfigureAwait(false);

    }

}