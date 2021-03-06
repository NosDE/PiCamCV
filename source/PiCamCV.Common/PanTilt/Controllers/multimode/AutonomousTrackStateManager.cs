﻿using System;
using System.Diagnostics;
using Kraken.Core;
using PiCamCV.Common.Interfaces;
using PiCamCV.ConsoleApp.Runners.PanTilt;

namespace PiCamCV.Common.PanTilt.Controllers.multimode
{
    public class AutonomousTrackStateManager : StateManager
    {
        private Stopwatch _timeSinceLastFaceSample;
        private Stopwatch _timeSinceLastColourSample;
        private Stopwatch _timeSinceLastSmoothPursuit;

        private readonly TimeSpan _sampleFaceEvery = TimeSpan.FromMilliseconds(1000);
        private readonly TimeSpan _sampleColourEvery = TimeSpan.FromMilliseconds(2000);

        private TimeSpan _nextSmoothPursuit;
        private TimeTarget _timeTarget;
        private readonly IScreen _screen;

        private Random _random;

        private AutonomousState _internalState;

        public Func<CameraProcessInput, bool> IsFaceFound { get; set; }

        public PanTiltSetting PursuitBoundaryUpper { get; set; }

        public PanTiltSetting PursuitBoundaryLower { get; set; }

        private readonly IPanTiltController _panTiltController;
        

        enum AutonomousState
        {
            Waiting
            ,SmoothPursuit
        }

        public AutonomousTrackStateManager(
            IPanTiltController panTiltController
            ,IScreen screen) : base(screen)
        {
            _screen = screen;
            _panTiltController = panTiltController;
            
            PursuitBoundaryUpper = new PanTiltSetting(40, 60);
            PursuitBoundaryLower = new PanTiltSetting(40, 60);

            Reset();
        }

        public void Reset()
        {
            _timeSinceLastColourSample = Stopwatch.StartNew();
            _timeSinceLastFaceSample = Stopwatch.StartNew();
            _timeSinceLastColourSample = Stopwatch.StartNew();
            _timeSinceLastSmoothPursuit = Stopwatch.StartNew();

            _internalState = AutonomousState.Waiting;

            _random = new Random();

            DecideNextSmoothPursuit();
        }

        private void DecideNextSmoothPursuit()
        {
            _nextSmoothPursuit = TimeSpan.FromSeconds(_random.Next(8, 20));
            var nextSmoothPursuitSpeedSeconds = _random.Next(3, 10);
            _timeSinceLastSmoothPursuit.Restart();
            _timeTarget = new TimeTarget();
            _timeTarget.Original = _panTiltController.CurrentSetting;
            _timeTarget.TimeSpan = TimeSpan.FromSeconds(nextSmoothPursuitSpeedSeconds);
            
            var nextPan = Convert.ToDecimal(_random.Next((int) PursuitBoundaryLower.PanPercent.Value, (int)PursuitBoundaryUpper.PanPercent.Value));
            var nextTilt = Convert.ToDecimal(_random.Next((int)PursuitBoundaryLower.TiltPercent.Value, (int)PursuitBoundaryUpper.TiltPercent.Value));

            _timeTarget.Target = new PanTiltSetting(nextPan, nextTilt);

        }

        public ProcessingMode AcceptInput(CameraProcessInput input)
        {
            if (_timeSinceLastFaceSample.Elapsed > _sampleFaceEvery)
            {
                if (IsFaceFound(input))
                {
                    return ProcessingMode.FaceDetection;
                }
                _timeSinceLastFaceSample.Restart();
            }

            if (_timeSinceLastColourSample.Elapsed > _sampleColourEvery)
            {
                //_screen.WriteLine("TODO: colour sample");
                _timeSinceLastColourSample.Restart();
            }

            if (_internalState == AutonomousState.Waiting && _timeSinceLastSmoothPursuit.Elapsed > _nextSmoothPursuit)
            {
                _timeTarget.Start(_panTiltController.CurrentSetting);
                _screen.WriteLine($"Starting smooth pursuit {_timeTarget.Original} to {_timeTarget.Target} over {_timeTarget.TimeSpan.ToHumanReadable()}");
                _internalState = AutonomousState.SmoothPursuit;
            }

            if (_internalState == AutonomousState.SmoothPursuit)
            {
                var nextPosition = _timeTarget.GetNextPosition();
                _panTiltController.MoveAbsolute(nextPosition);

                if (_timeTarget.Ticks % 25 == 0)
                {
                    _screen.WriteLine($"{nextPosition}");
                }

                if (_timeTarget.TimeTargetReached)
                {
                    DecideNextSmoothPursuit();
                    _screen.WriteLine("Smooth pursuit target reached");
                    _internalState = AutonomousState.Waiting;
                }
            }

            return ProcessingMode.Autonomous;
        }
    }
}