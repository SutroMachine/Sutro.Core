using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.FunctionalTest;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Info;

namespace gsCore.FunctionalTests
{
    [TestClass]
    public class FFF_PrintTests_Matches
    {
        [TestMethod]
        public void Frustum_RepRap()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Frustum.RepRap", new RepRapSettings
            {
                Part = { GenerateSupport = false }
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Cube_Prusa()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Cube.Prusa", new PrusaSettings
            {
                Part =
                {
                    GenerateSupport = false,
                    LayerHeightMM = 0.3
                }
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Sphere_Flashforge()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Sphere.Flashforge", new FlashforgeSettings
            {
                Part = { GenerateSupport = true },
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Bunny_Printrbot()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Bunny.Printrbot", new PrintrbotSettings
            {
                Part = { GenerateSupport = false },
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Benchy_Monoprice()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Benchy.Monoprice", new MonopriceSettings
            {
                Part = { GenerateSupport = false },
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Robot_Makerbot()
        {
            // Arrange
            var print = TestRunnerFactoryFFF.CreateTestRunner("Robot.Makerbot", new MakerbotSettings
            {
                Part = {
                GenerateSupport = false,
                Shells = 1,
                FloorLayers = 3,
                RoofLayers = 3,
            }
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }
    }
}