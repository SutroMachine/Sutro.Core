using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.FunctionalTest;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Part;
using System;

namespace gsCore.FunctionalTests
{
    [TestClass]
    public class FFF_PrintTests_Matches
    {
        [TestInitialize]
        public void Initialize()
        {
#if DEBUG
            Sutro.Core.Models.Config.Debug = true;
#else
            Sutro.Core.Models.Config.Debug = false;
#endif
        }

        [TestMethod]
        public void Frustum_RepRap()
        {
            // Arrange
            var machine = MachineProfilesFactoryFFF.RepRap.CreateGeneric();
            var part = new PartProfileFFF()
            {
                GenerateSupport = false,
                ShellRandomizeStart = true,
            };
            part.CarefulExtrudeSpeed = 20 * 60;

            PartProfileFactoryFFF.ApplyMaxMachineSpeeds(part, machine);

            var print = TestRunnerFactoryFFF.CreateTestRunner("Frustum.RepRap", new PrintProfileFFF()
            {
                Machine = machine, Part = part
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }

        [TestMethod]
        public void Cube_Prusa()
        {
            // Arrange
            var machine = MachineProfilesFactoryFFF.Prusa.Create_i3Mk3();
            var part = new PartProfileFFF();
            part.GenerateSupport = false;
            part.LayerHeightMM = 0.3;
            part.RapidTravelSpeed = 4800;
            part.RapidExtrudeSpeed = 3600;
            part.CarefulExtrudeSpeed = 20 * 60;

            var print = TestRunnerFactoryFFF.CreateTestRunner("Cube.Prusa", new PrintProfileFFF()
            {
                Machine = machine, Part = part
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }

        [TestMethod]
        public void Sphere_Flashforge()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Sphere.Flashforge", new PrintProfileFFF
            {
                Machine = MachineProfilesFactoryFFF.Flashforge.CreateCreatorPro(),
                Part = { 
                    GenerateSupport = true, 
                    ZipperAlignedToPoint = true,
                    RapidExtrudeSpeed = 3600,
                    MinLayerTime = 0
                },
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }

        [TestMethod]
        public void Bunny_Printrbot()
        {
            // Arrange
            var machine = MachineProfilesFactoryFFF.Printrbot.CreatePlus();
            var part = new PartProfileFFF()
            {
                GenerateSupport = false,
            };
            part.CarefulExtrudeSpeed = 1380;
            part.RapidExtrudeSpeed = 3600;
            part.RapidTravelSpeed = 4800;
            var print = TestRunnerFactoryFFF.CreateTestRunner("Bunny.Printrbot", new PrintProfileFFF()
            {
                Machine = machine, Part = part,
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }

        [TestMethod]
        public void Benchy_Monoprice()
        {
            // Arrange
            var machine = MachineProfilesFactoryFFF.Monoprice.CreateSelectMiniV2();
            var part = new PartProfileFFF()
            {
                GenerateSupport = false,
            }; 
            PartProfileFactoryFFF.ApplyMaxMachineSpeeds(part, machine);
            part.CarefulExtrudeSpeed = 20 * 60;

            var print = TestRunnerFactoryFFF.CreateTestRunner("Benchy.Monoprice", new PrintProfileFFF()
            {
                Part = part, Machine = machine
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }

        [TestMethod]
        public void Robot_Makerbot()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Robot.Makerbot", new PrintProfileFFF()
            {
                Machine = MachineProfilesFactoryFFF.Makerbot.CreateReplicator2(),
                Part = {
                    GenerateSupport = false,
                    Shells = 1,
                    FloorLayers = 3,
                    RoofLayers = 3,
                }
            });

            // Act
            print.GenerateFile();
            var comparison = print.CompareResults();
            Console.WriteLine(comparison.GetReport());

            // Assert
            comparison.AreEquivalent.Should().BeTrue();
        }
    }
}