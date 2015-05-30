﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PiCamCV.Common.Interfaces;
using PiCamCV.ConsoleApp.Runners.PanTilt;
using PiCamCV.ExtensionMethods;

namespace PiCamCV.Common.PanTilt.Controllers
{
    public class MotionTrackingPanTiltOutput : CameraPanTiltProcessOutput
    {
        public List<MotionSection> MotionSections { get; private set; }

        public MotionSection TargetedMotion { get;set;}

        public bool IsDetected
        {
            get { return MotionSections.Count > 0; }
        }

        public MotionTrackingPanTiltOutput()
        {
            MotionSections = new List<MotionSection>();
        }
    }

    public class MotionTrackingPanTiltController : CameraBasedPanTiltController<MotionTrackingPanTiltOutput>
    {
        public MotionDetectSettings Settings { get; set; }

        private readonly MotionDetector _motionDetector;

        public MotionTrackingPanTiltController(IPanTiltMechanism panTiltMech, CaptureConfig captureConfig)
            : base(panTiltMech, captureConfig)
        {
            _motionDetector = new MotionDetector();

            ServoSettleTime = TimeSpan.FromMilliseconds(200);
        }

        protected override MotionTrackingPanTiltOutput DoProcess(CameraProcessInput input)
        {
            var detectorInput = new MotionDetectorInput();
            detectorInput.SetCapturedImage = false;
            detectorInput.Settings = Settings;
            detectorInput.Captured = input.Captured;

            var motionOutput = _motionDetector.Process(detectorInput);

            var targetPoint = CentrePoint;
            
            if (motionOutput.IsDetected)
            {
                targetPoint = motionOutput.BiggestMotion.Region.Center();
            }
            
            var output = ReactToTarget(targetPoint);
            output.MotionSections.AddRange(motionOutput.MotionSections);

            if (motionOutput.BiggestMotion != null)
            {
                output.TargetedMotion = motionOutput.BiggestMotion;
            }

            return output;
        }

        protected override void PreServoSettle()
        {
            _motionDetector.Reset();
        }
    }
}
