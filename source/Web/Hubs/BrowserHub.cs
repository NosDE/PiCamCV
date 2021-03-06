﻿using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.AspNet.SignalR;
using PiCam.Web.Controllers;
using PiCam.Web.Models;
using PiCamCV.Common.PanTilt.Controllers;
using PiCamCV.ConsoleApp.Runners.PanTilt;
using MessageBus = Microsoft.AspNet.SignalR.Messaging.MessageBus;

namespace Web
{
    public class BrowserHub : Hub<IBrowserClient>
    {
        private PiBroker _broker;

        public BrowserHub() : this(PiBroker.Instance)
        {
            
        }

        public BrowserHub(PiBroker broker)
        {
            _broker = broker;
        }

        public override Task OnConnected()
        {
            var ip = Context.Request.Environment["server.RemoteIpAddress"]?.ToString();
            _broker.BrowserConnected(Context.ConnectionId, ip);
            return base.OnConnected();
        }

        public void ChangeSettings(SystemSettings settings)
        {
            _broker.ChangeSettings(settings);
        }

        public void SetMode(ProcessingMode mode)
        {
            _broker.SetMode(mode);
        }

        public void SendCommand(PanTiltSettingCommand command)
        {
            _broker.CameraSendCommand(command);
        }
    }
}