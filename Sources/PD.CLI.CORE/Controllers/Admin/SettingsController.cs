﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using PD.Api;
using PD.Api.DataTypes;
using PD.CLI.CORE.Core;

namespace PD.CLI.CORE.Controllers
{
    [RoutePrefix("Admin/Settings")]
    public class SettingsController : ControllerBase
    {

        private readonly IAdminApi _api;
        private readonly ILogManager _log;

        public SettingsController( IAdminApi api, ILogManager log ) : base( api ) {
            _api = api;
            _log = log;
        }

        [HttpGet]
        [Route( "GetSettings" )]
        public async Task<ISettings> GetSettings([FromUri]string key) {
            _log.Log($"Loading settings as admin[{RemoteIp}]");
            await ThrowOnBadKey(key).ConfigureAwait(false);
            return await _api.Settings.GetSettings().ConfigureAwait(false);
        }
        [HttpPost]
        [Route("SetSettings")]
        public async Task SetSettings([FromUri]string key, ISettings settings ) {
            _log.Log($"Updating settings as admin[{RemoteIp}]");
            await ThrowOnBadKey(key).ConfigureAwait(false);
            await _api.Settings.SetSettings( settings ).ConfigureAwait( false );
        }
        [HttpPost]
        [Route("SetKey")]
        public async Task SetKey([FromUri]string key, string newkey ) {
            _log.Log($"Updating key as admin[{RemoteIp}]");
            await ThrowOnBadKey(key).ConfigureAwait(false);
            await _api.Settings.SetKey( newkey ).ConfigureAwait( false );
        }
        [HttpPost]
        [Route("CheckKey")]
        public async Task<bool> CheckKey([FromUri] string key ) => await _api.Settings.CheckKey( key ).ConfigureAwait( false );

    }
}