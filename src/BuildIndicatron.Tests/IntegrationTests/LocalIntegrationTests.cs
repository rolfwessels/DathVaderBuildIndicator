﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BuildIndicatron.Core.Api;
using BuildIndicatron.Shared.Models;
using BuildIndicatron.Shared.Models.ApiResponses;
using BuildIndicatron.Shared.Models.Composition;
using FluentAssertions;
using NUnit.Framework;

namespace BuildIndicatron.Tests.IntegrationTests
{
    [TestFixture]
    public class LocalIntegrationTests
    {
        private IRobotApi _robotApi;
        protected string _hostApi;

        public LocalIntegrationTests()
        {
            //_hostApi = Config.Url;
            _hostApi = "http://192.168.1.14:5000/";
        }

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("Log4Net.config"));
            _robotApi = new RobotApi(_hostApi);
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion

        [Test]
        public async Task Ping()
        {
            var result = await _robotApi.Ping();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task PlayMp3File_Call_ValidResponse()
        {
            var result = await _robotApi.PlayMp3File("As_You_Wish_Sound_Effect.mp3");
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task PlayMp3File_CallOther_ValidResponse()
        {
            var result = await _robotApi.PlayMp3File("darthvader_dontmakeme.wav");
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task TextToSpeech_Call_ValidResponse()
        {
            var result = await _robotApi.TextToSpeech("nice one");
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task SetupGpIo_Call_ValidResponse()
        {
            var result = await _robotApi.GpIoSetup(18,Gpio.Out);
            result.Should().NotBeNull();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task GpIoOutput_Call_ValidResponse()
        {
            var result = await _robotApi.GpIoOutput(18, false);
            result.Should().NotBeNull();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task PassiveProcess_Call_ValidResponse()
        {
            var result = await _robotApi.PassiveProcess();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task PassiveProcess_CallToSetThePassive_ValidResponse()
        {
            var result = await _robotApi.PassiveProcess(new Passive()
            {
                Interval = 20,
                StartTime = "07:00",
                SleepTime = "20:00",
                Compositions = new List<Choreography>()
                {
                    new Choreography() {
                    Sequences = new List<Sequences>()
                        {
                            new SequencesPlaySound() {File = "darthvader_lackoffaith.wav"}
                        }   
                    }
                }
            });
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }


//        [Test]
//        public async Task PlayAudioFile()
//        {
//            var sendChoreography = _robotApi.SendChoreography(new Choreography()
//                {
//                    Sequences = new List<Sequences>()
//                        {
//                            new SequencesPlaySound() {File = "wavs/darthvader_lackoffaith.wav"}
//                        }
//                });
//
//            var result = await sendChoreography;
//            result
//        }
    }

    
}